using SUS.Objects.Items;
using SUS.Objects.Items.Equipment;
using SUS.Shared;

namespace SUS.Objects.Mobiles.Spawns
{
    public class Orc : BaseCreature
    {
        public Orc()
        {
            AiType = Ai.Types.Archer;
            Name = "an orc";

            SetStr(96, 120);
            SetDex(81, 105);
            SetInt(36, 60);

            SetHits(58, 72);

            SetDamage(5, 7);

            DamageOverride = DamageTypes.Bludgeoning;

            // Consumables and/or Equipment.
            Gold += Utility.RandomMinMax(200, 250);
            HealthPotions += Utility.Random(2);
            Bandages += Utility.Random(5);
            Arrows += Utility.RandomMinMax(10, 25);

            SetSkill(SkillName.Wrestling, 50.1, 70.0);
            SetSkill(SkillName.Archery, 50.1, 70.0);
            SetSkill(SkillName.Swordsmanship, 50.1, 70.0);

            EquipmentAdd(new CompositeBow(Weapon.Materials.Wooden));
            ItemAdd(new ShortSword(Weapon.Materials.Iron));
        }
    }
}
