using System;

namespace SUS.Shared.Objects
{
    [Serializable]
    public class Armor : Equippable
    {
        public enum Materials
        {
            Cloth       = 1,
            Leather     = 2,
            ChainMail   = 3,
            Plate       = 4,
        }

        private int m_ArmorRating;

        #region Constructors
        public Armor(ItemLayers layer, Materials material, string name) : base(ItemTypes.Armor, layer)
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
