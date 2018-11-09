using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SUS.Shared.Objects.Mobiles;

namespace SUSClient.MenuItems
{
    class Paperdoll : Menu
    {
        public Paperdoll(Mobile mobile) : base (mobile.ToString()) { }
    }
}
