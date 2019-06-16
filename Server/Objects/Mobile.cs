using System;
using System.Collections.Generic;
using System.Diagnostics;
using SUS.Server.Objects.Items;
using SUS.Server.Objects.Items.Consumables;
using SUS.Server.Objects.Items.Equipment;
using SUS.Shared;

namespace SUS.Server.Objects
{
    public enum StatCode
    {
        Strength,
        Dexterity,
        Intelligence
    }

    public enum SecondaryStatCode
    {
        Hits,
        Stamina,
        Mana
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
        // Regenerators for Stats.
        private readonly Regenerator _hitsRegenerator;
        private readonly Regenerator _manaRegenerator;
        private readonly Regenerator _staminaRegenerator;

        // Mobile Stats.
        private readonly Stopwatch _statTimer;

        private Mobile _currentTarget; // Current target.
        private Dictionary<ItemLayers, Equippable> _equipped;
        private int _hits, _stamina, _mana;

        // Currently owned and equipped items.
        private Dictionary<Serial, Item> _items;
        private Point2D _location;
        private string _name; // Name of the mobile.
        private DamageTypes _resistances;

        private Dictionary<SkillName, Skill> _skills; // Skills possessed by the mobile.

        // Mobile Properties
        private int _speed = 1; // Speed that the Mobile moves at.
        private int _str, _dex, _int;

        #region Contructors

        protected Mobile(MobileTypes type)
        {
            Serial = Serial.NewMobile;
            Type = type;
            IsDeleted = false;

            InitConsumables();
            _equipped = new Dictionary<ItemLayers, Equippable>();

            _statTimer = new Stopwatch();
            _statTimer.Start();

            _hitsRegenerator = new Regenerator(Regenerator.Speeds.Normal);
            _staminaRegenerator = new Regenerator(Regenerator.Speeds.Medium);
            _manaRegenerator = new Regenerator(Regenerator.Speeds.Medium);

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
                                _location.Y = Location.Y + distance > yMax ? yMax : Location.Y + distance;
                                break;
                            case MobileDirections.South:
                                // Protection from negative coordinate.
                                _location.Y = Location.Y - distance < 0 ? 0 : Location.Y - distance;
                                break;
                            case MobileDirections.East:
                                // Protect ourselves from extending beyond the coordinates we are allowed to.
                                _location.X = Location.X + distance > xMax ? xMax : Location.X + distance;
                                break;
                            case MobileDirections.West:
                                // Protection from negative coordinate.
                                _location.X = Location.X - distance < 0 ? 0 : Location.X - distance;
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
            get => _currentTarget;
            set
            {
                if (value == Target) return;

                _currentTarget = value;
            }
        }

        public Point2D Location
        {
            get => _location;
            set
            {
                if (value != Location) _location = value;
            }
        }

        public Serial Serial { get; }

        public string Name
        {
            get => _name ?? "Unknown";
            protected set
            {
                if (string.IsNullOrEmpty(value)) value = "Unknown";

                _name = value;
            }
        }

        public Regions Region { get; set; }

        public MobileTypes Type { get; }

        public bool IsPlayer => Type == MobileTypes.Player;

        public bool Alive => Hits > 0;

        public int Vision { get; } = 15;

        public int Speed
        {
            get => _speed;
            protected set
            {
                if (value < 0)
                    _speed = 0;
                else if (value == Speed) return;

                _speed = value;
            }
        }

        #endregion

        #region Getters / Setters - Items

        public List<Item> Items
        {
            get
            {
                if (_items == null) _items = new Dictionary<Serial, Item>();

                var items = new List<Item>();
                foreach (var item in _items.Values)
                    if (item == null || item.Owner == null || item.Owner.Serial != Serial)
                        ItemRemove(item);
                    else
                        items.Add(item);

                return items;
            }
        }

        public Dictionary<ItemLayers, Equippable> Equipment =>
            _equipped ?? (_equipped = new Dictionary<ItemLayers, Equippable>());

        public virtual Weapon Weapon
        {
            get
            {
                Weapon weapon = null;
                if (Equipment.ContainsKey(ItemLayers.Bow))
                    weapon = Equipment[ItemLayers.Bow] as Weapon;
                else if (Equipment.ContainsKey(ItemLayers.TwoHanded))
                    weapon = Equipment[ItemLayers.TwoHanded] as Weapon;
                else if (Equipment.ContainsKey(ItemLayers.MainHand))
                    weapon = Equipment[ItemLayers.MainHand] as Weapon;

                if (weapon != null)
                {
                    if (!weapon.IsBroken)
                        return weapon;

                    Unequip(weapon);
                }

                var newWeapon = new Unarmed();
                Equip(newWeapon);
                return newWeapon;
            }
        }

