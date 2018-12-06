using System;

namespace SUS.Shared.Objects
{
    [Serializable, Flags]
    public enum ItemTypes
    {
        None        = 0x00000000,

        Consumable  = 0x00000001,

        // Equippables
        Armor       = 0x00000002,
        Weapon      = 0x00000004,

        Equippable = Armor | Weapon,
    }


    [Serializable]
    public sealed class Coordinate
    {
        private int m_Xaxis = 0;
        private int m_Yaxis = 0;

        public Coordinate(int x, int y)
        {
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;

            X = x;
            Y = y;
        }

        #region Getters / Setters
        public int X
        {
            get { return m_Xaxis; }
            set
            {
                if (value < 0 || value == X)
                    return;

                m_Xaxis = value;
            }
        }

        public int Y
        {
            get { return m_Yaxis; }
            set
            {
                if (value < 0 || value == Y)
                    return;

                m_Yaxis = value;
            }
        }
        #endregion

        #region Overrides
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, X) ? X.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, Y) ? Y.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(Coordinate c1, Coordinate c2)
        {
            if (Object.ReferenceEquals(c1, c2)) return true;
            if (Object.ReferenceEquals(null, c1)) return false;
            return (c1.Equals(c2));
        }

        public static bool operator !=(Coordinate c1, Coordinate c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            if (value.GetType() != this.GetType()) return false;
            return IsEqual((Coordinate)value);
        }

        public bool Equals(Coordinate mobile)
        {
            if (Object.ReferenceEquals(null, mobile)) return false;
            if (Object.ReferenceEquals(this, mobile)) return true;
            return IsEqual(mobile);
        }

        private bool IsEqual(Coordinate value)
        {
            return (value != null)
                && (X == value.X)
                && (Y == value.Y);
        }
        #endregion

        public int Distance(Coordinate to) { return Distance(to.X, to.Y); }
        public int Distance(int x, int y)
        {
            return (int)Math.Sqrt(
                    Math.Pow(X - x, 2)
                    + Math.Pow(Y - y, 2)
                    );
        }

        public Coordinate Midpoint(Coordinate to) { return Midpoint(to.X, to.Y); }
        public Coordinate Midpoint(int x, int y)
        {
            return new Coordinate((X + x) / 2, (Y + y) / 2);
        }

        public void MoveTowards(Coordinate to, int speed) { MoveTowards(to.X, to.Y, speed); }
        public void MoveTowards(int x2, int y2, int speed)
        {   // Bresenham's line algorithm; provided by: Frank Lioty @StackOverflow
            int w = x2 - X;
            int h = y2 - Y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }

            int numerator = longest >> 1;
            for (int i = 0; i < speed; i++)
            {
                if (i == longest)
                    break;

                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    X += dx1;
                    Y += dy1;
                }
                else
                {
                    X += dx2;
                    Y += dy2;
                }
            }
        }
    }

    [Serializable]
    public abstract class Item
    {
        protected Guid m_Guid;
        protected string m_Name;
        private ItemTypes m_Type;
        protected int m_Weight;
        protected bool m_isDestroyable;

        #region Constructors
        protected Item(ItemTypes type)
        {
            Type = type;
        }
        #endregion

        #region Getters / Setters
        public Guid Guid
        {
            get
            {
                if (m_Guid == null || m_Guid == Guid.Empty)
                    m_Guid = Guid.NewGuid();

                return m_Guid;
            }
        }

        public virtual string Name
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

        public ItemTypes Type
        {
            get { return m_Type; }
            private set
            {
                if (value != ItemTypes.None && value != Type)
                    m_Type = value;
            }
        }

        public int Weight 
        {
            get { return m_Weight; }
            protected set
            {
                if (value != Weight)
                    m_Weight = value;
            }
        }

        public bool IsEquippable { get { return (ItemTypes.Equippable & Type) == Type; } }

        public bool IsDestroyable { get { return m_isDestroyable; } }
        #endregion

        #region Overrides
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, Guid) ? Guid.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, Name) ? Name.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, Type) ? Type.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(Item i1, Item i2)
        {
            if (Object.ReferenceEquals(i1, i2)) return true;
            if (Object.ReferenceEquals(null, i1)) return false;
            return (i1.Equals(i2));
        }

        public static bool operator !=(Item i1, Item i2)
        {
            return !(i1 == i2);
        }

        public override bool Equals(object value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            if (value.GetType() != this.GetType()) return false;
            return IsEqual((Item)value);
        }

        public bool Equals(Item mobile)
        {
            if (Object.ReferenceEquals(null, mobile)) return false;
            if (Object.ReferenceEquals(this, mobile)) return true;
            return IsEqual(mobile);
        }

        private bool IsEqual(Item value)
        {
            return (value != null)
                && (Type == value.Type)
                && (Guid == value.Guid);
        }
        #endregion
    }
}
