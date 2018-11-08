using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SUS.Shared.SQLite;
using SUS.Shared.Utility;

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

        #region Constructors
        public MobileModifier(Mobile mobile): this(mobile.m_ID, mobile.m_Name, mobile.m_Type) { }

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

        public void HitsModified(int change) { this.ModHits += change; }
        public void StaminaModified(int change) { this.ModStamina += change; }
        public void DeathModified(bool dead) { this.IsDead = dead; }

        public byte[] ToByte() { return Network.Serialize(this); }
    }

    [Serializable]
    public class Mobile
    {
        public Serial m_ID { get; set; }            // ID of the mobile.
        public string m_Name { get; set; }          // Name of the mobile.
        public MobileType m_Type { get; set; }      // Type of Mobile: NPC or Player.
        public Locations Location = Locations.None; // Location of the mobile.

        protected int m_Hits;                       // Current hit points.
        protected int m_HitsMax;                    // Maximum hit points.
        protected int m_DamageMin;                  // Minimum damage the mobile can do with a normal hit.
        protected int m_DamageMax;                  // Maximum damage the mobile can do with a normal hit.
        protected Attributes m_Attributes;          // Strength, Dexterity, Intellect.
        protected Dictionary<int, Skill> m_Skills;  // Skills possessed by the mobile.
        protected TypeOfDamage WeaponType = TypeOfDamage.Melee; // Current type of Weapon Damage.
        protected int m_Deaths;                     // Amount of Player Deaths.
        protected int m_KillCount;                  // Amount of Player Kills.

        #region Attributes
        [Serializable]
        public class Attributes
        {
            public int Strength { get; set; }
            public int Dexterity { get; set; }
            public int Intelligence { get; set; }

            public int Stamina { get; set; }
            public int StaminaMax { get; set; }
            public int Mana { get; set; }
            public int ManaMax { get; set; }

            public Attributes() : this(10, 10, 10) { }

            public Attributes(int str, int dex, int intellect)
            {
                this.Strength = str;
                this.Dexterity = dex;
                this.Intelligence = intellect;

                this.StaminaMax = this.Stamina = dex;
                this.ManaMax = this.Mana = intellect;
            }

            public int Total() { return Strength + Dexterity + Intelligence; }
        }
        #endregion

        #region Skills
        [Serializable]
        public class Skill
        {
            public string Name { get; set; }
            public int Type { get; set; }
            public double Value { get; set; }
            public double Max { get; set; }
            public double Step { get; }

            public enum Types { Archery, Magery, Fencing, Healing };

            public Skill(string name, int type, double value = 0, double max = 120.0)
            {
                this.Step = 0.1;
                if (max > 120.0)
                    this.Max = max;
                else
                    this.Max = 120.0;

                this.Name = name;
                this.Type = type;
                this.Value = value;
            }
        }
        #endregion

        #region Contructors
        public Mobile() : this(0, "Unknown", MobileType.None, 100) { }

        public Mobile(ulong ID, string name, MobileType type, int hits, int strength = 10, int dexterity = 10, int intelligence = 10)
        {
            this.m_Type = type;
            this.m_Attributes = new Attributes(strength, dexterity, intelligence);
            this.m_Skills = new Dictionary<int, Skill>();
            foreach (int skill in Enum.GetValues(typeof(Skill.Types)))
                m_Skills.Add(skill, new Skill(Enum.GetName(typeof(Skill.Types), skill), skill));

            if (ID == 0)
                this.m_ID = Serial.NewObject;
            else
                this.m_ID = new Serial(ID);
            this.m_Name = name;
            this.m_HitsMax = m_Hits = hits;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            string paperdoll = string.Format("                  ___________________\n" +
                "                  [Character Profile]\n" +
                "  + ---------------------------------------------------+\n" +
                "  | Character Name: {0}\n" +
                "  | Title: {1}\n" +
                "  +-[ Attributes ]\n" +
                "  | +-- Health: {2}, Max Health: {3}\n" +
                "  | +-- Strength: {4}\n" +
                "  | +-- Dexterity: {5}\t\tStamina: {6}\n" +
                "  | +-- Intelligence: {7}\tMana: {8}\n" +
                "  |   +-- Attack: {9}\n" +
                "  |   +-- Defense: {10}\n" +
                "  |\n" +
                "  +-[ Items ]\n" +
                "  | +-- Bandaids: {11}\t\tBandaid Heal Amount: {12}\n" +
                "  | +-- Arrows: {13}\t\tReagents:{14}\n" +
                "  | +-- Gold: {15}\n" +
                "  | +-- Weapon Type: {16}\n" +
                "  |\n" +
                "  +-[ Statistics ]\n" +
                "  | +-- Deaths: {17}\n" +
                "  | +-- Kill Count: {18}\n" +
                "  |\n" +
                "  +-[ Skills ]\n",
                this.m_Name, "The Player",
                this.m_Hits, this.m_HitsMax,
                this.m_Attributes.Strength,
                this.m_Attributes.Dexterity, this.m_Attributes.Stamina,
                this.m_Attributes.Intelligence, this.m_Attributes.Mana,
                0, 0,
                0, 0,
                0, 0,
                0,
                this.WeaponType.ToString(),
                this.m_Deaths,
                this.m_KillCount);

            foreach (KeyValuePair<int, Skill> skill in m_Skills)
                 paperdoll += $"  | +-- Skill: {skill.Value.Name} => [{skill.Value.Value}  /  {skill.Value.Max}]\n";

            paperdoll += "  +---------------------------------------------------+";

            return paperdoll;
        }

        public static bool operator ==(Mobile obj1, Mobile obj2)
        {
            if (object.ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            if (object.ReferenceEquals(obj1, null) ||
                object.ReferenceEquals(obj2, null))
            {
                return false;
            }

            return obj1.Equals(obj2);
        }

        public override bool Equals(object other)
        {
            return this == (other as Mobile);
        }

        public bool Equals(Mobile other)
        {
            return this == other;
        }

        public static bool operator !=(Mobile obj1, Mobile obj2)
        {
            return !(obj1 == obj2);
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash += m_ID.GetHashCode();
            hash *= 397;

            // If the name isn't blank (shouldn't be), factor it.
            if (m_Name != null)
            {
                hash += m_Name.GetHashCode();
                hash *= 397;
            }

            // If it is an NPC or Player, factor that into the hash.
            if (m_Type != MobileType.None)
            {
                hash += (int)m_Type;
                hash *= 397;
            }

            return hash;
        }
        #endregion

        public string GetHealth() { return $"{this.m_Hits} / {this.m_HitsMax}"; }

        public int GetDeaths() { return this.m_Deaths; }

        public int GetKillCount() { return this.m_KillCount; }

        // Prepares the class to be sent over the network.
        public byte[] ToByte() { return Network.Serialize(this); }

        #region Combat
        public bool IsDead()
        {
            if (this.m_Hits <= 0)
                return true;
            return false;
        }

        public void Kill()
        {
            this.m_Hits = 0;    // Sets health to 0, due to being death.
            this.m_Deaths++;    // Increase the player's death count by 1.
        }

        public void Combat(ref MobileModifier mm_init, ref Mobile opponent, ref MobileModifier mm_opp)
        {
            int initAtk = this.Attack();

            if (this == opponent)
            {   // Is the initiator attacking theirself? Do the damage and return.
                mm_init.HitsModified(this.TakeDamage(initAtk) * -1);
                mm_init.DeathModified(this.IsDead());
                return;
            }

            mm_opp.HitsModified(opponent.TakeDamage(initAtk) * -1);     // Update the MobileModifier. -1 to indicate a loss of health.
            mm_opp.DeathModified(opponent.IsDead());                    // Make sure the client knows the target is dead.

            if (!opponent.IsDead())
            {
                int oppAtk = opponent.Attack();
                mm_init.HitsModified(this.TakeDamage(oppAtk) * -1);     // Update the MobileModifier.
                mm_init.DeathModified(this.IsDead());
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

        public int Attack()
        {
            return weaponDamage();
        }

        private int weaponDamage()
        {
            Attributes attr = m_Attributes;
            switch (this.WeaponType)
            {
                case TypeOfDamage.Archery:
                    this.m_DamageMax = (attr.Dexterity / 2) + (attr.Strength / 3);
                    break;
                case TypeOfDamage.Magic:
                    this.m_DamageMax = (int)((double)attr.Intelligence / 1.5);
                    break;
                case TypeOfDamage.Melee:
                    this.m_DamageMax = (attr.Strength / 2) + (attr.Dexterity / 3);
                    break;
                default:
                    this.m_DamageMax = 4;
                    break;
            }

            this.m_DamageMin = this.m_DamageMax / 2;
            return RandomImpl.Next(m_DamageMin, m_DamageMax);
        }

        private void statIncrease() { }
        #endregion

        /// <summary>
        ///     Brings the Mobile back to life. Sets health, mana, and stamina to half the max value.
        /// </summary>
        public void Ressurrect()
        {
            this.m_Hits = this.m_HitsMax / 2;
            this.m_Attributes.Mana = this.m_Attributes.ManaMax / 2;
            this.m_Attributes.Stamina = this.m_Attributes.StaminaMax / 2;
        }
    }
}