        private Armor Armor
        {
            get
            {
                Armor armor = null;
                if (Equipment.ContainsKey(ItemLayers.Armor))
                    armor = Equipment[ItemLayers.Armor] as Armor;

                if (armor != null)
                {
                    if (!armor.IsBroken)
                        return armor;

                    Unequip(armor);
                }

                var newArmor = FindEquippable(ItemLayers.Armor, Weights.Light) as Armor ??
                               new ArmorSuit(Armor.Materials.Cloth);
                Equip(newArmor);
                return newArmor;
            }
        }

        #endregion

        #region Getters / Setters - Consumables

        public Consumable Gold
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

                _items[Gold.Serial] = (Gold) value;
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

                _items[HealthPotions.Serial] = (Potion) value;
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

                _items[Bandages.Serial] = (Bandage) value;
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

                _items[Arrows.Serial] = (Arrow) value;
            }
        }

        #endregion

        #region Getters / Setters - Stats

        protected void InitStats(int rawStr, int rawDex, int rawInt)
        {
            _str = rawStr;
            _dex = rawDex;
            _int = rawInt;

            Hits = HitsMax;
            Stamina = StaminaMax;
            Mana = ManaMax;
        }

        protected int RawStr
        {
            get => _str;
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                if (_str == value) return;

                _str = value;

                if (Hits > HitsMax) Hits = HitsMax;
            }
        }

        protected virtual int Str
        {
            get
            {
                var value = _str;

                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                return value;
            }
            set => RawStr = value;
        }

        protected int RawDex
        {
            get => _dex;
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                if (_dex == value) return;

                _dex = value;

                if (Stamina > StaminaMax) Stamina = StaminaMax;
            }
        }

        protected virtual int Dex
        {
            get
            {
                var value = _dex;

                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                return value;
            }
            set => RawDex = value;
        }

        protected int RawInt
        {
            get => _int;
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                if (_int == value) return;

                _int = value;

                if (Mana > ManaMax) Mana = ManaMax;
            }
        }

        protected virtual int Int
        {
            get
            {
                var value = _int;

                if (value < 1)
                    value = 1;
                else if (value > 65000) value = 65000;

                return value;
            }
            set => RawInt = value;
        }

        public int Hits
        {
            get
            {
                if (_hits <= 0)
                    _hitsRegenerator.Stop();
                else if (_hits < HitsMax && !_hitsRegenerator.Running)
                    _hitsRegenerator.Restart();

                if (!_hitsRegenerator.Running) return _hits;

                // If we are currently regenerating health, retrieve our health ticks.
                RecoverStat(SecondaryStatCode.Hits, _hitsRegenerator.RetrieveTicks());
                if (_hits == HitsMax)
                    _hitsRegenerator.Stop();

                return _hits;
            }
            set
            {
                if (value < 0)
                    _hits = 0;
                else if (value > HitsMax)
                    _hits = HitsMax;
                else
                    _hits = value;
            }
        }

        public virtual int HitsMax => 50 + Str / 2;

        protected int Stamina
        {
            private get
            {
                if (Hits <= 0)
                    _staminaRegenerator.Stop();
                else if (_stamina < StaminaMax && !_staminaRegenerator.Running)
                    _staminaRegenerator.Restart();

                // Stamina regenerator is not running, return current stamina.
                if (!_staminaRegenerator.Running) return _stamina;

                // Attempt to get the banked stamina ticks and apply it.
                RecoverStat(SecondaryStatCode.Stamina, _staminaRegenerator.RetrieveTicks());
                if (_stamina == StaminaMax)
                    _staminaRegenerator.Stop();

                return _stamina;
            }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value >= StaminaMax) value = StaminaMax;

                _stamina = value;
            }
        }

        protected virtual int StaminaMax => Dex;

        public int Mana
        {
            get
            {
                if (Hits <= 0)
                    _manaRegenerator.Stop();
                else if (_mana < ManaMax && !_manaRegenerator.Running)
                    _manaRegenerator.Restart();

                // Max health and not running the mana regenerator.
                if (!_manaRegenerator.Running) return _mana;

                // Try to pull the most recent ticks and apply it to our current mana.
                RecoverStat(SecondaryStatCode.Mana, _manaRegenerator.RetrieveTicks());
                if (_mana == ManaMax)
                    _manaRegenerator.Stop();

                return _mana;
            }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value >= ManaMax) value = ManaMax;

                _mana = value;
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
                _statTimer.Reset();
                return string.Empty;
            }

            if (StatTotal >= 250 && _statTimer.ElapsedMilliseconds >= 600000)
                _statTimer.Restart();
            else if (StatTotal >= 240 && _statTimer.ElapsedMilliseconds >= 540000)
                _statTimer.Restart();
            else if (StatTotal >= 230 && _statTimer.ElapsedMilliseconds >= 480000)
                _statTimer.Restart();
            else if (StatTotal >= 220 && _statTimer.ElapsedMilliseconds >= 420000)
                _statTimer.Restart();
            else if (StatTotal >= 210 && _statTimer.ElapsedMilliseconds >= 360000)
                _statTimer.Restart();
            else if (StatTotal >= 200 && _statTimer.ElapsedMilliseconds >= 300000)
                _statTimer.Restart();
            else if (StatTotal >= 175 && _statTimer.ElapsedMilliseconds >= 240000)
                _statTimer.Restart();
            else if (StatTotal >= 150 && _statTimer.ElapsedMilliseconds >= 180000)
                _statTimer.Restart();
            else if (StatTotal >= 100 && _statTimer.ElapsedMilliseconds >= 120000)
                _statTimer.Restart();
            else if (StatTotal >= 0 && _statTimer.ElapsedMilliseconds >= 660000)
                _statTimer.Restart();
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
            get => _resistances;
            set
            {
                if (value == Resistances) return;

                _resistances = value;
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
                if (_skills != null) return _skills;

                _skills = new Dictionary<SkillName, Skill>();
                InitSkills();

                return _skills;
            }
            private set
            {
                if (value == null) return;

                _skills = value;
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

        private Equippable FindEquippable(ItemLayers layer, Weights weight)
        {
            foreach (var i in Items)
            {
                if (!(i is Equippable equipmentItem)) continue;

                if (equipmentItem.Layer == layer && equipmentItem.Weight == weight) return equipmentItem;
            }

            return null;
        }

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
                _equipped.Remove(ItemLayers.Armor);
                _equipped[item.Layer] = item;
                return;
            }

            // Check to see if we need to remove our Main-Hand and Off-Hand
            if ((item.Layer & ItemLayers.TwoHanded) == ItemLayers.TwoHanded)
            {
                _equipped.Remove(ItemLayers.MainHand);
                _equipped.Remove(ItemLayers.Offhand);
                _equipped.Remove(ItemLayers.Bow);
            }

            if (_equipped.ContainsKey(ItemLayers.TwoHanded))
                _equipped.Remove(ItemLayers.TwoHanded);
            else if (_equipped.ContainsKey(ItemLayers.Bow)) _equipped.Remove(ItemLayers.Bow);

            _equipped[item.Layer] = item;
        }

        public void Unequip(Equippable item)
        {
            if (Equipment[item.Layer] == item)
                Unequip(item.Layer);
        }

        private void Unequip(ItemLayers item)
        {
            _equipped.Remove(item);
        }

        protected bool ItemAdd(Item item)
        {
            if (item == null) return false;

            item.Owner = this;
            if (!Items.Contains(item)) _items[item.Serial] = item;

            return true;
        }

        private void ItemRemove(Item item)
        {
            if (!Items.Contains(item)) return;

            _items?.Remove(item.Serial);
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

        public void RemoveConsumable(Consumable c, int amount)
        {
            RemoveConsumable(c.ConsumableType, amount);
        }

        public void RemoveConsumable(ConsumableTypes type, int amt)
        {
            if (amt <= 0) return;

            switch (type)
            {
                case ConsumableTypes.Gold:
                    if (amt > Gold.Amount)
                        amt = Gold.Amount;
                    Gold -= amt;
                    break;
                case ConsumableTypes.Arrows:
                    if (amt > Arrows.Amount)
                        amt = Arrows.Amount;
                    Arrows -= amt;
                    break;
                case ConsumableTypes.Bandages:
                    if (amt > Bandages.Amount)
                        amt = Bandages.Amount;
                    Bandages -= amt;
                    break;
                case ConsumableTypes.HealthPotion:
                    if (amt > HealthPotions.Amount)
                        amt = HealthPotions.Amount;
                    HealthPotions -= amt;
                    break;
                default:
                    return;
            }
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
            if (!isMagical && Armor.Weight == Weights.Heavy)
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

        private int RecoverStat(SecondaryStatCode statCode, int amount, Mobile from = null)
        {
            // Anonymous function that determines how much to apply of the maximum amount.
            int AmountChanged(int min, int max)
            {
                if (amount <= 0) return 0;
                if (min + amount > max) return max - min;
                return amount;
            }

            int statDifference;
            switch (statCode)
            {
                case SecondaryStatCode.Hits:
                    statDifference = AmountChanged(_hits, HitsMax);
                    _hits += statDifference;
                    break;
                case SecondaryStatCode.Stamina:
                    statDifference = AmountChanged(_stamina, StaminaMax);
                    _stamina += statDifference;
                    break;
                case SecondaryStatCode.Mana:
                    statDifference = AmountChanged(_mana, ManaMax);
                    _mana += statDifference;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statCode), statCode,
                        "Unknown StatCode used in Recovering Stats.");
            }

            return statDifference;
        }

        /// <summary>
        ///     Heals the mobile.
        /// </summary>
        /// <param name="amount">Amount to attempt to apply to the mobile.</param>
        /// <param name="from">Optional mobile that applied the heal.</param>
        /// <returns>Amount of health actually healed.</returns>
        public int Heal(int amount, Mobile from = null)
        {
            // We can't heal the dead and we cannot heal negative or 0.
            if (Hits <= 0 || amount <= 0)
                return 0;

            return RecoverStat(SecondaryStatCode.Hits, amount, from);
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