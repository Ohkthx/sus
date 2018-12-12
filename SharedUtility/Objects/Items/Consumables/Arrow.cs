using System;

namespace SUS.Shared.Objects.Items
{
    [Serializable]
    public class Arrow : Consumable
    {
        #region Constructors
        public Arrow() : this(0) { }
        public Arrow(int amount) : base(Types.Arrows, "Arrow", 2000)
        {
            Amount = amount;
        }
        #endregion
    }
}
