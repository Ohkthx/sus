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
        private static readonly int[] ManaTable = new int[] { 4, 6, 9, 11, 14, 20, 40, 50 };

        private SpellCircle m_Circle;

        protected Spell(SpellName name, SpellCircle circle)
        {
            Name = Enum.GetName(typeof(SpellName), name);
            Circle = circle;
        }

        #region Getters / Setters

        private static string Name { get; set; }

        protected int ManaRequired => ManaTable[(int)Circle];

        private SpellCircle Circle
        {
            get => m_Circle;
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
