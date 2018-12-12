using System;
using System.Collections.Generic;
using SUS.Shared.Utilities;
using SUS.Shared.Objects.Items;
using System.Diagnostics;

namespace SUS.Shared.Objects
{
    [Serializable]
    public enum StatCode
    {
        Strength,
        Dexterity,
        Intelligence
    }

    [Serializable]
    public class BasicMobile
    {
        private Guid m_Guid;
        private Serial m_ID;
        private string m_Name;
        private Mobile.Types m_Type;

        #region Constructors
        public BasicMobile(Mobile mobile) : this(mobile.Guid, mobile.ID, mobile.Type, mobile.Name) { }

        public BasicMobile(Guid guid, UInt64 id, Mobile.Types type, string name)
        {
            Guid = guid;
            ID = new Serial(id);
            Type = type;
            Name = name;
        }
        #endregion

        #region Getters/Setters
        public Guid Guid
        {
            get { return m_Guid; }
            private set
            {
                if (value == null)
                    return;
                else if (Guid == null)
                    m_Guid = value;

                if (Guid != value)
                    m_Guid = value;
            }
        }

        public Serial ID
        {
            get { return m_ID; }
            set
            {
                if (value == null)
                    return;
                else if (ID == null)
                    m_ID = value;

                if (ID != value)
                    m_ID = value;
            }
        }

        public Mobile.Types Type
        {
            get { return m_Type; }
            set
            {
                if (value == Mobile.Types.None)
                    return;
                else if (Type == Mobile.Types.None)
                    m_Type = value;

                if (Type != value)
                    m_Type = value;
            }
        }

        public string Name
        {
            get { return m_Name; }
            set
            {
                if (value == null)
                    return;
                else if (Name == null)
                    m_Name = value;

                if (Name != value)
                    m_Name = value;
            }
        }

        public bool IsPlayer { get { return Type == Mobile.Types.Player; } }
        #endregion

