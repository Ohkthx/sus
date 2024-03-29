﻿using SUS.Shared;

namespace SUS.Server.Objects.Items.Consumables
{
    public class Arrow : Consumable
    {
        #region Constructors

        public Arrow() : this(0)
        {
        }

        public Arrow(int amount)
            : base(ConsumableTypes.Arrows, "Arrow", 2000)
        {
            Amount = amount;
        }

        #endregion
    }
}