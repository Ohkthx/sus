using System;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Unarmed : Weapon
    {
        public Unarmed() : base(ItemLayers.MainHand, Materials.None, DamageTypes.Bludgeoning, StatCode.Strength, "unarmed", "1d4", range: 1)
        {
            Material = Materials.None;
        }
    }
}
