using System;

namespace SUS.Objects.Items
{
    [Flags]
    public enum ItemLayers
    {
        None = 0x00000000,

        Melee = 0x00000001,
        Ranged = 0x00000002,

        MainHand = 0x00000004,
        Offhand = 0x00000008,
        TwoHanded = MainHand | Offhand,
        Bow = TwoHanded | Ranged,

        Armor = 0x00000010
    }

    public enum Weights
    {
        Light,
        Medium,
        Heavy
    }

    public abstract class Equippable : Item
    {
        private ItemLayers m_Layer;
        private Weights m_Weight;

        #region Constructors

        protected Equippable(ItemTypes type, ItemLayers layer, int magicRating)
            : base(type)
        {
            Layer = layer;
            MagicRating = magicRating;
        }

        #endregion

        #region Getters / Setters

        public bool IsArmor => !IsWeapon;

        public bool IsWeapon => (Layer & ItemLayers.MainHand) == ItemLayers.MainHand ||
                                (Layer & ItemLayers.TwoHanded) == ItemLayers.TwoHanded;

        public ItemLayers Layer
        {
            get => m_Layer;
            private set
            {
                if (value != ItemLayers.None && value != Layer) m_Layer = value;
            }
        }

        public Weights Weight
        {
            get => m_Weight;
            protected set
            {
                if (value == Weight) return;

                m_Weight = value;
            }
        }

        private int MagicRating { get; }

        protected abstract int RawRating { get; }

        public int Rating => RawRating + MagicRating;

        // Can be magical with either a negative(cursed) or positive rating.
        public bool IsMagical => MagicRating != 0;

        #endregion
    }
}