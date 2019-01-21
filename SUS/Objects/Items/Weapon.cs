using System;
using SUS.Shared;

namespace SUS.Objects.Items
{
    public abstract class TwoHanded : Weapon
    {
        protected TwoHanded(Materials material, string damage)
            : base(ItemLayers.TwoHanded, material, damage)
        {
            Weight = Weights.Heavy;
        }
    }

    public abstract class OneHanded : Weapon
    {
        protected OneHanded(Materials material, string damage)
            : base(ItemLayers.MainHand, material, damage)
        {
            Weight = Weights.Light;
        }
    }

    public abstract class Weapon : Equippable
    {
        public enum Materials
        {
            None = 0,
            Wooden = 1,
            Iron = 2,
            Steel = 3
        }

        private int m_AttackRange;

        private DiceRoll m_Damage;
        private DamageTypes m_DamageType;
        private Materials m_Material = Materials.None;
        private SkillName m_Skill;
        private StatCode m_Stat;

        #region Constructors

        protected Weapon(ItemLayers layer, Materials material, string damage, int range = 1)
            : base(ItemTypes.Weapon, layer, 0)
        {
            if (material == Materials.None) material = Materials.Wooden;

            Material = material;
            Range = range;
            Weight = Weights.Medium;

            DiceDamage = new DiceRoll(damage);
        }

        #endregion

        #region Getters / Setters

        public override string Name => Material == Materials.None
            ? base.Name
            : $"{Enum.GetName(typeof(Materials), Material)} {base.Name}";

        protected override int RawRating => AttackRating;
        private int AttackRating => DiceDamage.Maximum + (int) Material;
        public int Damage => DiceDamage.Roll() + (int) Material;

        public bool IsBow => IsWeapon && (Layer & ItemLayers.Bow) == ItemLayers.Bow;

        private DiceRoll DiceDamage
        {
            get => m_Damage;
            set
            {
                if (value == null) return;

                m_Damage = value;
            }
        }

        protected Materials Material
        {
            private get => m_Material;
            set
            {
                if (value == Material) return;

                m_Material = value;
            }
        }

        public DamageTypes DamageType
        {
            get => m_DamageType == DamageTypes.None ? DamageTypes.Bludgeoning : m_DamageType;
            set
            {
                if (value == DamageTypes.None || value == DamageType) return;

                m_DamageType = value;
            }
        }

        public StatCode Stat
        {
            get => m_Stat;
            protected set
            {
                if (value == Stat) return;

                m_Stat = value;
            }
        }

        public SkillName RequiredSkill
        {
            get => m_Skill;
            protected set
            {
                if (value == RequiredSkill) return;

                m_Skill = value;
            }
        }

        public int Range
        {
            get => m_AttackRange;
            private set
            {
                if (value == Range)
                    return;
                if (value < 1) value = 1;

                m_AttackRange = value;
            }
        }

        #endregion
    }
}