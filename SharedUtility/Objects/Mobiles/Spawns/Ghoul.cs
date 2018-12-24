using System;
using SUS.Shared.Utilities;

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

            DamageOverride = DamageTypes.Slashing;

            SetSkill(SkillName.Wrestling, 45.1, 55.0);

            // Consumables and/or Equipment.
            Gold += Utility.RandomMinMax(35, 50);
            Bandages += Utility.Random(2);
        }
    }
}
