using System;

namespace SUS.Shared.Objects.Mobiles.Spawns
{
    [Serializable]
    public class Skeleton : BaseCreature
    {
        public Skeleton() : base()
        {
            Name = "a skeleton";

            SetStr(56, 80);
            SetDex(56, 75);
            SetInt(16, 40);

            SetHits(34, 48);

            SetDamage(3, 7);

            // Consumables and/or Equipment.
            Gold += 50;
        }
    }
}
