using System;

namespace SUS.Shared.Objects
{
    [Serializable]
    public enum WeaponMaterials
    {
        Wooden  = 1,
        Iron    = 2,
        Steel   = 3,
    }

    [Serializable]
    public class Weapon : Equippable
    {
        private int m_AttackRating;

        #region Constructors
        public Weapon(ItemLayers layer, WeaponMaterials material, string name) : base(ItemTypes.Weapon, layer)
        {
            Name = name;
            AttackRating = (int)material;
        }
        #endregion

        #region Getters / Setters
        public override int RawRating { get { return AttackRating; } }

        public int AttackRating
        {
            get { return m_AttackRating; }
            private set
            {
                if (value != AttackRating)
                    m_AttackRating = value;
            }
        }

        public bool IsBow
        {
            get { return IsWeapon && (Layer & ItemLayers.Bow) == ItemLayers.Bow; }
        }
        #endregion
    }
}
