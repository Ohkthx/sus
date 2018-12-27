using System;

namespace SUS.Objects
{
    [Flags]
    public enum ItemLayers
    {
        None        = 0x00000000,

        Melee       = 0x00000001,
        Ranged      = 0x00000002,

        MainHand    = 0x00000004,
        Offhand     = 0x00000008,
        TwoHanded   = MainHand | Offhand,
        Bow         = TwoHanded | Ranged,

        Armor       = 0x00000010,
    }

    public enum Weights
    {
        Light,
        Medium,
        Heavy,
    }

    public abstract class Equippable : Item
    {
        protected ItemLayers m_Layer;
        protected Weights m_Weight;
        protected int m_MagicRating;

        #region Constructors
        protected Equippable(ItemTypes type, ItemLayers layer) 
            : base(type)
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
                return (Layer & ItemLayers.MainHand) == ItemLayers.MainHand || (Layer & ItemLayers.TwoHanded) == ItemLayers.TwoHanded;
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

        public Weights Weight 
        {
            get { return m_Weight; }
            protected set
            {
                if (value == Weight)
                    return;
                m_Weight = value;
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
