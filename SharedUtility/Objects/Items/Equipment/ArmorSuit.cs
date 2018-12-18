using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class ArmorSuit : Armor
    {
        public ArmorSuit(Materials material) : base(ItemLayers.Armor, material, "Suit")
        {
            if (material == Materials.Cloth)
                Weight = Weights.Light;
            else if (material < Materials.Chainmail)
                Weight = Weights.Medium;
            else
                Weight = Weights.Heavy;
        }
    }
}
