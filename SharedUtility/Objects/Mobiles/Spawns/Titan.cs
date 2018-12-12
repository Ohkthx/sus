using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles.Spawns
{
    [Serializable]
    public class Titan : BaseCreature
    {
        public Titan() : base()
        {
            Name = "a titan";

            SetStr(536, 585);
            SetDex(126, 145);
            SetInt(281, 305);

            SetHits(322, 351);

            SetDamage(13, 16);

            // Consumables and/or Equipment.
            InitConsumables(gold: 450, potions: Utility.RandomMinMax(0, 3));
        }
    }
}
