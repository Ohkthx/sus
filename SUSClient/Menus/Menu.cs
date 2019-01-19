using System;
using System.Collections.Generic;
using SUS.Shared.Packets;

namespace SUSClient.Menus
{
    public abstract class Menu
    {
        private readonly string m_Header;   // Presented at the top of the Body.
        private readonly string m_Body;     // Information to be read.
        private string m_Footer;   // Footer is reserved for Items / Options.
        protected readonly List<string> Options = new List<string>();

        private string m_Name;
        private Menu m_ParentMenu;
        private readonly Dictionary<string, Menu> m_SubMenus = new Dictionary<string, Menu>(); // Key: Text; Value: SubMenu to call.

        private const int MaxItemPerLine = 5;

        private string Name
        {
            get => m_Name ?? "Unknown";
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                m_Name = value;
            }
        }

        #region Constructors

        private Menu(string header, string body) { m_Header = header; m_Body = body; }
        protected Menu(string body) : this(string.Empty, body) { }

        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            var m = obj as Menu;
            if (m == null)
            {
                return false;
            }

            return Name == m.Name;
        }

        public static bool operator ==(Menu obj1, Menu obj2)
        {
            return obj1 != null && obj1.Equals(obj2);
        }

        public static bool operator !=(Menu obj1, Menu obj2)
        {
            return !(obj1 == obj2);
        }

        public override int GetHashCode()
        {
            var hash = 37;
            hash += m_Body.GetHashCode();
            hash *= 37;
            if (Name != null) hash += Name.GetHashCode();
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
            menu.Name = str;     // Assign the name here so that it is exact to the key.
            menu.SetParent(this);  // Assign the Parent to this current instance.
            m_SubMenus[str] = menu;  // Add it to our list of menus.
        }

        /// <summary>
        ///     Generates the Footer based on the current Items.
        /// </summary>
        private void CreateFooter()
        {
            var n = 0;  // Counter for current place.

            foreach (var kp in m_SubMenus)
            {
                if ((n - 1 % MaxItemPerLine) == 0 && n + 1 < m_SubMenus.Count)
                {
                    m_Footer += "\n";     // Add a new line.
                }

                m_Footer += $"[ {kp.Key} ] ";    // Print out our "key"
                n++;
            }
        }

        private void SetParent(Menu parent) { m_ParentMenu = parent; }
        #endregion

        public abstract Packet Display();

        protected void ShowMenu()
        {   // Print our header if we have one.
            if (m_Header != string.Empty)
            {
                Console.WriteLine(m_Header + "\n");
            }

            // Print the body text.
            Console.WriteLine(m_Body);

            // Attempt to generate our footer, if one is created with items, print it.
            CreateFooter();
            if (m_Footer != string.Empty)
            {
                Console.WriteLine("\n" + m_Footer);
            }
        }

        protected static string GetInput()
        {
            Console.Write("Please choose an option: ");
            return Console.ReadLine();
        }

        protected abstract void PrintOptions();
        protected abstract int ParseOptions(string input);
    }
}
