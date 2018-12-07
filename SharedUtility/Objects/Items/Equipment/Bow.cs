using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class ShortBow : Bow
    {
        public ShortBow() : base("Short Bow", "1d6", 8) { }
    }

    [Serializable]
    public class CompositeBow : Bow 
    {
        public CompositeBow() : base("Composite Bow", "1d8", 10) { }
    }

    [Serializable]
    public abstract class Bow : Weapon
    {
        public Bow(string name, string damage, int r) : base(ItemLayers.Bow, Materials.Wooden, DamageTypes.Piercing, name, damage, range: r) { }
    }
}
