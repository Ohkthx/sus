namespace SUS.Objects.Items
{
    public class Arrow : Consumable
    {
        #region Constructors
        public Arrow() : this(0) { }
        public Arrow(int amount) 
            : base(ConsumableTypes.Arrows, "Arrow", 2000)
        {
            Amount = amount;
        }
        #endregion
    }
}
