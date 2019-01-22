using System;

namespace SUS.Server.Objects
{
    #region Enums

    [Flags]
    public enum ItemTypes
    {
        None = 0x00000000,

        Consumable = 0x00000001,

        // Equippable
        Armor = 0x00000002,
        Weapon = 0x00000004,

        Equippable = Armor | Weapon
    }

    public enum ConsumableTypes
    {
        Gold,
        Arrows,
        Bandages,
        HealthPotion
    }

    #endregion

    public abstract class Item
    {
        private string m_Name;
        private IEntity m_Owner;
        private ItemTypes m_Type;

        #region Constructors

        protected Item(ItemTypes type)
        {
            Serial = Serial.NewItem;
            Type = type;
            World.AddItem(this);
        }

        #endregion

        #region Getters / Setters

        public Serial Serial { get; }

        public IEntity Owner
        {
            get => m_Owner;
            set
            {
                if (value != null)
                    m_Owner = value;
            }
        }

        public virtual string Name
        {
            get => m_Name ?? "Unknown";
            protected set
            {
                if (string.IsNullOrEmpty(value))
                    value = "Unknown";

                m_Name = value;
            }
        }

        public ItemTypes Type
        {
            get => m_Type;
            private set
            {
                if (value != ItemTypes.None && value != Type)
                    m_Type = value;
            }
        }

        public bool IsEquippable => (ItemTypes.Equippable & Type) == Type;

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 13;
                hash = hash * 7 + Serial.GetHashCode();
                hash = hash * 7 + Type.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Item i1, Item i2)
        {
            if (ReferenceEquals(i1, i2)) return true;
            return !ReferenceEquals(null, i1) && i1.Equals(i2);
        }

        public static bool operator !=(Item i1, Item i2)
        {
            return !(i1 == i2);
        }

        public override bool Equals(object value)
        {
            if (ReferenceEquals(null, value)) return false;
            if (ReferenceEquals(this, value)) return true;
            return value.GetType() == GetType() && IsEqual((Item) value);
        }

        private bool Equals(Item mobile)
        {
            if (ReferenceEquals(null, mobile)) return false;
            return ReferenceEquals(this, mobile) || IsEqual(mobile);
        }

        private bool IsEqual(Item value)
        {
            return value != null
                   && Type == value.Type
                   && Serial == value.Serial;
        }

        #endregion
    }
}