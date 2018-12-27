namespace SUS.Objects.Items.Equipment
{
    public class ShortBow : Bow
    {
        public ShortBow(Materials material) 
            : base(material, "1d6", 8)
        {
            Name = "Short Bow";
        }
    }

    public class CompositeBow : Bow 
    {
        public CompositeBow(Materials material) 
            : base(material, "1d8", 10)
        {
            Name = "Composite Bow";
        }
    }

    public abstract class Bow : Weapon
    {
        public Bow(Materials material, string damage, int r) 
            : base(ItemLayers.Bow, material, damage, range: r)
        {
            RequiredSkill = SkillName.Archery;
            Stat = StatCode.Dexterity;
            DamageType = DamageTypes.Piercing;
        }
    }
}
