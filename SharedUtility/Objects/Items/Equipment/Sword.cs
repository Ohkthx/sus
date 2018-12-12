using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class ShortSword : OneHanded
    {
        public ShortSword(Materials material) : base(material, "Short Sword", "1d6") { }
    }

    [Serializable]
    public class TwoHandedSword : TwoHanded
    {
        public TwoHandedSword(Materials material) : base(material, "Two-Handed Sword", "2d6") { }
    }

    [Serializable]
    public abstract class TwoHanded : Weapon
    {
        public TwoHanded(Materials material, string name, string damage) : base(ItemLayers.TwoHanded, material, DamageTypes.Slashing, PrimaryStats.Strength, name, damage, range: 1) { }
    }

    [Serializable]
    public abstract class OneHanded : Weapon
    {
        public OneHanded(Materials material, string name, string damage) : base(ItemLayers.MainHand, material, DamageTypes.Slashing, PrimaryStats.Strength, name, damage, range: 1) { }
    }
}
