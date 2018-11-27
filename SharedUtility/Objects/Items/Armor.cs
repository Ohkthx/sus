using System;

namespace SUS.Shared.Objects
{
    [Serializable]
    public enum ArmorMaterials
    {
        Cloth       = 1,
        Leather     = 2,
        ChainMail   = 3,
        Plate       = 4,
    }

    [Serializable]
    public class Armor : Equippable
    {
        private int m_ArmorRating;

        #region Constructors
        public Armor(ItemLayers layer, ArmorMaterials material, string name) : base(ItemTypes.Armor, layer)
        {
            Name = name;
            ArmorRating = (int)material;
        }
        #endregion

        #region Getters / Setters
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
