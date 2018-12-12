using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Boots : Armor
    {
        public Boots(Materials material) : base(ItemLayers.Feet, material, "Boots") { }
    }
}
