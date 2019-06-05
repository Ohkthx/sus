using System;
using SUS.Shared;

namespace SUS.Server.Objects.Items
{
    public abstract class TwoHanded : Weapon
    {
        protected TwoHanded(Materials material, string damage)
            : base(ItemLayers.TwoHanded, material, damage)
        {
            Weight = Weights.Heavy;
            DurabilityMax = 80;
            Durability = 80;
        }
    }

    public abstract class OneHanded : Weapon
    {
        protected OneHanded(Materials material, string damage)
            : base(ItemLayers.MainHand, material, damage)
        {
            Weight = Weights.Light;
            DurabilityMax = 60;
            Durability = 60;
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

        private int _attackRange;

        private DiceRoll _damage;
        private DamageTypes _damageType;
        private Materials _material = Materials.None;
        private SkillName _skill;
        private StatCode _stat;

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
            get => _damage;
            set
            {
                if (value == null) return;

                _damage = value;
            }
        }

        protected Materials Material
        {
            private get => _material;
            set
            {
                if (value == Material) return;

                _material = value;
            }
        }

        public DamageTypes DamageType
        {
            get => _damageType == DamageTypes.None ? DamageTypes.Bludgeoning : _damageType;
            set
            {
                if (value == DamageTypes.None || value == DamageType) return;

                _damageType = value;
            }
        }

        public StatCode Stat
        {
            get => _stat;
            protected set
            {
                if (value == Stat) return;

                _stat = value;
            }
        }

        public SkillName RequiredSkill
        {
            get => _skill;
            protected set
            {
                if (value == RequiredSkill) return;

                _skill = value;
            }
        }

        public int Range
        {
            get => _attackRange;
            private set
            {
                if (value == Range)
                    return;
                if (value < 1) value = 1;

                _attackRange = value;
            }
        }

        #endregion
    }
}