using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles
{
    [Serializable]
    public abstract class BaseCreature : Mobile
    {
        private int m_HitsMax = -1;
        private int m_StamMax = -1;
        private int m_ManaMax = -1;
        private int m_DamageMin = -1;
        private int m_DamageMax = -1;

        #region Constructors
        public BaseCreature() : base(MobileType.NPC) { ID = Serial.NewObject; }
        #endregion

        #region Getters / Setters
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

        #region Combat
        public override int Attack()
        {
            return Utility.RandomMinMax(m_DamageMax / 2, m_DamageMax);
        }

        public override void Kill() { Hits = 0; }

        public override void Ressurrect()
        {
            Hits = HitsMax / 2;
            Mana = ManaMax / 2;
            Stam = StamMax / 2;
        }
        #endregion
    }
}
