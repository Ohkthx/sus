using System;
using SUS.Objects;

namespace SUS.Spells
{
    public enum SpellCircle
    {
        First,
        Second,
        Third,
        Forth,
        Fifth,
        Sixth,
        Seventh,
        Eighth
    }

    public enum SpellName
    {
        Heal,
        Fireball,
        Explosion,
        Flamestrike,
        Lightning,
    }

    public abstract class Spell
    {
        private static readonly int[] m_ManaTable = new int[] { 4, 6, 9, 11, 14, 20, 40, 50 };

        private string m_Name;
        private SpellCircle m_Circle;

        public Spell(SpellName name, SpellCircle circle)
        {
            Name = Enum.GetName(typeof(SpellName), name);
            Circle = circle;
        }

        #region Getters / Setters
        public string Name
        {
            get
            {
                if (m_Name != null)
                    return m_Name;
                else
                    return "Unknown";
            }
            set
            {
                if (value != m_Name)
                    m_Name = value;
            }
        }

        public int ManaRequired { get { return m_ManaTable[(int)Circle]; } }

        public SpellCircle Circle
        {
            get { return m_Circle; }
            set
            {
                if (value == Circle)
                    return;

                m_Circle = value;
            }
        }
        #endregion

        public abstract int Effect(Mobile caster, Mobile target);
    }
}
