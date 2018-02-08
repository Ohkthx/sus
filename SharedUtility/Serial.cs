using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUS.Shared.Utility
{
    class Serial
    {
        private readonly int m_Serial;

        private static Serial m_LastMobile = new Serial(0);
        private static Serial m_LastItem = 0x40000000;
    }
}
