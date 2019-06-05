using System;

namespace SUS.Server.Objects.Items
{
    public abstract class Armor : Equippable
    {
        public enum Materials
        {
            Cloth = 0,
            Leather = 11,
            Hide = 12,
            Chainmail = 16,
            Plate = 18
        }

        private int _armorRating;
        private Materials _material;
        private DamageTypes _resistances;


        #region Constructors

        protected Armor(ItemLayers layer, Materials material, string name)
            : base(ItemTypes.Armor, layer, 0)
        {
            Name = name;
            Material = material;
            ArmorRating = (int) Material;
            Weight = Weights.Light;
            Resistances = DamageTypes.None;
        }

        #endregion

        #region Getters / Setters

        public override string Name => $"{Enum.GetName(typeof(Materials), Material)} {base.Name}";

        private Materials Material
        {
            get => _material;
            set
            {
                if (value == Material) return;

                _material = value;
            }
        }

        protected override int RawRating => ArmorRating;

        private int ArmorRating
        {
            get => _armorRating;
            set
            {
                if (value != ArmorRating) _armorRating = value;
            }
        }

        public virtual DamageTypes Resistances
        {
            get => _resistances;
            protected set
            {
                if (value == Resistances) return;

                _resistances = value;
            }
        }

        #endregion
    }
}