namespace SUS.Objects.Items.Equipment
{
    public class Shield : Armor
    {
        public Shield() 
            : base(ItemLayers.Offhand, Materials.Plate, "Shield")
        {
            Weight = Weights.Medium;
            Resistances = DamageTypes.Piercing;
        }

        public override string Name { get { return $"Kite {base.Name}"; } }
        public override int RawRating { get { return 2; } }
    }
}
