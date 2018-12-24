using System;
using SUS.Shared.Utilities;

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

            DamageOverride = DamageTypes.Slashing;

            SetSkill(SkillName.Wrestling, 35.1, 50.0);

            // Consumables and/or Equipment.
            Gold += Utility.RandomMinMax(35, 50);
            Bandages += Utility.Random(2);
        }
    }
}
