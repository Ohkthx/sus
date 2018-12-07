using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Unarmed : Weapon
    {
        public Unarmed() : base(ItemLayers.MainHand, Materials.None, DamageTypes.Bludgeoning, PrimaryStats.Strength, "unarmed", "1d4", range: 1) { }
    }
}
