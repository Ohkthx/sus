using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Shield : Armor
    {
        public Shield(Materials material) : base(ItemLayers.Offhand, material, "Shield") { }
    }
}
