namespace SUS.Server.Objects.Items.Equipment
{
    public class Shield : Armor
    {
        public Shield()
            : base(ItemLayers.Offhand, Materials.Plate, "Shield")
        {
            Weight = Weights.Medium;
            Resistances = DamageTypes.Piercing;

            DurabilityMax = 70;
            Durability = 70;
        }

        public override string Name => $"Kite {base.Name}";
        protected override int RawRating => 2;
    }
}