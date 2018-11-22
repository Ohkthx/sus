﻿using System;

namespace SUS.Shared.Objects.Mobiles.Spawns
{
    [Serializable]
    public class Skeleton : BaseCreature
    {
        public Skeleton() : base()
        {
            Name = "a Skeleton";

            this.SetStr(56, 80);
            this.SetDex(56, 75);
            this.SetInt(16, 40);

            this.SetHits(34, 48);

            this.SetDamage(3, 7);

        }
    }
}