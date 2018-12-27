namespace SUS.Objects.Items
{
    public class Potion : Consumable
    {
        #region Constructors
        public Potion() : this(0) { }
        public Potion(int amount) 
            : base(ConsumableTypes.HealthPotion, "Health Potion", 10)
        {
            Amount = amount;
        }
        #endregion

        public static int GetEffect(int maxHits)
        {
            return (int)(maxHits * 0.33);
        }
    }
}
