using System;

namespace SUS.Shared
{
    #region Enums
    [Flags]
    public enum MobileTypes
    {
        None = 0,
        Player = 1,
        NPC = 2,
        Creature = 4,

        Mobile = Player | NPC | Creature,
    }

    [Flags]
    public enum MobileDirections
    {
        None = 0,

        North = 1,
        South = 2,
        East = 4,
        West = 8,

        Nearby = 16,

        NorthEast = North | East,
        NorthWest = North | West,
        SouthEast = South | East,
        SouthWest = South | West,
    }
    #endregion

    [Serializable]
    public struct BaseMobile
    {
        private int m_Serial;
        private string m_Name;
        private MobileTypes m_Type;

        #region Constructors
        public BaseMobile(MobileTypes type, int serial, string name)
        {
            m_Serial = serial;
            m_Type = type;
            m_Name = name;
        }
        #endregion

        #region Getters/Setters
        public int Serial { get { return m_Serial; } }

        public MobileTypes Type
        {
            get { return m_Type; }
            set
            {
                if (value == MobileTypes.None)
                    return;

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

        public bool IsPlayer { get { return Type == MobileTypes.Player; } }
        #endregion

        #region Overrides
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_Serial) ? m_Serial.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_Name) ? m_Name.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_Type) ? m_Type.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(BaseMobile m1, BaseMobile m2)
        {
            if (Object.ReferenceEquals(m1, m2)) return true;
            if (Object.ReferenceEquals(null, m1)) return false;
            return (m1.Equals(m2));
        }

        public static bool operator !=(BaseMobile m1, BaseMobile m2)
        {
            return !(m1 == m2);
        }

        public override bool Equals(object value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            if (value.GetType() != this.GetType()) return false;
            return IsEqual((BaseMobile)value);
        }

        public bool Equals(BaseMobile mobile)
        {
            if (Object.ReferenceEquals(null, mobile)) return false;
            if (Object.ReferenceEquals(this, mobile)) return true;
            return IsEqual(mobile);
        }

        private bool IsEqual(BaseMobile value)
        {
            return (value != null)
                && (m_Type == value.m_Type)
                && (m_Serial == value.m_Serial);
        }
        #endregion
    }
}
