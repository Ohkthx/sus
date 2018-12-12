using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class ShortBow : Bow
    {
        public ShortBow(Materials material) : base(material, "Short Bow", "1d6", 8) { }
    }

    [Serializable]
    public class CompositeBow : Bow 
    {
        public CompositeBow(Materials material) : base(material, "Composite Bow", "1d8", 10) { }
    }

    [Serializable]
    public abstract class Bow : Weapon
    {
        public Bow(Materials material, string name, string damage, int r) : 
            base(ItemLayers.Bow, SkillCode.Archery, StatCode.Dexterity, material, DamageTypes.Piercing, name, damage, range: r) { }
    }
}
