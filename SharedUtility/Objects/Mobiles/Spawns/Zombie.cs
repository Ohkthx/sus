using System;

namespace SUS.Shared.Objects.Mobiles.Spawns
{
    [Serializable]
    public class Zombie : BaseCreature
    {
        public Zombie() : base()
        {
            Name = "a zombie";

            SetStr(46, 70);
            SetDex(31, 50);
            SetInt(26, 40);

            SetHits(28, 42);

            SetDamage(3, 7);

            // Consumables and/or Equipment.
            InitConsumables(gold: 50);
        }
    }
}
