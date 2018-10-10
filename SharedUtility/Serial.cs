using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUS.Shared.Utility
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
                while (GameObject.FindMobile(m_LastMobile = (m_LastMobile + 1)) != null)
                { }

                return m_LastMobile;
            }
        }

        public UInt64 ToInt()
        {
            return m_Serial;
        }

        #region Overrides
        public override bool Equals(object obj)
        {
            Serial s = obj as Serial;
            return s.m_Serial == this.m_Serial;
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash += this.m_Serial.GetHashCode();
            hash *= 397;
            return hash;
        }

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
        #endregion
    }
}
