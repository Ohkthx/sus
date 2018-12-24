using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles.Spawns
{
    [Serializable]
    public class Titan : BaseCreature
    {
        public Titan() : base()
        {
            Name = "a titan";

            SetStr(536, 585);
            SetDex(126, 145);
            SetInt(281, 305);

            SetHits(322, 351);

            SetDamage(13, 16);

            DamageOverride = DamageTypes.Bludgeoning | DamageTypes.Energy;

            SetSkill(SkillName.Magery, 85.1, 100.0);
            SetSkill(SkillName.Wrestling, 40.1, 50.0);

            // Consumables and/or Equipment.
            Gold += Utility.Random(350, 450);
            HealthPotions += Utility.Random(3);
            Bandages += Utility.Random(6);
        }
    }
}