        #region Overrides
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_ID) ? m_ID.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_Name) ? m_Name.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_Type) ? m_Type.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(BasicMobile m1, BasicMobile m2)
        {
            if (Object.ReferenceEquals(m1, m2)) return true;
            if (Object.ReferenceEquals(null, m1)) return false;
            return (m1.Equals(m2));
        }

        public static bool operator !=(BasicMobile m1, BasicMobile m2)
        {
            return !(m1 == m2);
        }

        public override bool Equals(object value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            if (value.GetType() != this.GetType()) return false;
            return IsEqual((BasicMobile)value);
        }

        public bool Equals(BasicMobile mobile)
        {
            if (Object.ReferenceEquals(null, mobile)) return false;
            if (Object.ReferenceEquals(this, mobile)) return true;
            return IsEqual(mobile);
        }

        private bool IsEqual(BasicMobile value)
        {
            return (value != null)
                && (m_Type == value.m_Type)
                && (m_ID == value.m_ID);
        }
        #endregion
    }

    [Serializable]
    public abstract class Mobile
    {
        [Flags]
        public enum Types
        {
            None        = 0,
            Player      = 1,
            NPC         = 2,
            Creature    = 4,

            Mobile = Player | NPC | Creature,
        }

        [Flags]
        public enum Directions
        {
            None    = 0,

            North   = 1,
            South   = 2,
            East    = 4,
            West    = 8,

            Nearby  = 16,

            NorthEast = North | East,
            NorthWest = North | West,
            SouthEast = South | East,
            SouthWest = South | West,
        }

        private Coordinate m_Coord;
        private Guid m_Guid;
        private Serial m_ID;                // ID of the mobile.
        private string m_Name;              // Name of the mobile.
        private Types m_Type;               // Type of Mobile: NPC or Player.
        private Locations m_Location;       // Location of the mobile.

        // Currently owned and equipped items.
        protected Dictionary<Guid, Item> m_Items;
        private Dictionary<ItemLayers, Equippable> m_Equipped;

        // Mobile Properties
        private int m_Speed = 1;            // Speed that the Mobile moves at.
        private readonly int m_Vision = 15; // Distance the Mobile can see.

        // Mobile Stats.
        private Stopwatch m_StatTimer;
        private int m_StatCap;
        private int m_Str, m_Dex, m_Int;
        private int m_Hits, m_Stam, m_Mana;

        private Dictionary<SkillCode, Skill> m_Skills;  // Skills possessed by the mobile.

        #region Contructors
        public Mobile(Types type)
        {
            Type = type;

            InitConsumables();
            m_Equipped = new Dictionary<ItemLayers, Equippable>();

            m_StatTimer = new Stopwatch();
            m_StatTimer.Start();

            InitSkills();
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            string paperdoll = $"                  ___________________\n" +
                $"                  [Character Profile]\n" +
                $"  + ---------------------------------------------------+\n" +
                $"  | Character Name: {Name}\n" +
                $"  | Title: {"The Player"}\n" +
                $"  | Location: {Location.ToString()}\n" +
                $"  |\n" +
                $"  +-[ Attributes ]\n" +
                $"  | +-- Health: {Hits} / {HitsMax}\n" +
                $"  | +-- Strength: {Str}\n" +
                $"  | +-- Dexterity: {Dex}\t\tStamina: {Stam} / {StamMax}\n" +
                $"  | +-- Intelligence: {Int}\tMana: {Mana} / {ManaMax}\n" +
                $"  |   +-- Attack: {WeaponRating}\n" +
                $"  |   +-- Defense: {ArmorRating}\n" +
                $"  |\n" +
                $"  +-[ Items ]\n" +
                $"  | +-- Bandaids: {Bandages.Amount}\t\tBandaid Heal Amount: {Bandage.GetEffect(HitsMax)}\n" +
                $"  | +-- Potions: {HealthPotions.Amount}\t\tPotion Heal Amount: {Potion.GetEffect(HitsMax)}\n" +
                $"  | +-- Arrows: {Arrows.Amount}\t\tReagents: {0}\n" +
                $"  | +-- Gold: {Gold.Amount}\n" +
                $"  | +-- Weapon: {Weapon.Name}\n" +
                $"  +---------------------------------------------------+";

            return paperdoll;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_ID) ? m_ID.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_Name) ? m_Name.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_Type) ? m_Type.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(Mobile m1, Mobile m2)
        {
            if (Object.ReferenceEquals(m1, m2)) return true;
            if (Object.ReferenceEquals(null, m1)) return false;
            return (m1.Equals(m2));
        }

        public static bool operator !=(Mobile m1, Mobile m2)
        {
            return !(m1 == m2);
        }

        public override bool Equals(object value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            if (value.GetType() != this.GetType()) return false;
            return IsEqual((Mobile)value);
        }

        public bool Equals(Mobile mobile)
        {
            if (Object.ReferenceEquals(null, mobile)) return false;
            if (Object.ReferenceEquals(this, mobile)) return true;
            return IsEqual(mobile);
        }

        private bool IsEqual(Mobile value)
        {
            return (value != null)
                && (m_Type == value.m_Type)
                && (m_ID == value.m_ID);
        }
        #endregion

        #region Getters / Setters - Basic
        public Coordinate Coordinate
        {
            get { return m_Coord; }
            set
            {
                if (value == null)
                    return;
                else if (Coordinate == null)
                    m_Coord = value;

                if (value != Coordinate)
                    m_Coord = value;
            }
        }

        public Guid Guid
        {
            get
            {
                if (m_Guid == null || m_Guid == Guid.Empty)
                    m_Guid = Guid.NewGuid();

                return m_Guid;
            }
        }

        public Serial ID
        {
            get { return m_ID; }
            set
            {
                if (value != null && value != m_ID)
                    m_ID = value;
            }
        }

        public string Name
        {
            get
            {
                if (m_Name != null)
                    return m_Name;
                else
                    return "Unknown";
            }
            set
            {
                if (value != m_Name)
                    m_Name = value;
            }
        }
   
        public Locations Location
        {
            get { return m_Location; }
            set
            {
                if (m_Location != value)
                    m_Location = value;
            }
        }

        public Types Type
        {
            get { return m_Type; }
            private set
            {
                if (value != Types.None && value != Type)
                    m_Type = value;
            }
        }

        public bool IsPlayer { get { return m_Type == Types.Player; } }

        public bool IsDead { get { return Hits <= 0; } }

        public string GetHealth() { return $"{Hits} / {HitsMax}"; }

        public int Vision { get { return m_Vision; } }

        public int Speed
        {
            get { return m_Speed; }
            set
            {
                if (value < 0)
                    return;
                else if (value == Speed)
                    return;
                m_Speed = value;
            }
        }
        #endregion

        #region Getters / Setters - Items
        public Dictionary<Guid, Item> Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new Dictionary<Guid, Item>();
                return m_Items;
            }
        }

        public Dictionary<ItemLayers, Equippable> Equipment
        {
            get
            {
                if (m_Equipped == null)
                    m_Equipped = new Dictionary<ItemLayers, Equippable>();
                return m_Equipped;
            }
        }

        public Weapon Weapon
        {
            get
            {
                if (Equipment.ContainsKey(ItemLayers.Bow))
                    return Equipment[ItemLayers.Bow] as Weapon;
                else if (Equipment.ContainsKey(ItemLayers.TwoHanded))
                    return Equipment[ItemLayers.TwoHanded] as Weapon;
                else if (Equipment.ContainsKey(ItemLayers.MainHand))
                    return Equipment[ItemLayers.MainHand] as Weapon;
                else
                    return new Items.Equipment.Unarmed();
            }
        }
        #endregion

        #region Getters / Setters - Consumables
        public Consumable Gold 
        {
            get
            {
                Gold g;
                if ((g = FindConsumable(Consumable.Types.Gold) as Gold) == null)
                {
                    g = new Gold();
                    ItemAdd(g);
                }

                return g;
            }
            set
            {
                if (value == null)
                    return;
                else if (!(value is Gold))

                m_Items[Gold.Guid] = value as Gold;
            }
        }

        public Consumable HealthPotions
        {
            get
            {
                Potion p;
                if ((p = FindConsumable(Consumable.Types.HealthPotion) as Potion) == null)
                {
                    p = new Potion();
                    ItemAdd(p);
                }
                return p;
            }
            set
            {
                if (value == null)
                    return;
                else if (!(value is Potion))
                    return;

                m_Items[HealthPotions.Guid] = value as Potion;
            }
        }

        public Consumable Bandages
        {
            get
            {
                Bandage b;
                if ((b = FindConsumable(Consumable.Types.Bandages) as Bandage) == null)
                {
                    b = new Bandage();
                    ItemAdd(b);
                }
                return b;
            }
            set
            {
                if (value == null)
                    return;
                else if (!(value is Bandage))
                    return;

                m_Items[Bandages.Guid] = value as Bandage;
            }
        }

        public Consumable Arrows
        {
            get
            {
                Arrow a;
                if ((a = FindConsumable(Consumable.Types.Arrows) as Arrow) == null)
                {
                    a = new Arrow();
                    ItemAdd(a);
                }
                return a;
            }
            set
            {
                if (value == null)
                    return;
                if (!(value is Arrow))
                    return;

                m_Items[Arrows.Guid] = value as Arrow;
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
            Stam = StamMax;
            Mana = ManaMax;
        }

        public int RawStr
        {
            get { return m_Str; }
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 65000)
                    value = 65000;

                if (m_Str != value)
                {
                    m_Str = value;

                    if (Hits > HitsMax)
                        Hits = HitsMax;
                }
            }
        }

        public virtual int Str
        {
            get
            {
                int value = m_Str;

                if (value < 1)
                    value = 1;
                else if (value > 65000)
                    value = 65000;

                return value;
            }
            set { RawStr = value; }
        }

        public int RawDex
        {
            get { return m_Dex; }
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 65000)
                    value = 65000;

                if (m_Dex != value)
                {
                    m_Dex = value;

                    if (Stam > StamMax)
                        Stam = StamMax;
                }
            }
        }

        public virtual int Dex
        {
            get
            {
                int value = m_Dex;

                if (value < 1)
                    value = 1;
                else if (value > 65000)
                    value = 65000;

                return value;
            }
            set { RawDex = value; }
        }

        public int RawInt
        {
            get { return m_Int; }
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 65000)
                    value = 65000;

                if (m_Int != value)
                {
                    m_Int = value;

                    if (Mana > ManaMax)
                        Mana = ManaMax;
                }
            }
        }

        public virtual int Int
        {
            get
            {
                int value = m_Int;

                if (value < 1)
                    value = 1;
                else if (value > 65000)
                    value = 65000;

                return value;
            }
            set { RawInt = value; }
        }

        public int Hits
        {
            get { return m_Hits; }
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

        public virtual int HitsMax { get { return 50 + (Str / 2); } }

        public int Stam
        {
            get { return m_Stam; }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value >= StamMax)
                    value = StamMax;

                if (m_Stam != value)
                    m_Stam = value;
            }
        }

        public virtual int StamMax { get { return Dex; } }

        public int Mana
        {
            get { return m_Mana; }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value >= ManaMax)
                    value = ManaMax;

                if (m_Mana != value)
                    m_Mana = value;
            }
        }

        public virtual int ManaMax { get { return Int; } }

        public int StatCap
        {
            get { return m_StatCap; }
            set
            {
                if (m_StatCap != value)
                    m_StatCap = value;
            }
        }

        public int StatTotal { get { return Dex + Int + Str; } }

        public string StatIncrease(StatCode stat)
        {   // Do not exceed the cap.
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
                    return $"Dexterity increased by 1.";
                case StatCode.Intelligence:
                    Int++;
                    return $"Intelligence increased by 1."; ;
                case StatCode.Strength:
                    Str++;
                    return $"Strength increased by 1.";
            }
            return string.Empty;
        }
        #endregion

        #region Getters / Setters - Combat
        public int ArmorRating
        {
            get
            {
                int rating = 0;
                foreach (KeyValuePair<ItemLayers, Equippable> item in Equipment)
                {
                    if (item.Value.IsArmor)
                        rating += item.Value.Rating;
                }
                return rating;
            }
        }

        public int WeaponRating
        {
            get
            {
                int rating = 0;
                foreach (KeyValuePair<ItemLayers, Equippable> item in Equipment)
                {
                    if (item.Value.IsWeapon)
                        rating += item.Value.Rating;
                }
                return rating;
            }
        }

        public int AbilityModifier { get { return abilityScore(); } }
        #endregion

        #region Skills
        public Dictionary<SkillCode, Skill> Skills
        {
            get
            {
                if (m_Skills == null)
                {
                    m_Skills = new Dictionary<SkillCode, Skill>();
                    InitSkills();
                }

                return m_Skills;
            }
            private set
            {
                if (value == null)
                    return;
                m_Skills = value;
            }
        }

        public string SkillIncrease(SkillCode skill)
        {
            double increase = Skills[skill].Increase();
            if (increase == 0.0)
                return string.Empty;

            Skill s = Skills[skill];
            return $"{s.Name} increased by {increase.ToString("F1")}.";
        }

        private void InitSkills()
        {
            if (Skills == null)
            {   // Create our dictionary.
                Skills = new Dictionary<SkillCode, Skill>();
            }
            else if (Skills.Count > 0)
            {   // Skill list has already been populated, just return early.
                return;
            }

            // Iterate each of the existing skills and add it to the dictionary.
            foreach (SkillCode skill in Enum.GetValues(typeof(SkillCode)))
            {
                Skills.Add(skill, new Skill(Enum.GetName(typeof(SkillCode), skill), skill));
            }
        }
        #endregion

        #region Items / Equippables / Consumables
        public void EquipmentAdd(Equippable item)
        {
            if (item == null || !item.IsEquippable)
                return;

            if(ItemAdd(item))
                Equip(item);
        }

        public void Equip(Equippable item)
        {
            if (item == null || !item.IsEquippable)
                return;

            // Check to see if we need to remove our Main-Hand and Off-Hand
            if ((item.Layer & ItemLayers.TwoHanded) == ItemLayers.TwoHanded)
            {
                m_Equipped.Remove(ItemLayers.MainHand);
                m_Equipped.Remove(ItemLayers.Offhand);
                m_Equipped.Remove(ItemLayers.Bow);
            }

            if (item.IsWeapon && m_Equipped.ContainsKey(ItemLayers.TwoHanded))
            {
                m_Equipped.Remove(ItemLayers.TwoHanded);
            }
            else if (item.IsWeapon && m_Equipped.ContainsKey(ItemLayers.Bow))
            {
                m_Equipped.Remove(ItemLayers.Bow);
            }

            m_Equipped[item.Layer] = item;
        }

        public void Unequip(Equippable item) { Unequip(item.Layer); }
        public void Unequip(ItemLayers item)
        {
            m_Equipped.Remove(item);
        }
        
        public Item FindItem(Guid item)
        {
            if (item == null || item == Guid.Empty)
                return null;
            else if (!HasItem(item))
                return null;

            return Items[item];
        }

        public bool HasItem(Guid item)
        {
            if (item == null || item == Guid.Empty)
                return false;

            return Items.ContainsKey(item);
        }

        public bool ItemAdd(Item item)
        {
            if (item == null)
                return false;

            if (!HasItem(item.Guid))
                m_Items[item.Guid] = item;

            return true;
        }

        public bool ItemRemove(Guid item)
        {
            if (item == null || item == Guid.Empty)
                return false;
            else if (!HasItem(item))
                return true;

            return m_Items.Remove(item);
        }

        private void InitConsumables(int gold = 0, int potions = 0, int bandages = 0, int arrows = 0)
        {
            ItemAdd(new Gold(gold));
            ItemAdd(new Potion(potions));
            ItemAdd(new Bandage(bandages));
            ItemAdd(new Arrow(arrows));
        }

        private Consumable FindConsumable(Consumable.Types type)
        {
            foreach (KeyValuePair<Guid, Item> i in Items)
                if (i.Value.Type == ItemTypes.Consumable
                    && (i.Value as Consumable).ConsumableType == type)
                    return i.Value as Consumable;
            return null;
        }

        public int ConsumableAdd(Consumable c) { return ConsumableAdd(c.ConsumableType, c.Amount); }
        public int ConsumableAdd(Consumable.Types type, int amt)
        {
            if (amt <= 0)
                return 0;

            int tValue = 0;
            int tMax = 0;
            switch (type)
            {
                case Consumable.Types.Arrows:
                    tValue = Arrows.Amount;
                    tMax = Arrows.Maximum;
                    Arrows += amt;
                    break;
                case Consumable.Types.HealthPotion:
                    tValue = HealthPotions.Amount;
                    tMax = HealthPotions.Maximum;
                    HealthPotions += amt;
                    break;
                case Consumable.Types.Gold:
                    tValue = Gold.Amount;
                    tMax = Gold.Maximum;
                    Gold += amt;
                    break;
                case Consumable.Types.ManaPotion:
                case Consumable.Types.Bolts:
                default:
                    return 0;
            }

            return (tValue + amt) > tMax ? tMax - tValue : amt;
        }
        #endregion

        #region Combat
        /// <summary>
        ///     Current mobile takes damage from outside source.
        /// </summary>
        /// <param name="damage">Amount of base damage to take.</param>
        /// <returns>Total amount of damage taken after potential modifiers.</returns>
        public int TakeDamage(int damage)
        {
            int originalHP = Hits;

            if (damage > Hits)
            {
                Hits = 0;
                return originalHP;  // This is the amount of damage taken (last remaining hp.)
            }

            Hits -= damage;
            return damage;          // Damage taken was damage received.
        }

        public abstract int Attack();

        private int abilityScore()
        {
            int weaponStatVal = 0;  // Will store the value.
            switch(Weapon.Stat)
            {   // Gets the stat that is proficient for the equipped weapon.
                case StatCode.Strength:
                    weaponStatVal = Str;
                    break;
                case StatCode.Dexterity:
                    weaponStatVal = Dex;
                    break;
                case StatCode.Intelligence:
                    weaponStatVal = Int;
                    break;
            }

            int score = 0;
            weaponStatVal /= 4;
            if (weaponStatVal >= 30)
                score = 10;
            else if (weaponStatVal >= 28)
                score = 9;
            else if (weaponStatVal >= 26)
                score = 8;
            else if (weaponStatVal >= 24)
                score = 7;
            else if (weaponStatVal >= 22)
                score = 6;
            else if (weaponStatVal >= 20)
                score = 5;
            else if (weaponStatVal >= 18)
                score = 4;
            else if (weaponStatVal >= 16)
                score = 3;
            else if (weaponStatVal >= 14)
                score = 2;
            else if (weaponStatVal >= 12)
                score = 1;
            else if (weaponStatVal >= 10)
                score = 0;
            else if (weaponStatVal >= 8)
                score = -1;
            else if (weaponStatVal >= 6)
                score = -2;
            else if (weaponStatVal >= 4)
                score = -3;
            else if (weaponStatVal >= 2)
                score = -4;
            else
                score = -5;

            return score;
        }
        #endregion

        public abstract void Kill();

        public abstract void Ressurrect();

        public int MoveInDirection(Directions direction, int xMax, int yMax)
        {
            if (direction == Directions.None || direction == Directions.Nearby)
                return 0; // No desired direction, do not move.


            // Gets a pseudo-random distance between our vision (default: 15) and 30 * Speed (default: 2 - 3)
            int distance = Utility.RandomMinMax(Vision, (Vision * Speed / 2));

            // Factor in our current direction.
            while (direction > Directions.None)
            {
                foreach (Directions dir in Enum.GetValues(typeof(Directions)))
                {
                    if (dir == Directions.None || (dir & (dir - 1)) != 0)
                        continue;   // Current iteration is either 'None' or it is a combination of directions.

                    if ((direction & dir) == dir)
                    {   // We have found a direction that is within our current direction.
                        switch (dir)
                        {
                            case Directions.North:
                                // Protect ourselves from extending beyond the coordinates we are allowed to.
                                Coordinate.Y = ((Coordinate.Y + distance) > yMax) ? yMax : Coordinate.Y + distance;
                                break;
                            case Directions.South:
                                // Protection from negative coordinate.
                                Coordinate.Y = ((Coordinate.Y - distance) < 0) ? 0 : Coordinate.Y - distance;
                                break;
                            case Directions.East:
                                // Protect ourselves from extending beyond the coordinates we are allowed to.
                                Coordinate.X = ((Coordinate.X + distance) > xMax) ? xMax : Coordinate.X + distance;
                                break;
                            case Directions.West:
                                // Protection from negative coordinate.
                                Coordinate.X = ((Coordinate.X - distance) < 0) ? 0 : Coordinate.X - distance; 
                                break;
                        }

                        direction &= ~(dir);    // Removes our value from direction.
                    }
                }
            }

            return distance;
        }

        public BasicMobile Basic() { return new BasicMobile(this); }
    }
}
