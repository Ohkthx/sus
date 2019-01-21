using System;
using System.Collections.Generic;
using System.Diagnostics;
using SUS.Objects.Items;
using SUS.Objects.Items.Consumables;
using SUS.Objects.Items.Equipment;
using SUS.Shared;

namespace SUS.Objects
{
    public enum StatCode
    {
        Strength,
        Dexterity,
        Intelligence
    }

    [Flags]
    public enum DamageTypes
    {
        None = 0,

        // Physical Damage Types
        Bludgeoning = 1,
        Piercing = 2,
        Slashing = 4,
        Physical = Bludgeoning | Piercing | Slashing,

        // Elemental Damage Types
        Fire = 8,
        Cold = 16,
        Poison = 32,
        Energy = 64,
        Elemental = Fire | Cold | Poison | Energy
    }

    public abstract class Mobile : IEntity
    {
        // Mobile Stats.
        private readonly Stopwatch m_StatTimer;
        private Dictionary<ItemLayers, Equippable> m_Equipped;
        private int m_Hits, m_Stamina, m_Mana;

        // Currently owned and equipped items.
        private Dictionary<Serial, Item> m_Items;
        private Point2D m_Location;
        private string m_Name; // Name of the mobile.
        private DamageTypes m_Resistances;

        private Dictionary<SkillName, Skill> m_Skills; // Skills possessed by the mobile.

        // Mobile Properties
        private int m_Speed = 1; // Speed that the Mobile moves at.
        private int m_Str, m_Dex, m_Int;

        private Mobile m_Target; // Current target.

        #region Contructors

        protected Mobile(MobileTypes type)
        {
            Serial = Serial.NewMobile;
            Type = type;
            IsDeleted = false;

            InitConsumables();
            m_Equipped = new Dictionary<ItemLayers, Equippable>();

            m_StatTimer = new Stopwatch();
            m_StatTimer.Start();

            InitSkills();

            World.AddMobile(this);
        }

        #endregion

        public void Delete()
        {
            if (!IsDeleted || IsPlayer) return;

            foreach (var i in Items) World.RemoveItem(i);

            World.RemoveMobile(this);
        }

        public void MoveInDirection(MobileDirections direction, int xMax, int yMax)
        {
            if (direction == MobileDirections.None || direction == MobileDirections.Nearby) return;


            // Gets a pseudo-random distance between our vision (default: 15) and 30 * Speed (default: 2 - 3)
            var distance = Utility.RandomMinMax(Vision, Vision * Speed / 2);

            // Factor in our current direction.
            while (direction > MobileDirections.None)
                foreach (MobileDirections dir in Enum.GetValues(typeof(MobileDirections)))
                {
                    if (dir == MobileDirections.None || (dir & (dir - 1)) != 0)
                        continue; // Current iteration is either 'None' or it is a combination of directions.

                    if ((direction & dir) == dir)
                    {
                        // We have found a direction that is within our current direction.
                        switch (dir)
                        {
                            case MobileDirections.North:
                                // Protect ourselves from extending beyond the coordinates we are allowed to.
                                m_Location.Y = Location.Y + distance > yMax ? yMax : Location.Y + distance;
                                break;
                            case MobileDirections.South:
                                // Protection from negative coordinate.
                                m_Location.Y = Location.Y - distance < 0 ? 0 : Location.Y - distance;
                                break;
                            case MobileDirections.East:
                                // Protect ourselves from extending beyond the coordinates we are allowed to.
                                m_Location.X = Location.X + distance > xMax ? xMax : Location.X + distance;
                                break;
                            case MobileDirections.West:
                                // Protection from negative coordinate.
                                m_Location.X = Location.X - distance < 0 ? 0 : Location.X - distance;
                                break;
                        }

                        direction &= ~dir; // Removes our value from direction.
                    }
                }
        }

        public BaseMobile Base()
        {
            return new BaseMobile(Type, Serial, Name);
        }

        #region Overrides

