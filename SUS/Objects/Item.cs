using System;
using SUS.Shared;

namespace SUS.Objects
{
    #region Enums
    [Flags]
    public enum ItemTypes
    {
        None = 0x00000000,

        Consumable = 0x00000001,

        // Equippables
        Armor = 0x00000002,
        Weapon = 0x00000004,

        Equippable = Armor | Weapon,
    }

    public enum ConsumableTypes
    {
        Gold,
        Arrows,
        Bolts,
        Bandages,
        HealthPotion,
        ManaPotion,
    }
    #endregion

    public abstract class Item
    {
        protected Serial m_Serial;
        protected string m_Name;
        private ItemTypes m_Type;
        protected bool m_isDestroyable;

        #region Constructors
        protected Item(ItemTypes type)
        {
            m_Serial = Serial.NewItem;
            Type = type;
            World.AddItem(this);
        }
        #endregion

        #region Getters / Setters
        public Serial Serial { get { return m_Serial; } }

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

        public bool IsEquippable { get { return (ItemTypes.Equippable & Type) == Type; } }

        public bool IsDestroyable { get { return m_isDestroyable; } }
        #endregion

        #region Overrides
        public override string ToString() { return Name; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, Serial) ? Serial.GetHashCode() : 0);
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
                && (Serial == value.Serial);
        }
        #endregion
    }
}
