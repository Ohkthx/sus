namespace SUS.Server.Objects.Items.Equipment
{
    public class Unarmed : Weapon
    {
        public Unarmed()
            : base(ItemLayers.MainHand, Materials.None, "1d4")
        {
            Name = "unarmed";
            RequiredSkill = SkillName.Wrestling;
            Stat = StatCode.Strength;
            DamageType = DamageTypes.Bludgeoning;

            Material = Materials.None;
            Weight = Weights.Light;
        }
    }
}