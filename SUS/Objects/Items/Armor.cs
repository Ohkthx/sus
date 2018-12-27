using System;

namespace SUS.Objects.Items
{
    public abstract class Armor : Equippable
    {
        public enum Materials
        {
            Cloth = 0,
            Leather = 11,
            Hide = 12,
            Chainmail = 16,
            Plate = 18,
        }

        private int m_ArmorRating;
        private Materials m_Material;
        private DamageTypes m_Resistances;

        #region Constructors
        public Armor(ItemLayers layer, Materials material, string name) 
            : base(ItemTypes.Armor, layer)
        {
            Name = name;
            Material = material;
            ArmorRating = (int)Material;
            Weight = Weights.Light;
            Resistances = DamageTypes.None;
        }
        #endregion

        #region Getters / Setters
        public override string Name { get { return $"{Enum.GetName(typeof(Materials), Material)} {base.Name}"; } }

        public Materials Material
        {
            get { return m_Material; }
            set
            {
                if (value == Material)
                    return;

                m_Material = value;
            }
        }
        public override int RawRating { get { return ArmorRating; } }

        public int ArmorRating
        {
            get { return m_ArmorRating; }
            private set
            {
                if (value != ArmorRating)
                    m_ArmorRating = value;
            }
        }

        public virtual DamageTypes Resistances
        {
            get { return m_Resistances; }
            protected set
            {
                if (value == Resistances)
                    return;
                m_Resistances = value;
            }

            #endregion
        }
    }
}
