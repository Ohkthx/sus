using System;

namespace SUS.Shared.Utilities
{
    [Serializable]
    public enum SkillCode
    {
        Archery,
        Fencing,
        Healing,
        Macefighting,
        Magery,
        Swordsmanship,
        Wrestling,
    }

    [Serializable]
    public class Skill
    {
        private enum TimerLevels
        {   // LTE(Skill Level) = Time in seconds.
            LTE30  = 1,     // Chance to gain: 50%
            LTE45  = 15,
            LTE60  = 30,    // Chance to gain: 40%
            LTE70  = 45,
            LTE80  = 60,    // Chance to gain: 30%
            LTE90  = 80,
            LTE100 = 100,   // Chance to gain: 20%
            LTE105 = 120,   // Chance to gain: 10%
            LTE110 = 180,  
            LTE115 = 240,
            LTE120 = 300,
        }

        private string m_Name;
        private SkillCode m_Type;
        private double m_Value;
        private double m_Cap;
        private Timer m_Timer;

        #region Constructors
        public Skill(string name, SkillCode type, double value = 0.0, double cap = 100.0)
        {
            Name = name;
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

        public SkillCode Type
        {
            get { return m_Type; }
            private set
            {
                if (value != Type)
                    m_Type = value;
            }
        }

        public double Value
        {
            get { return m_Value; }
            set
            {
                if (value == Value)
                    return;

                if (value < 0.0)
                    value = 0.0;
                else if (value > Cap)
                    value = Cap;

                m_Value = value;
            }
        }

        public double Cap 
        {
            get { return m_Cap; }
            set
            {
                if (value == Cap)
                    return;
                else if (value < Value)
                    return;

                if (value < 0.0)
                    value = 0.0;
                else if (value > 200.0)
                    value = 200.0;

                m_Cap = value;
            }
        }
        #endregion

        #region Overrides
        public static Skill operator ++(Skill s) { return s + 0.1; }
        public static Skill operator +(Skill s, double amt)
        {
            if (amt <= 0.0)
                return s;
            else if (s.Value == s.Cap)
                return s;

            if (s.Value + amt > s.Cap)
                s.Value = s.Cap;
            else
                s.Value = s.Value + amt;

            return s;
        }

        public static Skill operator --(Skill s) { return s - 0.1; }
        public static Skill operator -(Skill s, double amt)
        {
            if (amt <= 0.0)
                return s;
            else if (s.Value == 0.0)
                return s;

            if (s.Value - amt < 0.0)
                s.Value = 0.0;
            else
                s.Value = s.Value - amt;

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

            int time;       // Time in miliseconds that is required to pass to increase in skill.
            int chance;     // Chance that the skill will be increased.
            double amount;  // Amount to increase the skill by.

            switch (Value)
            {
                case double n when (n <= 30.0):
                    chance = 5;
                    amount = 1.0;
                    time = (int)TimerLevels.LTE30 * 1000;
                    break;
                case double n when (n <= 45.0):
                    chance = 5;
                    amount = 0.5;
                    time = (int)TimerLevels.LTE45 * 1000;
                    break;
                case double n when (n <= 60.0):
                    chance = 4;
                    amount = 0.2;
                    time = (int)TimerLevels.LTE60 * 1000;
                    break;
                case double n when (n <= 70.0):
                    chance = 4;
                    amount = 0.2;
                    time = (int)TimerLevels.LTE70 * 1000;
                    break;
                case double n when (n <= 80.0):
                    chance = 3;
                    amount = 0.1;
                    time = (int)TimerLevels.LTE80 * 1000;
                    break;
                case double n when (n <= 90.0):
                    chance = 3;
                    amount = 0.1;
                    time = (int)TimerLevels.LTE90 * 1000;
                    break;
                case double n when (n <= 100.0):
                    chance = 2;
                    amount = 0.1;
                    time = (int)TimerLevels.LTE100 * 1000;
                    break;
                case double n when (n <= 105.0):
                    chance = 1;
                    amount = 0.1;
                    time = (int)TimerLevels.LTE105 * 1000;
                    break;
                case double n when (n <= 110.0):
                    chance = 1;
                    amount = 0.1;
                    time = (int)TimerLevels.LTE110 * 1000;
                    break;
                case double n when (n <= 115.0):
                    chance = 1;
                    amount = 0.1;
                    time = (int)TimerLevels.LTE115 * 1000;
                    break;
                default:
                    chance = 1;
                    amount = 0.1;
                    time = (int)TimerLevels.LTE120 * 1000;
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
