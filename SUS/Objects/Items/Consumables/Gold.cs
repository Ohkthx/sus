namespace SUS.Objects.Items.Consumables
{
    public class Gold : Consumable
    {
        #region Constructors
        public Gold() : this(0) { }
        public Gold(int amount)
            : base(ConsumableTypes.Gold, "Gold", int.MaxValue)
        {
            Amount = amount;
        }
        #endregion

        #region Overrides
        public override string ToString() { return $"{Amount}gp"; }
        #endregion
    }
}
