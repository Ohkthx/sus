using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Unarmed : Weapon
    {
        public Unarmed() : base(ItemLayers.MainHand, SkillCode.Wrestling, StatCode.Strength, Materials.None, DamageTypes.Bludgeoning, "unarmed", "1d4")
        {
            Material = Materials.None;
        }
    }
}
