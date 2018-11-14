using System;

namespace SUS.Shared.Objects.Mobiles.Spawns
{
    [Serializable]
    public class Orc : BaseCreature
    {
        public Orc() : base()
        {
            Name = "an Orc";

            this.SetStr(96, 120);
            this.SetDex(81, 105);
            this.SetInt(36, 60);

            this.SetHits(58, 72);

            this.SetDamage(5, 7);
        }
    }
}
