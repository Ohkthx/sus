using System;

namespace SUS.Shared.Objects
{
    [Serializable]
    public class Gold : Consumable
    {
        #region Constructors
        public Gold() : this(0) { }
        public Gold(int amount) : base(ConsumableTypes.Gold, int.MaxValue)
        {
            Amount = amount;
        }
        #endregion

        #region Overrides
        public override string Name { get { return $"{Amount}gp"; } }
        #endregion
    }
}
