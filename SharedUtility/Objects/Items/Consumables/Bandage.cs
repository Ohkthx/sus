﻿using System;

namespace SUS.Shared.Objects.Items
{
    [Serializable]
    public class Bandage : Consumable
    {
        #region Constructors
        public Bandage() : this(0) { }
        public Bandage(int amount) : base(Types.Bandages, "Bandage", 20)
        {
            Amount = amount;
        }
        #endregion

        public static int GetEffect(int maxHits, double skillLvl = 1.0)
        {
            return (int)((maxHits * 0.20) + (skillLvl / 5));
        }
    }
}
