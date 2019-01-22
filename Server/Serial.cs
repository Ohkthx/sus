using System;

namespace SUS.Server
{
    public struct Serial : IComparable, IComparable<Serial>
    {
        private static readonly Serial MinusOne = new Serial(-1);
        private static readonly Serial Zero = new Serial(0);

        private static Serial LastMobile { get; set; } = Zero;
        private static Serial LastItem { get; set; } = 0x40000000;


        public static Serial NewMobile
        {
            get
            {
                while (World.FindMobile(LastMobile = LastMobile + 1) != null)
                {
                }

                return LastMobile;
            }
        }

        public static Serial NewItem
        {
            get
            {
                while (World.FindItem(LastItem = LastItem + 1) != null)
                {
                }

                return LastItem;
            }
        }

        private Serial(int serial)
        {
            Value = serial;
        }

        private int Value { get; }

        public bool IsMobile => Value > 0 && Value < 0x40000000;

        public bool IsItem => Value >= 0x40000000;

        public bool IsValid => Value > 0;

        #region Overrides

        public override int GetHashCode()
        {
            return Value;
        }

        public int CompareTo(Serial other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(object other)
        {
            switch (other)
            {
                case Serial serial:
                    return CompareTo(serial);
                case null:
                    return -1;
                default:
                    throw new ArgumentException();
            }
        }

        public override bool Equals(object o)
        {
            if (!(o is Serial)) return false;

            return ((Serial) o).Value == Value;
        }

        public static bool operator ==(Serial l, Serial r)
        {
            return l.Value == r.Value;
        }

        public static bool operator !=(Serial l, Serial r)
        {
            return l.Value != r.Value;
        }

        public static bool operator >(Serial l, Serial r)
        {
            return l.Value > r.Value;
        }

        public static bool operator <(Serial l, Serial r)
        {
            return l.Value < r.Value;
        }

        public static bool operator >=(Serial l, Serial r)
        {
            return l.Value >= r.Value;
        }

        public static bool operator <=(Serial l, Serial r)
        {
            return l.Value <= r.Value;
        }

        public override string ToString()
        {
            return $"0x{Value:X8}";
        }

        public static implicit operator int(Serial a)
        {
            return a.Value;
        }

        public static implicit operator Serial(int a)
        {
            return new Serial(a);
        }

        #endregion
    }
}