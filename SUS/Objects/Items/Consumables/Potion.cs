namespace SUS.Objects.Items.Consumables
{
    public class Potion : Consumable
    {
        public static int GetEffect(int maxHits)
        {
            return (int) (maxHits * 0.33);
        }

        #region Constructors

        public Potion() : this(0)
        {
        }

        public Potion(int amount)
            : base(ConsumableTypes.HealthPotion, "Health Potion", 10)
        {
            Amount = amount;
        }

        #endregion
    }
}