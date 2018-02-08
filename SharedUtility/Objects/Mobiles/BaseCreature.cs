using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SUS.Shared.Utility;

namespace SUS.Shared.Objects.Mobiles
{
    class BaseCreature : Mobile
    {
        #region Sets
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

        public void SetHits(int val)
        {
            m_HitsMax = m_Hits = val;
        }

        public void SetHits(int min, int max)
        {
            m_HitsMax = m_Hits = RandomImpl.Next(min, max);
        }

        public void SetStr(int val)
        {
            m_Attributes.Strength = val;
        }

        public void SetStr(int min, int max)
        {
            m_Attributes.Strength = RandomImpl.Next(min, max);
        }

        public void SetDex(int val)
        {
            m_Attributes.Dexterity = val;
        }

        public void SetDex(int min, int max)
        {
            m_Attributes.Dexterity = RandomImpl.Next(min, max);
        }

        public void SetInt(int val)
        {
            m_Attributes.Intelligence = val;
        }

        public void SetInt(int min, int max)
        {
            m_Attributes.Intelligence = RandomImpl.Next(min, max);
        }
        #endregion
    }
}
