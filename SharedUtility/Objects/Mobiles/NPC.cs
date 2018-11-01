using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUS.Shared.Objects.Mobiles
{
    [Serializable]
    public class NPC : Mobile
    {
        #region Constructors
        public NPC() : base() { }
        public NPC(string name, int hits, int strength = 10, int dexterity = 10, int intelligence = 10) :
            base(0, name, MobileType.NPC, hits, strength, dexterity, intelligence)
        { }
        #endregion
    }
}