using SUS.Server.Objects.Items;
using SUS.Server.Objects.Items.Equipment;
using SUS.Shared;

namespace SUS.Server.Objects.Mobiles
{
    public abstract class BaseCreature : Mobile, ISpawnable
    {
        private Ai.Types _aiType;
        private int _damageMax = -1;
        private int _damageMin = -1;
        private DamageTypes _damageType = DamageTypes.None;
        private int _hitsMax = -1;
        private int _manaMax = -1;

        private ISpawner _owningSpawner;
        private int _staminaMax = -1;

        #region Constructors

        protected BaseCreature(SpawnTypes spawnType)
            : base(MobileTypes.Creature)
        {
            SpawnType = spawnType;
            StatCap = int.MaxValue;

            // Give access to all regions by default.
            AddRegionAccess((Regions) ~0);
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

        public SpawnTypes SpawnType { get; }

        public ISpawner Spawner
        {
            get => _owningSpawner;
            set
            {
                if (value != Spawner)
                    _owningSpawner = value;
            }
        }

        private int CR => HitsMax / 50;

        protected void SetDamage(int min, int max)
        {
            _damageMin = min;
            _damageMax = max;
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
            _hitsMax = val;
            Hits = HitsMax;
        }

        protected void SetHits(int min, int max)
        {
            _hitsMax = Utility.RandomMinMax(min, max);
            Hits = HitsMax;
        }

        protected void SetStamina(int val)
        {
            _staminaMax = val;
            Stamina = StaminaMax;
        }

        protected void SetStamina(int min, int max)
        {
            _staminaMax = Utility.RandomMinMax(min, max);
            Stamina = StaminaMax;
        }

        protected void SetMana(int val)
        {
            _manaMax = val;
            Mana = ManaMax;
        }

        protected void SetMana(int min, int max)
        {
            _manaMax = Utility.RandomMinMax(min, max);
            Mana = ManaMax;
        }

        public override int HitsMax
        {
            get
            {
                if (_hitsMax <= 0)
                    return Str;

                return _hitsMax > 1000000 ? 1000000 : _hitsMax;
            }
        }

        protected override int StaminaMax
        {
            get
            {
                if (_staminaMax > 0)
                {
                    if (_staminaMax > 1000000)
                        return 1000000;

                    return _staminaMax;
                }

                return Dex;
            }
        }

        protected override int ManaMax
        {
            get
            {
                if (_manaMax > 0)
                {
                    if (_manaMax > 1000000)
                        return 1000000;

                    return _manaMax;
                }

                return Int;
            }
        }

        #endregion

        #region Getters / Setters - Combat

        protected Ai.Types AiType
        {
            private get => _aiType;
            set
            {
                if (value == AiType)
                    return;

                _aiType = value;
            }
        }

        protected DamageTypes DamageOverride
        {
            private get => _damageType;
            set
            {
                if (value == DamageTypes.None || value == DamageOverride)
                    return;

                _damageType = value;
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

        public override int AttackRating => Weapon is Unarmed ? _damageMax : base.AttackRating;

        protected override int ProficiencyModifier => CR / 4 + 2 > 9 ? 9 : CR / 4 + 2;

        #endregion

        #region Combat

        public override int Attack()
        {
            if (Weapon is Unarmed)
                return Utility.RandomMinMax(_damageMin, _damageMax);

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