using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class ShortSword : OneHanded
    {
        public ShortSword() : base("Short Sword", "1d6") { }
    }

    [Serializable]
    public class TwoHandedSword : TwoHanded
    {
        public TwoHandedSword() : base("Two-Handed Sword", "2d6") { }
    }

    [Serializable]
    public abstract class TwoHanded : Weapon
    {
        public TwoHanded(string name, string damage) : base(ItemLayers.TwoHanded, Materials.Iron, DamageTypes.Slashing, PrimaryStats.Strength, name, damage, range: 1) { }
    }

    [Serializable]
    public abstract class OneHanded : Weapon
    {
        public OneHanded(string name, string damage) : base(ItemLayers.MainHand, Materials.Iron, DamageTypes.Slashing, PrimaryStats.Strength, name, damage, range: 1) { }
    }
}
