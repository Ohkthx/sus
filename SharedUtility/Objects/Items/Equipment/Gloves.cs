using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Gloves : Armor
    {
        public Gloves(Materials material) : base(ItemLayers.Hands, material, "Gloves") { }
    }
}
