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

        public int Distance(Coordinate to) { return Distance(to.X, to.Y); }
        public int Distance(int x, int y)
        {
            return (int)Math.Sqrt(
                    Math.Pow(X - x, 2)
                    + Math.Pow(Y - y, 2)
                    );
        }
    }

    [Serializable]
    public abstract class Item
    {
        protected Guid m_Guid;
        protected string m_Name;
        protected ItemTypes m_Type;
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

        public ItemTypes Type
        {
            get { return m_Type; }
            protected set
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
