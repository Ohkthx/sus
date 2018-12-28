using System;
using SUS.Shared;
using SUS.Objects.Items;
using SUS.Objects.Items.Equipment;

namespace SUS.Objects
{
    public abstract class BaseCreature : Mobile, ISpawnable
    {
        private int m_HitsMax = -1;
        private int m_StamMax = -1;
        private int m_ManaMax = -1;
        private int m_DamageMin = -1;
        private int m_DamageMax = -1;

        private ISpawner m_OwningSpawner;

        protected AI.Types m_AIType;
        protected DamageTypes m_DamageType = DamageTypes.None;

        #region Constructors
        public BaseCreature() 
            : base(MobileTypes.Creature)
        {
            StatCap = int.MaxValue;
        }
        #endregion

        #region Getters / Setters - Basic
        public ISpawner Spawner
        {
            get { return m_OwningSpawner; }
            set
            {
                if (value != Spawner)
                    m_OwningSpawner = value;
            }
        }

        public int CR { get { return HitsMax / 50; } }

        public void SetDamage(int val)
        {
            m_DamageMin = val;
            m_DamageMax = val;
        }

        public void SetDamage(int min, int max)
        {
            m_DamageMin = min;
            m_DamageMax = max;
        }
        #endregion

        #region Getters / Setters - Stats
        public void SetStr(int val)
        {
            RawStr = val;
            Hits = HitsMax;
        }

        public void SetStr(int min, int max)
        {
            RawStr = Utility.RandomMinMax(min, max);
            Hits = HitsMax;
        }

        public void SetDex(int val)
        {
            RawDex = val;
            Stam = StamMax;
        }

        public void SetDex(int min, int max)
        {
            RawDex = Utility.RandomMinMax(min, max);
            Stam = StamMax;
        }

        public void SetInt(int val)
        {
            RawInt = val;
            Mana = ManaMax;
        }

        public void SetInt(int min, int max)
        {
            RawInt = Utility.RandomMinMax(min, max);
            Mana = ManaMax;
        }

        public void SetHits(int val)
        {
            m_HitsMax = val;
            Hits = HitsMax;
        }

        public void SetHits(int min, int max)
        {
            m_HitsMax = Utility.RandomMinMax(min, max);
            Hits = HitsMax;
        }

        public void SetStam(int val)
        {
            m_StamMax = val;
            Stam = StamMax;
        }

        public void SetStam(int min, int max)
        {
            m_StamMax = Utility.RandomMinMax(min, max);
            Stam = StamMax;
        }

        public void SetMana(int val)
        {
            m_ManaMax = val;
            Mana = ManaMax;
        }

        public void SetMana(int min, int max)
        {
            m_ManaMax = Utility.RandomMinMax(min, max);
            Mana = ManaMax;
        }

        public override int HitsMax
        {
            get
            {
                if (m_HitsMax > 0)
                {
                    if (m_HitsMax > 1000000)
                        return 1000000;

                    return m_HitsMax;
                }

                return Str;
            }
        }

        public override int StamMax
        {
            get
            {
                if (m_StamMax > 0)
                {
                    if (m_StamMax > 1000000)
                        return 1000000;

                    return m_StamMax;
                }

                return Dex;
            }
        }

        public override int ManaMax
        {
            get
            {
                if (m_ManaMax > 0)
                {
                    if (m_ManaMax > 1000000)
                        return 1000000;

                    return m_ManaMax;
                }

                return Int;
            }
        }
        #endregion

        #region Getters / Setters - Combat
        public AI.Types AIType
        {
            get { return m_AIType; }
            set
            {
                if (value == AIType)
                    return;

                m_AIType = value;
            }
        }

        protected DamageTypes DamageOverride
        {
            get { return m_DamageType; }
            set
            {
                if (value == DamageTypes.None || value == DamageOverride)
                    return;

                m_DamageType = value;
            }
        }

        public override Weapon Weapon
        {
            get
            {
                Weapon weapon = base.Weapon;
                if (weapon is Unarmed && weapon.DamageType != DamageOverride)
                    weapon.DamageType = DamageOverride;

                return weapon;
            }
        }

        public override int AttackRating
        {
            get
            {   // If the creature is unarmed, the AttackRating is maximum damage.
                if (Weapon is Unarmed)
                    return m_DamageMax;

                // Return the base AttackRating instead.
                return base.AttackRating;
            }
        }

        public override int ArmorClass => base.ArmorClass;

        public override int ProficiencyModifier { get { return (CR / 4) + 2 > 9 ? 9 : (CR / 4) + 2; } }
        #endregion

        public void Spawned(ISpawner spawner, Regions region, Point2D location)
        {
            Spawner = spawner;
            Region = region;
            Location = location;
        }

        protected void SetSkill(SkillName skill, double min, double max)
        {
            if (min > max)
            {
                double tval = min;
                min = max;
                max = tval;
            }

            Skills[skill].Value = Utility.RandomMinMax(min, max);
        }

        #region Combat
        public override int Attack()
        {
            if (Weapon is Unarmed)
            {
                return Utility.RandomMinMax(m_DamageMin, m_DamageMax);
            }

            return Weapon.Damage + ProficiencyModifier;
        }

        public override void Kill()
        {
            Hits = 0;
            IsDeleted = true;
        }

        public override void Ressurrect()
        {
            Hits = HitsMax / 2;
            Mana = ManaMax / 2;
            Stam = StamMax / 2;
        }
        #endregion
    }
}