        public override string ToString()
        {
            var info = "                  ___________________\n" +
                       "                  [Character Profile]\n" +
                       "  + ---------------------------------------------------+\n" +
                       $"  | Character Name: {Name}\n" +
                       "  | Title: The Player\n" +
                       $"  | Location: {Region}{(Location.IsValid ? ": " + Location : string.Empty)}\n" +
                       $"  | {(this is IPlayer ? "Player ID: " + ((IPlayer) this).PlayerID + "\t" : "")}Serial: {Serial}\n" +
                       $"{(this is IDamageable ? $"  | CR: {((IDamageable) this).CR}\n" : string.Empty)}" +
                       "  |\n" +
                       "  +-[ Attributes ]\n" +
                       $"  | +-- Health: {Hits} / {HitsMax}\n" +
                       $"  | +-- Strength: {Str}\n" +
                       $"  | +-- Dexterity: {Dex}\t\tStamina: {Stamina} / {StaminaMax}\n" +
                       $"  | +-- Intelligence: {Int}\tMana: {Mana} / {ManaMax}\n" +
                       $"  |   +-- Attack: {AttackRating}\n" +
                       $"  |   +-- Defense: {ArmorClass}\n" +
                       "  |\n" +
                       "  +-[ Items ]\n" +
                       $"  | +-- Bandages: {Bandages.Amount}\t\tBandage Heal Amount: {Bandage.GetEffect(HitsMax, Skills[SkillName.Healing].Value)}\n" +
                       $"  | +-- Potions: {HealthPotions.Amount}\t\tPotion Heal Amount: {Potion.GetEffect(HitsMax)}\n" +
                       $"  | +-- Arrows: {Arrows.Amount}\t\tReagents: {0}\n" +
                       $"  | +-- Gold: {Gold.Amount}\n" +
                       $"  | +-- Weapon: {Weapon.Name}\n" +
                       $"  |   +-- Proficiency: {ProficiencyModifier}\n" +
                       "  +---------------------------------------------------+";

            return info;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 13;
                hash = hash * 7 + Serial.GetHashCode();
                hash = hash * 7 + Type.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Mobile m1, Mobile m2)
        {
            if (ReferenceEquals(m1, m2)) return true;

            return !ReferenceEquals(null, m1) && m1.Equals(m2);
        }

        public static bool operator !=(Mobile m1, Mobile m2)
        {
            return !(m1 == m2);
        }

        public override bool Equals(object value)
        {
            if (ReferenceEquals(null, value)) return false;

            if (ReferenceEquals(this, value)) return true;

            if (value.GetType() != GetType()) return false;

            return IsEqual((Mobile) value);
        }

        private bool Equals(Mobile mobile)
        {
            if (ReferenceEquals(null, mobile)) return false;

            if (ReferenceEquals(this, mobile)) return true;

            return IsEqual(mobile);
        }

        private bool IsEqual(Mobile value)
        {
            return value != null
                   && Type == value.Type
                   && Serial == value.Serial;
        }

        #region Compare to...

        public int CompareTo(IEntity other)
        {
            if (other == null) return -1;

            return Serial.CompareTo(other.Serial);
        }

        public int CompareTo(Mobile other)
        {
            return CompareTo((IEntity) other);
        }

        public int CompareTo(object other)
        {
            if (other == null || other is IEntity) return CompareTo((IEntity) other);

            return -1;
        }

        #endregion

        #endregion

        #region Getters / Setters - Basic

        public bool IsDeleted { get; protected set; }

        public Mobile Target
        {
            get => m_Target;
            set
            {
                if (value == Target) return;

                m_Target = value;
            }
        }

        public Point2D Location
        {
            get => m_Location;
            set
            {
                if (value != Location) m_Location = value;
            }
        }

        public Serial Serial { get; }

        public string Name
        {
            get => m_Name ?? "Unknown";
            protected set
            {
                if (string.IsNullOrEmpty(value)) value = "Unknown";

                m_Name = value;
            }
        }

        public Regions Region { get; set; }

        public MobileTypes Type { get; }

        public bool IsPlayer => Type == MobileTypes.Player;

