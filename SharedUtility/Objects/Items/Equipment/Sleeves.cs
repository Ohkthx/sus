using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Sleeves : Armor
    {
        public Sleeves(Materials material) : base(ItemLayers.Arms, material, "Sleeves") { }
    }
}
