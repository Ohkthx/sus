namespace SUS.Server.Objects.Items.Equipment
{
    public class ArmorSuit : Armor
    {
        public ArmorSuit(Materials material)
            : base(ItemLayers.Armor, material, "Suit")
        {
            switch (material)
            {
                case Materials.Plate:
                case Materials.Chainmail:
                    Weight = Weights.Heavy;
                    Resistances = DamageTypes.Slashing | DamageTypes.Bludgeoning;
                    DurabilityMax = 80;
                    Durability = 80;
                    break;
                case Materials.Hide:
                case Materials.Leather:
                    Weight = Weights.Medium;
                    Resistances = DamageTypes.Elemental;
                    DurabilityMax = 60;
                    Durability = 60;
                    break;
                default:
                    Name = "Rags";
                    Weight = Weights.Light;
                    Resistances = DamageTypes.None;
                    Invulnerable = true;
                    IsStarter = true;
                    break;
            }
        }
    }
}