        public bool Alive => Hits > 0;

        public int Vision { get; } = 15;

        public int Speed
        {
            get => m_Speed;
            protected set
            {
                if (value < 0)
                    m_Speed = 0;
                else if (value == Speed) return;

                m_Speed = value;
            }
        }

        #endregion

        #region Getters / Setters - Items

        public List<Item> Items
        {
            get
            {
                if (m_Items == null) m_Items = new Dictionary<Serial, Item>();

                var items = new List<Item>();
                foreach (var item in m_Items.Values)
                    if (item == null || item.Owner == null || item.Owner.Serial != Serial)
                        ItemRemove(item);
                    else
                        items.Add(item);

                return items;
            }
        }

        public Dictionary<ItemLayers, Equippable> Equipment =>
            m_Equipped ?? (m_Equipped = new Dictionary<ItemLayers, Equippable>());

        public virtual Weapon Weapon
        {
            get
            {
                if (Equipment.ContainsKey(ItemLayers.Bow)) return Equipment[ItemLayers.Bow] as Weapon;

                if (Equipment.ContainsKey(ItemLayers.TwoHanded)) return Equipment[ItemLayers.TwoHanded] as Weapon;

                if (Equipment.ContainsKey(ItemLayers.MainHand)) return Equipment[ItemLayers.MainHand] as Weapon;

                return new Unarmed();
            }
        }

        private Armor Armor
        {
            get
            {
                if (Equipment.ContainsKey(ItemLayers.Armor)) return Equipment[ItemLayers.Armor] as Armor;

                return new ArmorSuit(Armor.Materials.Cloth);
            }
        }

        #endregion

        #region Getters / Setters - Consumables

        protected Consumable Gold
        {
            get
            {
                Gold g;
                if ((g = FindConsumable(ConsumableTypes.Gold) as Gold) != null) return g;

                g = new Gold();
                ItemAdd(g);

                return g;
            }
            set
            {
                if (value == null) return;

                if (!(value is Gold)) return;

                m_Items[Gold.Serial] = (Gold) value;
            }
        }

        public Consumable HealthPotions
        {
            get
            {
                Potion p;
                if ((p = FindConsumable(ConsumableTypes.HealthPotion) as Potion) != null) return p;

                p = new Potion();
                ItemAdd(p);

                return p;
            }
            set
            {
                if (value == null) return;

                if (!(value is Potion)) return;

                m_Items[HealthPotions.Serial] = (Potion) value;
            }
        }

        public Consumable Bandages
        {
            get
            {
                Bandage b;
                if ((b = FindConsumable(ConsumableTypes.Bandages) as Bandage) != null) return b;

                b = new Bandage();
                ItemAdd(b);

                return b;
            }
            set
            {
                if (value == null) return;

                if (!(value is Bandage)) return;

                m_Items[Bandages.Serial] = (Bandage) value;
            }
        }

        public Consumable Arrows
        {
            get
            {
                Arrow a;
                if ((a = FindConsumable(ConsumableTypes.Arrows) as Arrow) != null) return a;

                a = new Arrow();
                ItemAdd(a);

                return a;
            }
            set
            {
                if (value == null) return;

                if (!(value is Arrow)) return;

                m_Items[Arrows.Serial] = (Arrow) value;
            }
        }

        #endregion

        #region Getters / Setters - Stats

        protected void InitStats(int rawStr, int rawDex, int rawInt)
        {
            m_Str = rawStr;
            m_Dex = rawDex;
            m_Int = rawInt;

            Hits = HitsMax;
            Stamina = StaminaMax;
            Mana = ManaMax;
        }

        protected int RawStr
        {
            get => m_Str;
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                if (m_Str == value) return;

                m_Str = value;

                if (Hits > HitsMax) Hits = HitsMax;
            }
        }

        protected virtual int Str
        {
            get
            {
                var value = m_Str;

                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                return value;
            }
            set => RawStr = value;
        }

