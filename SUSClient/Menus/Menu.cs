using System;
using System.Collections.Generic;

namespace SUSClient
{
    public class Menu
    {
        private string Header = string.Empty;   // Presented at the top of the Body.
        private string Body = string.Empty;     // Information to be read.
        private string Footer = string.Empty;   // Footer is reserved for Items / Options.

        private string Name = string.Empty;
        private Menu ParentMenu;
        private Dictionary<string, Menu> SubMenus = new Dictionary<string, Menu>(); // Key: Text; Value: SubMenu to call.

        private const int MAXItemPerLine = 5;

        #region Constructors
        public Menu(string header, string body) { this.Header = header; this.Body = body; }
        public Menu(string body) : this(string.Empty, body) { }
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

        /// <summary>
        ///     Shows the menu and available options.
        /// </summary>
        public void Display()
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
    }
}
