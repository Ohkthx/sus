using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SUS.Shared.Objects.Mobiles;

namespace SUSClient.MenuItems
{
    class Paperdoll
    {
        private Menu menu;

        public Paperdoll(Mobile mobile)
        {
            this.menu = new Menu(mobile.ToString());
        }

        public void Print() { menu.Display(); }
    }
}
