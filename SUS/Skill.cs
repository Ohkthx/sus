using SUS.Shared;
using System;

namespace SUS
{
    public enum SkillName
    {
        Archery,
        Fencing,
        Healing,
        Macefighting,
        Magery,
        Swordsmanship,
        Wrestling,
    }

    public class Skill
    {
        private enum TimerLevels
        {   // LTE(Skill Level) = Time in seconds.
            Lte30 = 1,     // Chance to gain: 50%
            Lte45 = 15,
            Lte60 = 30,    // Chance to gain: 40%
            Lte70 = 45,
            Lte80 = 60,    // Chance to gain: 30%
            Lte90 = 80,
            Lte100 = 100,   // Chance to gain: 20%
            Lte105 = 120,   // Chance to gain: 10%
            Lte110 = 180,
            Lte115 = 240,
            Lte120 = 300,
        }

        private string m_Name;
        private SkillName m_Type;
        private double m_Value;
        private double m_Cap;
        private readonly Timer m_Timer;

        #region Constructors
        public Skill(SkillName type, double value = 0.0, double cap = 100.0)
        {
            Name = Enum.GetName(typeof(SkillName), type);
            Type = type;
            Cap = cap;
            Value = value;

            m_Timer = new Timer();
            m_Timer.Start();
        }
        #endregion

        #region Getters / Setters
        public string Name
        {
            get => m_Name ?? "Unknown";
            private set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    m_Name = value;
                }
            }
        }

        private SkillName Type
        {
            get => m_Type;
            set
            {
                if (value != Type)
                {
                    m_Type = value;
                }
            }
        }

        public double Value
        {
            get => m_Value;
            set
            { 
                if (value < 0.0)
                {
                    value = 0.0;
                }
                else if (value > Cap)
                {
                    value = Cap;
                }

                m_Value = value;
            }
        }

        public double Cap
        {
            get => m_Cap;
            private set
            {
                if (value < Value)
                {
                    return;
                }

                if (value < 0.0)
                {
                    value = 0.0;
                }
                else if (value > 200.0)
                {
                    value = 200.0;
                }

                m_Cap = value;
            }
        }
        #endregion

        #region Overrides
        public static Skill operator ++(Skill s) { return s + 0.1; }
        public static Skill operator +(Skill s, double amt)
        {
            if (amt <= 0.0)
            {
                return s;
            }

            if (s.Value >= s.Cap)
            {
                return s;
            }

            if (s.Value + amt > s.Cap)
            {
                s.Value = s.Cap;
            }
            else
            {
                s.Value = s.Value + amt;
            }

            return s;
        }

        public static Skill operator --(Skill s) { return s - 0.1; }
        public static Skill operator -(Skill s, double amt)
        {
            if (amt <= 0.0)
            {
                return s;
            }

            if (s.Value <= 0.0)
            {
                s.Value = 0.0;
                return s;
            }

            if (s.Value - amt < 0.0)
            {
                s.Value = 0.0;
            }
            else
            {
                s.Value = s.Value - amt;
            }

            return s;
        }
        #endregion

        public double Increase()
        {
            if (Value >= Cap)
            {   // No need to try and increase skill.
                m_Timer.Restart();
                return 0.0;
            }

            int time;       // Time in milliseconds that is required to pass to increase in skill.
            int chance;     // Chance that the skill will be increased.
            double amount;  // Amount to increase the skill by.

            switch (Value)
            {
                case double n when (n <= 30.0):
                    chance = 5;
                    amount = 1.0;
                    time = (int)TimerLevels.Lte30 * 1000;
                    break;
                case double n when (n <= 45.0):
                    chance = 5;
                    amount = 0.5;
                    time = (int)TimerLevels.Lte45 * 1000;
                    break;
                case double n when (n <= 60.0):
                    chance = 4;
                    amount = 0.2;
                    time = (int)TimerLevels.Lte60 * 1000;
                    break;
                case double n when (n <= 70.0):
                    chance = 4;
                    amount = 0.2;
                    time = (int)TimerLevels.Lte70 * 1000;
                    break;
                case double n when (n <= 80.0):
                    chance = 3;
                    amount = 0.1;
                    time = (int)TimerLevels.Lte80 * 1000;
                    break;
                case double n when (n <= 90.0):
                    chance = 3;
                    amount = 0.1;
                    time = (int)TimerLevels.Lte90 * 1000;
                    break;
                case double n when (n <= 100.0):
                    chance = 2;
                    amount = 0.1;
                    time = (int)TimerLevels.Lte100 * 1000;
                    break;
                case double n when (n <= 105.0):
                    chance = 1;
                    amount = 0.1;
                    time = (int)TimerLevels.Lte105 * 1000;
                    break;
                case double n when (n <= 110.0):
                    chance = 1;
                    amount = 0.1;
                    time = (int)TimerLevels.Lte110 * 1000;
                    break;
                case double n when (n <= 115.0):
                    chance = 1;
                    amount = 0.1;
                    time = (int)TimerLevels.Lte115 * 1000;
                    break;
                default:
                    chance = 1;
                    amount = 0.1;
                    time = (int)TimerLevels.Lte120 * 1000;
                    break;

            }

            int d11 = Utility.RandomMinMax(0, 10);
            if (d11 >= chance && m_Timer.ElapsedTime > time)
            {   // chance is right, and timer is exceeded.
                m_Timer.Restart();
                double tValue = Value;
                double skillAmt = Math.Round(Utility.RandomMinMax(0.1, amount), 1);
                Value += skillAmt;
                return Value - tValue;
            }

            return 0.0;
        }
    }
}
