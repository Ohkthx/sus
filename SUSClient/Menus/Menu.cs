using System;
using System.Collections.Generic;
using SUS.Shared.Packets;

namespace SUSClient
{
    public abstract class Menu
    {
        protected string Header = string.Empty;   // Presented at the top of the Body.
        protected string Body = string.Empty;     // Information to be read.
        protected string Footer = string.Empty;   // Footer is reserved for Items / Options.
        protected List<string> Options = new List<string>();

        private string Name = string.Empty;
        private Menu ParentMenu;
        private Dictionary<string, Menu> SubMenus = new Dictionary<string, Menu>(); // Key: Text; Value: SubMenu to call.

        private const int MAXItemPerLine = 5;

        #region Constructors
        public Menu(string header, string body) { this.Header = header; this.Body = body; }
        public Menu(string body) : this(string.Empty, body) { }
        public Menu() { }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            Menu m = obj as Menu;
            if (m == null)
                return false;

            return this.Name == m.Name;
        }

        public static bool operator ==(Menu obj1, Menu obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(Menu obj1, Menu obj2)
        {
            return !(obj1 == obj2);
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash += this.Body.GetHashCode();
            hash *= 37;
            hash += this.Name.GetHashCode();
            hash *= 397;
            return hash;
        }
        #endregion

        #region Modifiers
        /// <summary>
        ///     Adds and links a new Submenu.
        /// </summary>
        /// <param name="str">SubMenu name.</param>
        /// <param name="menu">SubMenu object.</param>
        public void AddItem(string str, Menu menu)
        {
            menu.SetName(str);     // Assign the name here so that it is exact to the key.
            menu.SetParent(this);  // Assign the Parent to this current instance.
            SubMenus[str] = menu;  // Add it to our list of menus.
        }

        /// <summary>
        ///     Generates the Footer based on the current Items.
        /// </summary>
        private void CreateFooter()
        {
            int n = 0;  // Counter for current place.

            foreach(KeyValuePair<string, Menu> kp in this.SubMenus)
            {
                if ((n - 1 % MAXItemPerLine) == 0 && n+1 < this.SubMenus.Count)
                    Footer += "\n";     // Add a new line.
                Footer += string.Format("[ {0} ] ", kp.Key);    // Print out our "key"
                n++;
            }
        }
        
        private void SetName(string str) { this.Name = str; }

        private void SetParent(Menu parent) { this.ParentMenu = parent; }
        #endregion

        public abstract Packet Display();

        protected void ShowMenu()
        {   // Print our header if we have one.
            if (this.Header != string.Empty)
                Console.WriteLine(this.Header + "\n");

            // Print the body text.
            Console.WriteLine(this.Body);

            // Attempt to generate our footer, if one is created with items, print it.
            CreateFooter();
            if (this.Footer != string.Empty)
                Console.WriteLine("\n"+Footer);
        }

        protected string GetInput()
        {
            Console.Write("Please choose an option: ");
            return Console.ReadLine();
        }

        protected abstract void PrintOptions();
        protected abstract int ParseOptions(string input);
    }
}
