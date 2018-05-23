using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUS.Shared.Utility
{
    public class Serial
    {
        private readonly int m_Serial;

        private static Serial m_LastMobile = new Serial(0);

        public Serial(int serial)
        {
            m_Serial = 0;
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

        public static implicit operator int(Serial a)
        {
            return a.m_Serial;
        }

        public static implicit operator Serial(int a)
        {
            return new Serial(a);
        }
    }
}
