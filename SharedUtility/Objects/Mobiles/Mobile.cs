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

    [Serializable]
    public class Mobile
    {
        public Serial m_ID { get; set; }            // ID of the mobile.
        public string m_Name { get; set; }          // Name of the mobile.
        public Locations Location = Locations.None; // Location of the mobile.

        protected int m_Hits;                       // Current hit points.
        protected int m_HitsMax;                    // Maximum hit points.
        protected int m_DamageMin;                  // Minimum damage the mobile can do with a normal hit.
        protected int m_DamageMax;                  // Maximum damage the mobile can do with a normal hit.
        protected Attributes m_Attributes;          // Strength, Dexterity, Intellect.
        protected Dictionary<int, Skill> m_Skills;  // Skills possessed by the mobile.
        protected TypeOfDamage WeaponType = TypeOfDamage.Melee; // Current type of Weapon Damage.

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

            public Skill(string name, int type, double value = 0 , double max = 120.0)
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
        public Mobile() : this(0, "Unknown", 100) { }

        public Mobile(ulong ID, string name, int hits, int strength = 10, int dexterity = 10, int intelligence = 10)
        {
            this.m_Attributes = new Attributes(strength, dexterity, intelligence);
            this.m_Skills = new Dictionary<int, Skill>();
            foreach (int skill in Enum.GetValues(typeof(Skill.Types)))
                m_Skills.Add(skill, new Skill(Enum.GetName(typeof(Skill.Types),skill), skill));

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
            string whoami = $"ID: {this.m_ID}\n Username: {this.m_Name}\n Health: {this.m_Hits} / {this.m_HitsMax}\n";
            whoami += $"Stats:\n Strength: {this.m_Attributes.Strength}\n Dexterity: {this.m_Attributes.Dexterity}\n Intelligence: {this.m_Attributes.Intelligence}\n";
            whoami += "Skills:\n";

            foreach (KeyValuePair<int, Skill> skill in m_Skills)
                whoami += $" Skill: {skill.Value.Name} => [{skill.Value.Value}  /  {skill.Value.Max}]\n";

            return whoami;
        }

        public override bool Equals(object obj)
        {
            Mobile mobile = obj as Mobile;
            if (mobile == null)
                return false;

            return mobile.m_ID == this.m_ID && mobile.m_Name == this.m_Name;
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash += m_ID.GetHashCode();
            hash *= 397;
            if (m_Name != null)
                hash += m_Name.GetHashCode();
            return hash *= 397;
        }
        #endregion

        public string GetHealth()
        {
            return $"{this.m_Hits} / {this.m_HitsMax}";
        }

        public byte[] ToByte()
        {
            return Utility.Utility.Serialize(this);
        }

        #region Combat
        public bool IsDead()
        {
            if (this.m_Hits <= 0)
            {
                this.Kill();
                return true;
            }
            return false;
        }

        public void Kill()
        {
            this.m_Hits = 0;
        }

        public void Combat(ref Mobile opponent)
        {
            opponent.TakeDamage(this.Attack());
            this.TakeDamage(opponent.Attack());
        }

        public void TakeDamage(int damage)
        {
            this.m_Hits -= damage;
        }

        public int Attack()
        {
            return weaponDamage();
        }

        private int weaponDamage()
        {
            switch (this.WeaponType)
            {
                case TypeOfDamage.Archery:
                    return 4;
                case TypeOfDamage.Magic:
                    return 4;
                case TypeOfDamage.Melee:
                    return 4;
                default:
                    return 4;
            }
        }
        #endregion
    }
}