        protected int RawDex
        {
            get => m_Dex;
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                if (m_Dex == value) return;

                m_Dex = value;

                if (Stamina > StaminaMax) Stamina = StaminaMax;
            }
        }

        protected virtual int Dex
        {
            get
            {
                var value = m_Dex;

                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                return value;
            }
            set => RawDex = value;
        }

        protected int RawInt
        {
            get => m_Int;
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                if (m_Int == value) return;

                m_Int = value;

                if (Mana > ManaMax) Mana = ManaMax;
            }
        }

        protected virtual int Int
        {
            get
            {
                var value = m_Int;

                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                return value;
            }
            set => RawInt = value;
        }

        public int Hits
        {
            get => m_Hits;
            set
            {
                if (value < 0)
                    m_Hits = 0;
                else if (value > HitsMax)
                    m_Hits = HitsMax;
                else
                    m_Hits = value;
            }
        }

        public virtual int HitsMax => 50 + Str / 2;

        protected int Stamina
        {
            private get => m_Stamina;
            set
            {
                if (value < 0)
                    value = 0;
                else if (value >= StaminaMax) value = StaminaMax;

                m_Stamina = value;
            }
        }

        protected virtual int StaminaMax => Dex;

        public int Mana
        {
            get => m_Mana;
            set
            {
                if (value < 0)
                    value = 0;
                else if (value >= ManaMax) value = ManaMax;

                m_Mana = value;
            }
        }

        protected virtual int ManaMax => Int;

        protected int StatCap { private get; set; }

        private int StatTotal => Dex + Int + Str;

        public string StatIncrease(StatCode stat)
        {
            // Do not exceed the cap.
            if (StatTotal >= StatCap)
            {
                m_StatTimer.Reset();
                return string.Empty;
            }

            if (StatTotal >= 250 && m_StatTimer.ElapsedMilliseconds >= 600000)
                m_StatTimer.Restart();
            else if (StatTotal >= 240 && m_StatTimer.ElapsedMilliseconds >= 540000)
                m_StatTimer.Restart();
            else if (StatTotal >= 230 && m_StatTimer.ElapsedMilliseconds >= 480000)
                m_StatTimer.Restart();
            else if (StatTotal >= 220 && m_StatTimer.ElapsedMilliseconds >= 420000)
                m_StatTimer.Restart();
            else if (StatTotal >= 210 && m_StatTimer.ElapsedMilliseconds >= 360000)
                m_StatTimer.Restart();
            else if (StatTotal >= 200 && m_StatTimer.ElapsedMilliseconds >= 300000)
                m_StatTimer.Restart();
            else if (StatTotal >= 175 && m_StatTimer.ElapsedMilliseconds >= 240000)
                m_StatTimer.Restart();
            else if (StatTotal >= 150 && m_StatTimer.ElapsedMilliseconds >= 180000)
                m_StatTimer.Restart();
            else if (StatTotal >= 100 && m_StatTimer.ElapsedMilliseconds >= 120000)
                m_StatTimer.Restart();
            else if (StatTotal >= 0 && m_StatTimer.ElapsedMilliseconds >= 660000)
                m_StatTimer.Restart();
            else
                return string.Empty;

            switch (stat)
            {
                case StatCode.Dexterity:
                    Dex++;
                    return "Dexterity increased by 1.";
                case StatCode.Intelligence:
                    Int++;
                    return "Intelligence increased by 1.";
                case StatCode.Strength:
                    Str++;
                    return "Strength increased by 1.";
            }

            return string.Empty;
        }

        #endregion

        #region Getters / Setters - Combat

        public virtual int AttackRating
        {
            get
            {
                var rating = 0;
                foreach (var item in Equipment)
                    if (item.Value.IsWeapon)
                        rating += item.Value.Rating;

                return rating;
            }
        }

        public virtual int ArmorClass
        {
            get
            {
                var rating = 0;
                if (Equipment.ContainsKey(ItemLayers.Armor))
                {
                    // dexModMax allows for + 3 AC to medium armor if dex is GTE 16. Default: 2, >= 16: 3.
                    var dexModMax = DexterityModifier > 15 ? 3 : 2;
                    var r = Equipment[ItemLayers.Armor].Rating;
                    if (r < 12)
                        rating += r + DexterityModifier;
                    else if (r < 16)
                        rating += r + (DexterityModifier >= dexModMax ? dexModMax : DexterityModifier);
                    else
                        rating += r;
                }

                if (Equipment.ContainsKey(ItemLayers.Offhand) && Equipment[ItemLayers.Offhand].IsArmor)
                    rating += Equipment[ItemLayers.Offhand].Rating;

                return rating > 0 ? rating : IsPlayer ? 10 + DexterityModifier : 0;
            }
        }

        protected virtual DamageTypes Resistances
        {
            get => m_Resistances;
            set
            {
                if (value == Resistances) return;

                m_Resistances = value;
            }
        }

        protected virtual int ProficiencyModifier => ConvertProficiencyScore();

        public int AbilityModifier => ConvertAbilityScore(Weapon.Stat);

        private int DexterityModifier => ConvertAbilityScore(StatCode.Dexterity);

        public int IntelligenceModifier => ConvertAbilityScore(StatCode.Intelligence);

        public int StrengthModifier => ConvertAbilityScore(StatCode.Strength);

        #endregion

        #region Skills

        public Dictionary<SkillName, Skill> Skills
        {
            get
            {
                if (m_Skills != null) return m_Skills;

                m_Skills = new Dictionary<SkillName, Skill>();
                InitSkills();

                return m_Skills;
            }
            private set
            {
                if (value == null) return;

                m_Skills = value;
            }
        }

        protected double SkillTotal
        {
            get
            {
                var value = 0.0;
                foreach (var kp in Skills) value += kp.Value.Value;

                return value;
            }
        }

        public string SkillIncrease(SkillName skill)
        {
            if (IsPlayer && SkillTotal >= 720.0) return string.Empty;

            var increase = Skills[skill].Increase();
            if (increase <= 0) return string.Empty;

            var s = Skills[skill];
            return $"{s.Name} increased by {increase:F1}.";
        }

        private void InitSkills()
        {
            if (Skills == null)
                Skills = new Dictionary<SkillName, Skill>();
            else if (Skills.Count > 0) return;

            // Iterate each of the existing skills and add it to the dictionary.
            foreach (SkillName skill in Enum.GetValues(typeof(SkillName))) Skills.Add(skill, new Skill(skill));
        }

        #endregion

        #region Items / Equippables / Consumables

        protected void EquipmentAdd(Equippable item)
        {
            if (item == null
                || !item.IsEquippable)
                return;

            item.Owner = this;
            if (ItemAdd(item)) Equip(item);
        }

        public void Equip(Equippable item)
        {
            if (item == null || !item.IsEquippable) return;

            // Equip Armor (excluding shields, those are handled with weaponry.
            if (item.IsArmor && item.Layer == ItemLayers.Armor)
            {
                m_Equipped[item.Layer] = item;
                return;
            }

            // Check to see if we need to remove our Main-Hand and Off-Hand
            if ((item.Layer & ItemLayers.TwoHanded) == ItemLayers.TwoHanded)
            {
                m_Equipped.Remove(ItemLayers.MainHand);
                m_Equipped.Remove(ItemLayers.Offhand);
                m_Equipped.Remove(ItemLayers.Bow);
            }

            if (m_Equipped.ContainsKey(ItemLayers.TwoHanded))
                m_Equipped.Remove(ItemLayers.TwoHanded);
            else if (m_Equipped.ContainsKey(ItemLayers.Bow)) m_Equipped.Remove(ItemLayers.Bow);

            m_Equipped[item.Layer] = item;
        }

        public void Unequip(Equippable item)
        {
            Unequip(item.Layer);
        }

        private void Unequip(ItemLayers item)
        {
            m_Equipped.Remove(item);
        }

        protected bool ItemAdd(Item item)
        {
            if (item == null) return false;

            item.Owner = this;
            if (!Items.Contains(item)) m_Items[item.Serial] = item;

            return true;
        }

        private void ItemRemove(Item item)
        {
            if (!Items.Contains(item)) return;

            m_Items?.Remove(item.Serial);
        }

        private void InitConsumables(int gold = 0, int potions = 0, int bandages = 0, int arrows = 0)
        {
            ItemAdd(new Gold(gold));
            ItemAdd(new Potion(potions));
            ItemAdd(new Bandage(bandages));
            ItemAdd(new Arrow(arrows));
        }

        private Consumable FindConsumable(ConsumableTypes type)
        {
            foreach (var i in Items)
                if (i.Type == ItemTypes.Consumable
                    && ((Consumable) i).ConsumableType == type)
                    return (Consumable) i;

            return null;
        }

        public int ConsumableAdd(Consumable c)
        {
            return ConsumableAdd(c.ConsumableType, c.Amount);
        }

        private int ConsumableAdd(ConsumableTypes type, int amt)
        {
            if (amt <= 0) return 0;

            int tValue;
            int tMax;
            switch (type)
            {
                case ConsumableTypes.Arrows:
                    tValue = Arrows.Amount;
                    tMax = Arrows.Maximum;
                    Arrows += amt;
                    break;
                case ConsumableTypes.HealthPotion:
                    tValue = HealthPotions.Amount;
                    tMax = HealthPotions.Maximum;
                    HealthPotions += amt;
                    break;
                case ConsumableTypes.Bandages:
                    tValue = Bandages.Amount;
                    tMax = Bandages.Maximum;
                    Bandages += amt;
                    break;
                case ConsumableTypes.Gold:
                    tValue = Gold.Amount;
                    tMax = Gold.Maximum;
                    Gold += amt;
                    break;
                default:
                    return 0;
            }

            return tValue + amt > tMax ? tMax - tValue : amt;
        }

        #endregion

        #region Combat

        public abstract int Attack();
        public abstract void Kill();
        public abstract void Resurrect();

        public int Damage(int damage, Mobile from = null)
        {
            return Damage(damage, from, false);
        }

        public int Damage(int damage, Mobile from, bool isMagical)
        {
            if (!isMagical)
                if (Armor.Weight == Weights.Heavy)
                    damage -= 3;

            // Do not allow negative damage that would otherwise heal.
            if (damage <= 0) return 0;

            var originalHp = Hits;
            if (damage > Hits)
            {
                Hits = 0;
                return originalHp; // This is the amount of damage taken (last remaining hp.)
            }

            Hits -= damage;
            return damage; // Damage taken was damage received.
        }

        public int ApplyResistance(DamageTypes damageType, int damage)
        {
            if (Resistances == DamageTypes.None) return damage;

            if (damageType == DamageTypes.None) damageType = DamageTypes.Bludgeoning;

            var armorResists = 0;
            while (damageType != DamageTypes.None)
                foreach (DamageTypes dt in Enum.GetValues(typeof(DamageTypes)))
                {
                    // Ignore values that are either 'None' or not part of the damageType.
                    if (dt == DamageTypes.None || (damageType & dt) != dt) continue;

                    // Compare the value to the Resistances.
                    if ((Resistances & dt) == dt) ++armorResists;

                    damageType &= ~dt; // Remove the value.
                }

            var dmg = (int) (damage * (1 - 0.1 * armorResists));

            return dmg < 0 ? 0 : dmg;
        }

        private int ConvertAbilityScore(StatCode stat)
        {
            var statValue = 0;
            switch (stat)
            {
                // Gets the stat that is proficient for the equipped weapon.
                case StatCode.Strength:
                    statValue = Str;
                    break;
                case StatCode.Dexterity:
                    statValue = Dex;
                    break;
                case StatCode.Intelligence:
                    statValue = Int;
                    break;
            }

            var modifier = statValue / 8 - 5;

            return modifier > 10 ? 10 : modifier;
        }

        private int ConvertProficiencyScore()
        {
            var val = ((int) Skills[Weapon.RequiredSkill].Value - 60) / 10;
            return val > 0 ? val > 6 ? 6 : val : 0;
        }

        #endregion
    }
}