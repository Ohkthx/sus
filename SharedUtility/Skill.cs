using System;

namespace SUS.Shared.Utilities
{
    [Serializable]
    public class Skill
    {
        public enum Types
        {
            Archery,
            Magery,
            Fencing,
            Healing,
        }

        private string m_Name;
        private Types m_Type;
        private double m_Value;
        private double m_Cap;

        #region Constructors
        public Skill(string name, Types type, double value = 0.0, double cap = 100.0)
        {
            Name = name;
            Type = type;
            Cap = cap;
            Value = value;
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

        public Types Type
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
    }
}
