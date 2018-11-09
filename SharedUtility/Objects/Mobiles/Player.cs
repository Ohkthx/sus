using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUS.Shared.Objects.Mobiles
{
    [Serializable]
    public class Player : Mobile
    {
        public bool isLoggedIn { get; private set; } = false;

        #region Constructors
        public Player(ulong id, string name, int hits, int strength = 10, int dexterity = 10, int intelligence = 10) : 
            base(id, name, MobileType.Player, hits, strength, dexterity, intelligence) { }
        #endregion

        public void Logout() { isLoggedIn = false; }
        public void Login() { isLoggedIn = true; }
    }
}
