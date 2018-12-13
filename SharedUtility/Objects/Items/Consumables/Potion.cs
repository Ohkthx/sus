using System;

namespace SUS.Shared.Objects.Items
{
    [Serializable]
    public class Potion : Consumable
    {
        #region Constructors
        public Potion() : this(0) { }
        public Potion(int amount) : base(Types.HealthPotion, "Health Potion", 10)
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
