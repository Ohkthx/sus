using SUS.Shared;

namespace SUS.Server.Objects.Mobiles.Spawns
{
    public class Ettin : BaseCreature
    {
        public Ettin() : base(SpawnTypes.Ettin)
        {
            AiType = Ai.Types.Melee;
            Name = "an ettin";

            SetStr(136, 165);
            SetDex(56, 75);
            SetInt(31, 55);

            SetHits(82, 99);

            SetDamage(7, 17);

            DamageOverride = DamageTypes.Bludgeoning;

            // Consumables and/or Equipment.
            Gold += Utility.RandomMinMax(175, 225);
            HealthPotions += Utility.Random(1);
            Bandages += Utility.Random(3);

            SetSkill(SkillName.Wrestling, 50.1, 60.0);
        }
    }
}