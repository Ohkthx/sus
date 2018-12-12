using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Helmet : Armor
    {
        public Helmet(Materials material) : base(ItemLayers.Head, material, "Helmet") { }
    }
}
