using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles
{
    public interface IPlayableClass
    {
        string ToString();
        int GetHashCode();
        bool Equals(Object obj);
        void ToInsert(ref SQLiteCommand cmd);
    }

    [Flags, Serializable]
    public enum TypeOfDamage
    {
        Archery = 1,
        Magic = 2,
        Melee = 4,
    };

    [Flags, Serializable]
    public enum MobileType
    {
        None = 0,
        Player = 1,
        NPC = 2,

        Mobile = Player | NPC,
    }

    [Serializable]
    public class MobileModifier
    {
        public Serial ID { get; private set; }
        public string Name { get; private set; }
        public MobileType Type { get; private set; } = MobileType.None;
        public bool IsDead { get; private set; } = false;   // Sets it to be false by default.
        public int ModHits { get; private set; } = 0;
        public int ModStamina { get; private set; } = 0;
        public int ModStrength { get; set; } = 0;
        public int ModDexterity { get; set; } = 0;
        public int ModIntelligence { get; set; } = 0;

        #region Constructors
        public MobileModifier(Mobile mobile): this(mobile.ID, mobile.Name, mobile.Type) { }

        public MobileModifier(ulong id, string name, MobileType type)
        {
            this.ID = id;
            this.Name = name;
            this.Type = type;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            MobileModifier mobile = obj as MobileModifier;
            if (mobile == null)
                return false;

            return mobile.ID == this.ID && mobile.Name == this.Name && mobile.Type == this.Type;
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash += ID.GetHashCode();
            hash *= 397;

            // If the name isn't blank (shouldn't be), factor it.
            if (Name != null)
            {
                hash += Name.GetHashCode();
                hash *= 397;
            }

            // If it is an NPC or Player, factor that into the hash.
            if (Type != MobileType.None)
            {
                hash += (int)Type;
                hash *= 397;
            }

            return hash;
        }
        #endregion

        public bool IsPlayer { get { return Type == MobileType.Player; } }

        public void HitsModified(int change) { this.ModHits += change; }
        public void StaminaModified(int change) { this.ModStamina += change; }
        public void DeathModified(bool dead) { this.IsDead = dead; }

        public byte[] ToByte() { return Network.Serialize(this); }
    }

    [Serializable]
    public class MobileTag
    {
        private Guid m_Guid;
        private Serial m_ID;
        private string m_Name;
        private MobileType m_Type;

        #region Constructors
        public MobileTag(Mobile mobile) : this(mobile.Guid, mobile.ID, mobile.Type, mobile.Name) { }

        public MobileTag(Guid guid, UInt64 id, MobileType type, string name)
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

        public MobileType Type
        {
            get { return m_Type; }
            set
            {
                if (value == MobileType.None)
                    return;
                else if (Type == MobileType.None)
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

        public bool IsPlayer { get { return Type == MobileType.Player; } }
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

        public static bool operator ==(MobileTag m1, MobileTag m2)
        {
            if (Object.ReferenceEquals(m1, m2)) return true;
            if (Object.ReferenceEquals(null, m1)) return false;
            return (m1.Equals(m2));
        }

        public static bool operator !=(MobileTag m1, MobileTag m2)
        {
            return !(m1 == m2);
        }

        public override bool Equals(object value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            if (value.GetType() != this.GetType()) return false;
            return IsEqual((MobileTag)value);
        }

        public bool Equals(MobileTag mobile)
        {
            if (Object.ReferenceEquals(null, mobile)) return false;
            if (Object.ReferenceEquals(this, mobile)) return true;
            return IsEqual(mobile);
        }

        private bool IsEqual(MobileTag value)
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
        private Guid m_Guid;
        private Serial m_ID;                // ID of the mobile.
        private string m_Name;              // Name of the mobile.
        private MobileType m_Type;          // Type of Mobile: NPC or Player.
        private Locations m_Location = Locations.None;    // Location of the mobile.

        private int m_StatCap;
        //private int m_StrCap, m_DexCap, m_IntCap;
        //private int m_StrMaxCap, m_DexMaxCap, m_IntMaxCap;
        private int m_Str, m_Dex, m_Int;
        private int m_Hits, m_Stam, m_Mana;

        private Dictionary<int, Skill> m_Skills;  // Skills possessed by the mobile.

        #region Contructors
        public Mobile(MobileType type)
        {
            Type = type;

            m_Skills = new Dictionary<int, Skill>();
            foreach (int skill in Enum.GetValues(typeof(Skill.Types)))
                m_Skills.Add(skill, new Skill(Enum.GetName(typeof(Skill.Types), skill), skill));

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
                $"  | +-- Health: {Hits}, Max Health: {HitsMax}\n" +
                $"  | +-- Strength: {Str}\n" +
                $"  | +-- Dexterity: {Dex}\t\tStamina: {Stam}\n" +
                $"  | +-- Intelligence: {Int}\tMana: {Mana}\n" +
                $"  |   +-- Attack: {0}\n" +
                $"  |   +-- Defense: {0}\n" +
                $"  |\n" +
                $"  +-[ Items ]\n" +
                $"  | +-- Bandaids: {0}\t\tBandaid Heal Amount: {0}\n" +
                $"  | +-- Arrows: {0}\t\tReagents: {0}\n" +
                $"  | +-- Gold: {0}\n" +
                $"  |\n" +
                $"  +-[ Skills ]\n";

            foreach (KeyValuePair<int, Skill> skill in m_Skills)
                 paperdoll += $"  | +-- Skill: {skill.Value.Name} => [{skill.Value.Value}  /  {skill.Value.Max}]\n";

            paperdoll += "  +---------------------------------------------------+";

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

        #region Getters / Setters
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

        public MobileType Type
        {
            get { return m_Type; }
            private set
            {
                if (value != MobileType.None && value != Type)
                    m_Type = value;
            }
        }

        public bool IsPlayer { get { return m_Type == MobileType.Player; } }

        public bool IsDead { get { return Hits <= 0; } }

        public string GetHealth() { return $"{Hits} / {HitsMax}"; }
        #endregion

        #region Stats
        public void InitStats(int rawStr, int rawDex, int rawInt)
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
        #endregion

        #region Skills
        public Dictionary<int, Skill> Skills
        {
            get { return m_Skills; }
        }
        #endregion

        #region Combat
        public void Combat(ref MobileModifier mm_init, ref Mobile opponent, ref MobileModifier mm_opp)
        {
            int initAtk = this.Attack();

            if (this == opponent)
            {   // Is the initiator attacking theirself? Do the damage and return.
                mm_init.HitsModified(this.TakeDamage(initAtk) * -1);
                mm_init.DeathModified(this.IsDead);
                return;
            }

            mm_opp.HitsModified(opponent.TakeDamage(initAtk) * -1);     // Update the MobileModifier. -1 to indicate a loss of health.
            mm_opp.DeathModified(opponent.IsDead);                    // Make sure the client knows the target is dead.

            if (!opponent.IsDead)
            {
                int oppAtk = opponent.Attack();
                mm_init.HitsModified(this.TakeDamage(oppAtk) * -1);     // Update the MobileModifier.
                mm_init.DeathModified(this.IsDead);
            }
        }

        /// <summary>
        ///     Current mobile takes damage from outside source.
        /// </summary>
        /// <param name="damage">Amount of base damage to take.</param>
        /// <returns>Total amount of damage taken after potential modifiers.</returns>
        public int TakeDamage(int damage)
        {
            int originalHP = this.m_Hits;

            if (damage > this.m_Hits)
            {   
                this.Kill();
                return originalHP;  // This is the amount of damage taken (last remaining hp.)
            }

            this.m_Hits -= damage;
            return damage;          // Damage taken was damage received.
        }

        public abstract int Attack();
        #endregion

        public void ApplyModification(MobileModifier mod)
        {
            if (mod.ID != ID || mod.Type != Type)
                return;

            Str += mod.ModStrength;
            Int += mod.ModIntelligence;
            Dex += mod.ModDexterity;

            if (mod.IsDead)
                Kill();
            else
            {
                Hits += mod.ModHits;
                Stam += mod.ModStamina;
            }
        }

        public abstract void Kill();

        public abstract void Ressurrect();

        public MobileTag getTag() { return new MobileTag(this); }

        // Prepares the class to be sent over the network.
        public byte[] ToByte() { return Network.Serialize(this); }
    }
}
