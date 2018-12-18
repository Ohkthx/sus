using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects
{
    [Serializable]
    public abstract class TwoHanded : Weapon
    {
        public TwoHanded(Materials material, string damage) :
            base(ItemLayers.TwoHanded, material, damage)
        {
            Weight = Weights.Heavy;
        }
    }

    [Serializable]
    public abstract class OneHanded : Weapon
    {
        public OneHanded(Materials material, string damage) :
            base(ItemLayers.MainHand, material, damage)
        {
            Weight = Weights.Light;
        }
    }

    [Serializable]
    public abstract class Weapon : Equippable
    {
        public enum Materials
        {
            None    = 0,
            Wooden  = 1,
            Iron    = 2,
            Steel   = 3,
        }

        public enum DamageTypes
        {
            Bludgeoning,
            Piercing,
            Slashing,
        }

        private DiceRoll m_Damage;
        private int m_AttackRange;
        private Materials m_Material = Materials.None;
        private DamageTypes m_DamageType;
        private StatCode m_Stat;
        private SkillCode m_Skill;

        #region Constructors
        public Weapon(ItemLayers layer, Materials material, string damage, int range = 1) : base(ItemTypes.Weapon, layer)
        {
            if (material == Materials.None)
                material = Materials.Wooden;

            Material = material;
            Range = range;
            Weight = Weights.Medium;

            DiceDamage = new DiceRoll(damage);
        }
        #endregion

        #region Getters / Setters
        public override string Name
        { get { return Material == Materials.None ? base.Name : $"{Enum.GetName(typeof(Materials), Material)} {base.Name}"; } }

        public override int RawRating { get { return AttackRating; } }
        private int AttackRating { get { return DiceDamage.Maximum + (int)Material; } }
        public int Damage { get { return DiceDamage.Roll() + (int)Material; } }

        public bool IsBow
        {
            get { return IsWeapon && (Layer & ItemLayers.Bow) == ItemLayers.Bow; }
        }

        private DiceRoll DiceDamage
        {
            get { return m_Damage; }
            set
            {
                if (value == null)
                    return;
                m_Damage = value;
            }
        }

        public Materials Material
        {
            get { return m_Material; }
            protected set
            {
                if (value == Material)
                    return;

                m_Material = value;
            }
        }

        public DamageTypes DamageType
        {
            get { return m_DamageType; }
            protected set
            {
                if (value == DamageType)
                    return;

                m_DamageType = value;
            }
        }

        public StatCode Stat
        {
            get { return m_Stat; }
            protected set
            {
                if (value == Stat)
                    return;

                m_Stat = value;
            }
        }

        public SkillCode RequiredSkill
        {
            get { return m_Skill; }
            protected set
            {
                if (value == RequiredSkill)
                    return;

                m_Skill = value;
            }
        }

        public int Range
        {
            get { return m_AttackRange; }
            private set
            {
                if (value == Range)
                    return;
                else if (value < 1)
                    value = 1;

                m_AttackRange = value;
            }
        }
        #endregion
    }
}
