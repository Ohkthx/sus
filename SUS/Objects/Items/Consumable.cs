namespace SUS.Objects.Items
{
    public abstract class Consumable : Item
    {
        private int m_Amount;
        private int m_AmountMaximum;

        #region Constructors

        protected Consumable(ConsumableTypes type, string name, int maximum)
            : base(ItemTypes.Consumable)
        {
            ConsumableType = type;
            Name = name;
            Amount = 0;
            Maximum = maximum;
        }

        #endregion

        #region Getters / Setters

        public override string Name
        {
            get
            {
                if (ConsumableType == ConsumableTypes.Gold || Amount <= 1) return base.Name;

                // Make the name plural.
                return base.Name + "s";
            }
        }

        public ConsumableTypes ConsumableType { get; }

        public int Amount
        {
            get => m_Amount;
            protected set
            {
                if (value < 0)
                    value = 0;
                else if (value > Maximum) value = Maximum;

                m_Amount = value;
            }
        }

        public int Maximum
        {
            get => m_AmountMaximum;
            private set
            {
                if (value < 1) value = 1;

                m_AmountMaximum = value;
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{Name} ({Amount} / {Maximum})";
        }

        public static Consumable operator ++(Consumable c)
        {
            return c + 1;
        }

        public static Consumable operator +(Consumable c1, Consumable c2)
        {
            return c1 + c2.Amount;
        }

        public static Consumable operator +(Consumable c, int amt)
        {
            if (amt <= 0)
                return c;
            if (c.Amount == c.Maximum) return c;

            if (c.Amount + amt > c.Maximum)
                c.Amount = c.Maximum;
            else
                c.Amount = c.Amount + amt;

            return c;
        }

        public static Consumable operator --(Consumable c)
        {
            return c - 1;
        }

        public static Consumable operator -(Consumable c1, Consumable c2)
        {
            return c1 - c2.Amount;
        }

        public static Consumable operator -(Consumable c, int amt)
        {
            if (amt <= 0)
                return c;
            if (c.Amount == 0) return c;

            if (c.Amount - amt < 0)
                c.Amount = 0;
            else
                c.Amount = c.Amount - amt;

            return c;
        }

        #endregion
    }
}