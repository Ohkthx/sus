namespace SUS.Server.Objects.Items.Equipment
{
    public class ShortBow : Bow
    {
        public ShortBow(Materials material)
            : base(WeaponTypes.ShortBow, material, "1d6", 8)
        {
            Name = "Short Bow";
        }
    }

    public class CompositeBow : Bow
    {
        public CompositeBow(Materials material)
            : base(WeaponTypes.CompositeBow, material, "1d8", 10)
        {
            Name = "Composite Bow";
        }
    }

    public abstract class Bow : Weapon
    {
        protected Bow(WeaponTypes type, Materials material, string damage, int r)
            : base(type, ItemLayers.Bow, material, damage, r)
        {
            RequiredSkill = SkillName.Archery;
            Stat = StatCode.Dexterity;
            DamageType = DamageTypes.Piercing;

            DurabilityMax = 80;
            Durability = 80;
        }
    }
}