using System;
using SUS.Shared.Objects;

namespace SUS.Shared.Utilities
{
    [Serializable]
    public class Serial
    {
        private readonly UInt64 m_Serial;

        private static Serial m_LastMobile = new Serial(0);

        public Serial(int serial)
        {
            m_Serial = 0;
        }

        public Serial(UInt64 serial)
        {
            m_Serial = serial;
        }

        public static Serial NewObject
        {
            get
            {
                while (GameObject.FindMobile(MobileType.Creature, m_LastMobile = (m_LastMobile + 1)) != null)
                { }

                return m_LastMobile;
            }
        }

        public UInt64 ToInt()
        {
            return m_Serial;
        }

        #region Overrides
        public static implicit operator UInt64(Serial a)
        {
            return a.m_Serial;
        }

        public static implicit operator Serial(UInt64 a)
        {
            return new Serial(a);
        }

        public override string ToString()
        {
            return this.m_Serial.ToString();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_Serial) ? m_Serial.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(Serial s1, Serial s2)
        {
            if (Object.ReferenceEquals(s1, s2)) return true;
            if (Object.ReferenceEquals(null, s1)) return false;
            return (s1.Equals(s2));
        }

        public static bool operator !=(Serial s1, Serial s2)
        {
            return !(s1 == s2);
        }

        public override bool Equals(object value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            if (value.GetType() != this.GetType()) return false;
            return IsEqual((Serial)value);
        }

        public bool Equals(Serial serial)
        {
            if (Object.ReferenceEquals(null, serial)) return false;
            if (Object.ReferenceEquals(this, serial)) return true;
            return IsEqual(serial);
        }

        private bool IsEqual(Serial value)
        {
            return (value != null)
                && (ToInt() == value.ToInt());
        }
        #endregion
    }
}
