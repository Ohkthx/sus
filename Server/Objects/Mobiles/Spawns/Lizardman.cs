using SUS.Server.Objects.Items;
using SUS.Server.Objects.Items.Equipment;
using SUS.Shared;

namespace SUS.Server.Objects.Mobiles.Spawns
{
    public class Lizardman : BaseCreature
    {
        public Lizardman() : base(SpawnTypes.Lizardman)
        {
            AiType = Ai.Types.Melee;
            Name = "a lizardman";

            SetStr(96, 120);
            SetDex(86, 105);
            SetInt(36, 60);

            SetHits(58, 72);

            SetDamage(5, 7);

            DamageOverride = DamageTypes.Piercing;


            // Consumables and/or Equipment.
            Gold += Utility.RandomMinMax(150, 200);
            HealthPotions += Utility.Random(1);
            Bandages += Utility.Random(3);

            SetSkill(SkillName.Wrestling, 50.1, 70.0);

            AddItem(new ShortSword(Weapon.Materials.Iron));
        }
    }
}