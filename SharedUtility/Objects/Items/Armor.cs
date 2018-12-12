using System;

namespace SUS.Shared.Objects
{
    [Serializable]
    public abstract class Armor : Equippable
    {
        public enum Materials
        {
            Cloth       = 1,
            Leather     = 2,
            Chain       = 3,
            Plate       = 4,
        }

        private int m_ArmorRating;
        private Materials m_Material;

        #region Constructors
        public Armor(ItemLayers layer, Materials material, string name) : base(ItemTypes.Armor, layer)
        {
            Name = name;
            Material = material;
            ArmorRating = (int)Material;
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
