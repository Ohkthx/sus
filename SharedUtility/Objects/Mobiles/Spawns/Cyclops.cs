using System;

namespace SUS.Shared.Objects.Mobiles.Spawns
{
    [Serializable]
    public class Cyclops : BaseCreature
    {
        public Cyclops() : base()
        {
            Name = "a Cyclops";

            this.SetStr(336, 385);
            this.SetDex(96, 115);
            this.SetInt(31, 55);

            this.SetHits(202, 231);
            this.SetMana(0);

            this.SetDamage(7, 23);
        }
    }
}
