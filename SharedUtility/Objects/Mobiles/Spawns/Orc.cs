using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles.Spawns
{
    [Serializable]
    public class Orc : BaseCreature
    {
        public Orc() : base()
        {
            Name = "an orc";

            SetStr(96, 120);
            SetDex(81, 105);
            SetInt(36, 60);

            SetHits(58, 72);

            SetDamage(5, 7);

            // Consumables and/or Equipment.
            InitConsumables(gold: 250, potions: Utility.RandomMinMax(0,2), bandages: 20, arrows: Utility.RandomMinMax(10, 25));

            EquipmentAdd(new Items.Equipment.CompositeBow(Weapon.Materials.Wooden));
        }
    }
}
