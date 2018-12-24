using System;
using SUS.Shared.Utilities;

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

            DamageOverride = DamageTypes.Bludgeoning | DamageTypes.Fire;

            SetSkill(SkillName.Magery, 55.1, 70.0);
            SetSkill(SkillName.Wrestling, 45.1, 55.0);

            // Consumables and/or Equipment.
            Gold += Utility.RandomMinMax(35, 50);
            Bandages += Utility.Random(2);
        }
    }
}
