using System;

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

            EquipmentAdd(new Weapon(ItemLayers.Bow, WeaponMaterials.Steel, "Composite Bow", "1d8"));
        }
    }
}
