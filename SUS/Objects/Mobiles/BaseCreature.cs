using SUS.Objects.Items;
using SUS.Objects.Items.Equipment;
using SUS.Shared;

namespace SUS.Objects.Mobiles
{
    public abstract class BaseCreature : Mobile, ISpawnable
    {
        private Ai.Types m_AiType;
        private int m_DamageMax = -1;
        private int m_DamageMin = -1;
        private DamageTypes m_DamageType = DamageTypes.None;
        private int m_HitsMax = -1;
        private int m_ManaMax = -1;

        private ISpawner m_OwningSpawner;
        private int m_StaminaMax = -1;

        #region Constructors

        protected BaseCreature()
            : base(MobileTypes.Creature)
        {
            StatCap = int.MaxValue;
        }

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
                var val = min;
                min = max;
                max = val;
            }

            Skills[skill].Value = Utility.RandomMinMax(min, max);
        }

        #region Getters / Setters - Basic

        public ISpawner Spawner
        {
            get => m_OwningSpawner;
            set
            {
                if (value != Spawner)
                    m_OwningSpawner = value;
            }
        }

        private int CR => HitsMax / 50;

        protected void SetDamage(int min, int max)
        {
            m_DamageMin = min;
            m_DamageMax = max;
        }

        #endregion

        #region Getters / Setters - Stats

        protected void SetStr(int val)
        {
            RawStr = val;
            Hits = HitsMax;
        }

        protected void SetStr(int min, int max)
        {
            RawStr = Utility.RandomMinMax(min, max);
            Hits = HitsMax;
        }

        protected void SetDex(int val)
        {
            RawDex = val;
            Stamina = StaminaMax;
        }

        protected void SetDex(int min, int max)
        {
            RawDex = Utility.RandomMinMax(min, max);
            Stamina = StaminaMax;
        }

        protected void SetInt(int val)
        {
            RawInt = val;
            Mana = ManaMax;
        }

        protected void SetInt(int min, int max)
        {
            RawInt = Utility.RandomMinMax(min, max);
            Mana = ManaMax;
        }

        protected void SetHits(int val)
        {
            m_HitsMax = val;
            Hits = HitsMax;
        }

        protected void SetHits(int min, int max)
        {
            m_HitsMax = Utility.RandomMinMax(min, max);
            Hits = HitsMax;
        }

        protected void SetStamina(int val)
        {
            m_StaminaMax = val;
            Stamina = StaminaMax;
        }

        protected void SetStamina(int min, int max)
        {
            m_StaminaMax = Utility.RandomMinMax(min, max);
            Stamina = StaminaMax;
        }

        protected void SetMana(int val)
        {
            m_ManaMax = val;
            Mana = ManaMax;
        }

        protected void SetMana(int min, int max)
        {
            m_ManaMax = Utility.RandomMinMax(min, max);
            Mana = ManaMax;
        }

        public override int HitsMax
        {
            get
            {
                if (m_HitsMax <= 0) return Str;

                return m_HitsMax > 1000000 ? 1000000 : m_HitsMax;
            }
        }

        protected override int StaminaMax
        {
            get
            {
                if (m_StaminaMax > 0)
                {
                    if (m_StaminaMax > 1000000)
                        return 1000000;

                    return m_StaminaMax;
                }

                return Dex;
            }
        }

        protected override int ManaMax
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

        protected Ai.Types AiType
        {
            private get => m_AiType;
            set
            {
                if (value == AiType)
                    return;

                m_AiType = value;
            }
        }

        protected DamageTypes DamageOverride
        {
            private get => m_DamageType;
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
                var weapon = base.Weapon;
                if (weapon is Unarmed && weapon.DamageType != DamageOverride)
                    weapon.DamageType = DamageOverride;

                return weapon;
            }
        }

        public override int AttackRating => Weapon is Unarmed ? m_DamageMax : base.AttackRating;

        protected override int ProficiencyModifier => CR / 4 + 2 > 9 ? 9 : CR / 4 + 2;

        #endregion

        #region Combat

        public override int Attack()
        {
            if (Weapon is Unarmed) return Utility.RandomMinMax(m_DamageMin, m_DamageMax);

            return Weapon.Damage + ProficiencyModifier;
        }

        public override void Kill()
        {
            Hits = 0;
            IsDeleted = true;
        }

        public override void Resurrect()
        {
            Hits = HitsMax / 2;
            Mana = ManaMax / 2;
            Stamina = StaminaMax / 2;
        }

        #endregion
    }
}