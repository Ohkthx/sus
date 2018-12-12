using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Leggings : Armor
    {
        public Leggings(Materials material) : base(ItemLayers.Legs, material, "Leggings") { }
    }
}
