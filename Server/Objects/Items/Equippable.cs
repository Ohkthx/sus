using System;
using SUS.Shared;

namespace SUS.Server.Objects.Items
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

    public abstract class Equippable : Item, IDestroyable
    {
        private ItemLayers _layer;
        private Weights _weight;

        protected int _durability;
        protected int _durabilityMax;
        private bool _invulnerable;

        #region Constructors

        protected Equippable(ItemTypes type, ItemLayers layer, int magicRating)
            : base(type)
        {
            Layer = layer;
            MagicRating = magicRating;
        }

        #endregion

        #region Getters / Setters

        public override string Name
        {
            get
            {
                if (IsBroken) return base.Name + " [Broken]";
                return base.Name;
            }
        }

        public bool IsArmor => (Type & ItemTypes.Armor) == ItemTypes.Armor;

        public bool IsWeapon => (Type & ItemTypes.Weapon) == ItemTypes.Weapon;

        public ItemLayers Layer
        {
            get => _layer;
            private set
            {
                if (value != ItemLayers.None && value != Layer) _layer = value;
            }
        }

        public Weights Weight
        {
            get => _weight;
            protected set
            {
                if (value == Weight) return;

                _weight = value;
            }
        }

        private int MagicRating { get; }

        protected abstract int RawRating { get; }

        public int Rating => RawRating + MagicRating;

        // Can be magical with either a negative(cursed) or positive rating.
        public bool IsMagical => MagicRating != 0;

        public int Durability
        {
            get => _durability;
            protected set
            {
                if (value < 0)
                    value = 0;
                else if (value > DurabilityMax)
                    value = DurabilityMax;

                _durability = value;
            }
        }

        public int DurabilityMax
        {
            get => _durabilityMax;
            protected set
            {
                if (value < 0)
                    value = 0;

                _durabilityMax = value;
            }
        }

        public bool Invulnerable
        {
            get => _invulnerable;
            protected set
            {
                if (value)
                {
                    DurabilityMax = 256;
                    Durability = 256;
                }

                _invulnerable = value;
            }
        }

        public bool IsBroken => !Invulnerable && Durability == 0;

        public bool IsStarter { get; protected set; }

        #endregion

        public bool DurabilityLoss()
        {
            // Do not take damage if the item is invulnerable or already broken.
            if (Invulnerable || IsBroken) return false;

            var lossChance = (int)(((float)Durability / (float)DurabilityMax) * 12);
            if (lossChance < 5)
                lossChance = 5;

            // Check if durability needs to be lost on the item. Return early if not.
            if (Utility.RandomMinMax(1, 100) > lossChance) return false;

            if (Durability > 0)
                --Durability; // Decrease the current durability by 1.

            return true;
        }
    }
}