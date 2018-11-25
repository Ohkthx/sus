using System;

namespace SUS.Shared.Objects.Mobiles.Spawns
{
    [Serializable]
    public class Wraith : BaseCreature
    {
        public Wraith() : base()
        {
            Name = "a wraith";

            SetStr(76, 100);
            SetDex(76, 95);
            SetInt(36, 60);

            SetHits(46, 60);

            SetDamage(7, 11);
        }
    }
}
