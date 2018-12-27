namespace SUS.Objects.Items.Equipment
{
    public class ArmorSuit : Armor
    {
        public ArmorSuit(Materials material) 
            : base(ItemLayers.Armor, material, "Suit")
        {
            switch(material)
            {
                case Materials.Plate:
                case Materials.Chainmail:
                    Weight = Weights.Heavy;
                    Resistances = DamageTypes.Slashing | DamageTypes.Bludgeoning;
                    break;
                case Materials.Hide:
                case Materials.Leather:
                    Weight = Weights.Medium;
                    Resistances = DamageTypes.Elemental;
                    break;
                case Materials.Cloth:
                default:
                    Weight = Weights.Medium;
                    Resistances = DamageTypes.None;
                    break;
            }
        }
    }
}
