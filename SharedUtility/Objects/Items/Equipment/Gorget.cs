using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Gorget : Armor
    {
        public Gorget(Materials material) : base(ItemLayers.Neck, material, "Gorget") { }
    }
}
