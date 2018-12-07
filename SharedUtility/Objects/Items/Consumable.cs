using System;

namespace SUS.Shared.Objects
{
    [Serializable]
    public abstract class Consumable : Item
    {
        public enum ConsumableTypes
        {
            Gold,
            Arrows,
            Bolts,
            HealthPotion,
            ManaPotion,
        }

        private ConsumableTypes m_Type;
        private int m_Amount;
        private int m_AmountMaximum;

        #region Constructors
        public Consumable(ConsumableTypes type, int maximum) : base (ItemTypes.Consumable)
        {
            ConsumableType = type;
            Amount = 0;
            Maximum = maximum;
        }
        #endregion

        #region Getters / Setters
        public override string Name
        {
            get { return $"{base.Name} ({Amount} / {Maximum})"; }
        }

        public ConsumableTypes ConsumableType
        {
            get { return m_Type; }
            private set
            {
                m_Type = value;
            }
        }

        public int Amount
        {
            get { return m_Amount; }
            protected set
            {
                if (value < 0)
                    value = 0;
                else if (value > Maximum)
                    value = Maximum;

                m_Amount = value;
            }
        }

        public int Maximum
        {
            get { return m_AmountMaximum; }
            private set
            {
                if (value < 1)
                    value = 1;

                m_AmountMaximum = value;
            }
        }
        #endregion

        #region Overrides
        public static Consumable operator ++(Consumable c)
        {
            c.Add(1);
            return c;
        }

        public static Consumable operator --(Consumable c)
        {
            c.Subtract(1);
            return c;
        }
        #endregion

        public int Add(int amount)
        {
            if (amount < 0)
                return 0;

            if (Amount + amount > Maximum)
            {
                int amt = Maximum - Amount;
                Amount = Maximum;
                return amt;
            }
            else
            {
                Amount += amount;
                return amount;
            }
        }

        private int Subtract(int amount)
        {
            if (amount < 0)
                return 0;

            if (Amount - amount < 0)
            {
                Amount = 0;
                return Amount;
            }
            else
            {
                Amount -= amount;
                return Amount - amount;
            }
        }
    }
}
