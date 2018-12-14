using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class ArmorSuit : Armor
    {
        public ArmorSuit(Materials material) : base(ItemLayers.Armor, material, "Suit") { }
    }
}
