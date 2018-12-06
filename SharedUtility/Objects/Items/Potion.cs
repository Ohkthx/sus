using System;

namespace SUS.Shared.Objects
{
    [Serializable]
    public class Potion : Consumable
    {
        #region Constructors
        public Potion() : this(0) { }
        public Potion(int amount) : base(ConsumableTypes.HealthPotion, 10)
        {
            Name = "Health Potion";
            Amount = amount;
        }
        #endregion

        public static int GetEffect(int baseMax)
        {
            return (int)(baseMax * 0.33);
        }
    }
}
