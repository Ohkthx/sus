using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects
{
    [Serializable]
    public enum WeaponMaterials
    {
        None    = 0,
        Wooden  = 1,
        Iron    = 2,
        Steel   = 3,
    }

    [Serializable]
    public class Weapon : Equippable
    {
        private DiceRoll m_Damage;
        private int m_AttackMinimum;
        private int m_AttackMaximum;
        private WeaponMaterials m_Material = WeaponMaterials.None;

        #region Constructors
        public Weapon(ItemLayers layer, WeaponMaterials material, string name, string damage) : base(ItemTypes.Weapon, layer)
        {
            Name = name;

            DiceDamage = new DiceRoll(damage);
            MinimumDMG = ((DiceDamage.Dice + DiceDamage.Modifier) > 0) ? DiceDamage.Dice + DiceDamage.Modifier : 0;
            MaximumDMG = ((DiceDamage.Dice * DiceDamage.Faces + DiceDamage.Modifier) > 0) ? DiceDamage.Dice * DiceDamage.Faces + DiceDamage.Modifier : 0;

            Material = material;  // Acts like a damage modifier.
        }
        #endregion

        #region Getters / Setters
        public override int RawRating { get { return AttackRating; } }

        private int AttackRating
        {
            get
            {
                return (MaximumDMG) + (int)Material;
            }
        }

        public bool IsBow
        {
            get { return IsWeapon && (Layer & ItemLayers.Bow) == ItemLayers.Bow; }
        }

        public int Damage { get { return DiceDamage.Roll() + (int)Material; } }

        public DiceRoll DiceDamage
        {
            get { return m_Damage; }
            private set
            {
                if (value == null)
                    return;
                m_Damage = value;
            }
        }

        public WeaponMaterials Material
        {
            get { return m_Material; }
            private set
            {
                if (value == WeaponMaterials.None || value == Material)
                    return;

                m_Material = value;
            }
        }

        public int MinimumDMG
        {
            get { return m_AttackMinimum; }
            private set
            {
                if (value < 0 || value > MaximumDMG || value == MinimumDMG)
                    return;

                m_AttackMinimum = value;
            }
        }

        public int MaximumDMG
        {
            get { return m_AttackMaximum; }
            private set
            {
                if (value < 0 || value < MinimumDMG || value == MaximumDMG)
                    return;

                m_AttackMaximum = value;
            }
        }
        #endregion
    }
}
