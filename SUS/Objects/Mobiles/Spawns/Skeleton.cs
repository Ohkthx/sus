using SUS.Shared;

namespace SUS.Objects.Mobiles.Spawns
{
    public class Skeleton : BaseCreature
    {
        public Skeleton()
        {
            AiType = Ai.Types.Melee;
            Name = "a skeleton";

            SetStr(56, 80);
            SetDex(56, 75);
            SetInt(16, 40);

            SetHits(34, 48);

            SetDamage(3, 7);

            DamageOverride = DamageTypes.Slashing;

            SetSkill(SkillName.Wrestling, 45.1, 55.0);

            // Consumables and/or Equipment.
            Gold += Utility.RandomMinMax(35, 50);
            Bandages += Utility.Random(2);
        }
    }
}