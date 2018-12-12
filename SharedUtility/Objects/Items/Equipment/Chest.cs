using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Chest : Armor
    {
        public Chest(Materials material) : base(ItemLayers.Chest, material, "Chest") { }
    }
}
