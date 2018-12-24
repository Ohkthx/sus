using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Unarmed : Weapon
    {
        public Unarmed() : base(ItemLayers.MainHand, Materials.None, "1d4")
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
