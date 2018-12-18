using System;

namespace SUS.Shared.Objects
{
    [Serializable]
    public abstract class Armor : Equippable
    {
        public enum Materials
        {
            Cloth       = 0,
            Leather     = 11,
            Hide        = 12,
            Chainmail   = 16,
            Plate       = 18,
        }

        private int m_ArmorRating;
        private Materials m_Material;

        #region Constructors
        public Armor(ItemLayers layer, Materials material, string name) : base(ItemTypes.Armor, layer)
        {
            Name = name;
            Material = material;
            ArmorRating = (int)Material;
            Weight = Weights.Light;
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
        public override int RawRating {  get { return ArmorRating; } }

        public int ArmorRating
        {
            get { return m_ArmorRating; }
            private set
            {
                if (value != ArmorRating)
                    m_ArmorRating = value;
            }
        }
        #endregion
    }
}
