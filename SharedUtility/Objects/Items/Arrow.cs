using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUS.Shared.Objects
{
    [Serializable]
    public class Arrow : Consumable
    {
        #region Constructors
        public Arrow() : this(0) { }
        public Arrow(int amount) : base(ConsumableTypes.Arrows, 2000)
        {
            Name = "Arrow";
            Amount = amount;
        }
        #endregion
    }
}
