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

        private int m_ArmorRating;
        private Materials m_Material;
        private DamageTypes m_Resistances;

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
            get => m_Material;
            set
            {
                if (value == Material) return;

                m_Material = value;
            }
        }

        protected override int RawRating => ArmorRating;

        private int ArmorRating
        {
            get => m_ArmorRating;
            set
            {
                if (value != ArmorRating) m_ArmorRating = value;
            }
        }

        public virtual DamageTypes Resistances
        {
            get => m_Resistances;
            protected set
            {
                if (value == Resistances) return;

                m_Resistances = value;
            }

            #endregion
        }
    }
}