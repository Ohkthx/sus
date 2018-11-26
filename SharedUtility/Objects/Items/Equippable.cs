using System;

namespace SUS.Shared.Objects
{
    [Serializable, Flags]
    public enum ItemLayers
    {
        None        = 0x00000000,

        Head        = 0x00000001,
        Neck        = 0x00000002,
        Chest       = 0x00000004,
        Legs        = 0x00000008,
        Arms        = 0x00000010,
        Hands       = 0x00000020,
        Feet        = 0x00000040,

        MainHand    = 0x00000080,
        Offhand     = 0x00000100,
        TwoHanded   = MainHand | Offhand,
    }

    [Serializable]
    public abstract class Equippable : Item
    {
        protected ItemLayers m_Layer;
        protected int m_MagicRating;

        #region Constructors
        protected Equippable(ItemTypes type, ItemLayers layer) : base (type)
        {
            Layer = layer;
        }
        #endregion

        #region Getters / Setters
        public bool IsArmor { get { return !IsWeapon; } }

        public bool IsWeapon
        {
            get
            {
                return ((Layer & ItemLayers.MainHand) == ItemLayers.MainHand) || ((Layer & ItemLayers.TwoHanded) == ItemLayers.TwoHanded);
            }
        }

        public ItemLayers Layer
        {
            get { return m_Layer; }
            protected set
            {
                if (value != ItemLayers.None && value != Layer)
                    m_Layer = value;
            }
        }

        public int MagicRating
        {
            get { return m_MagicRating; }
            protected set
            {
                if (value != MagicRating)
                    m_MagicRating = value;
            }
        }

        public abstract int RawRating { get; }

        public int Rating { get { return RawRating + MagicRating; } }

        // Can be magical with either a negative(cursed) or positive rating.
        public bool IsMagical { get { return MagicRating != 0; } }
        #endregion
    }
}
