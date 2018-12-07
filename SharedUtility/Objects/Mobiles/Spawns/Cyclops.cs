using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles.Spawns
{
    [Serializable]
    public class Cyclops : BaseCreature
    {
        public Cyclops() : base()
        {
            Name = "a cyclops";

            SetStr(336, 385);
            SetDex(96, 115);
            SetInt(31, 55);

            SetHits(202, 231);
            SetMana(0);

            SetDamage(7, 23);

            // Consumables and/or Equipment.
            InitConsumables(350, Utility.RandomMinMax(0, 3), 0);
        }
    }
}
