using System;

namespace SUS.Shared.Utilities
{
    [Serializable]
    public class Skill
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public double Value { get; set; }
        public double Max { get; set; }
        public double Step { get; }

        public enum Types { Archery, Magery, Fencing, Healing };

        public Skill(string name, int type, double value = 0, double max = 120.0)
        {
            this.Step = 0.1;
            if (max > 120.0)
                this.Max = max;
            else
                this.Max = 120.0;

            this.Name = name;
            this.Type = type;
            this.Value = value;
        }
    }
}
