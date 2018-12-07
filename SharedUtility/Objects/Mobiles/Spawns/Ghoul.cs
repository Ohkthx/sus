using System;

namespace SUS.Shared.Objects.Mobiles.Spawns
{
    [Serializable]
    public class Ghoul : BaseCreature
    {
        public Ghoul() : base()
        {
            Name = "a ghoul";

            SetStr(76, 100);
            SetDex(76, 95);
            SetInt(36, 60);

            SetHits(46, 60);
            SetMana(0);

            SetDamage(7, 9);

            // Consumables and/or Equipment.
            InitConsumables(gold: 50);
        }
    }
}
