using System;

namespace SUS.Shared
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

    [Serializable]
    public struct BaseItem
    {
        private readonly string _name;
        private readonly bool _notEmpty;

        #region Constructors

        public BaseItem(ItemTypes type, string name, int serial)
        {
            Type = type;
            _name = name;
            _notEmpty = true;
            Serial = serial;
        }

        #endregion

        #region Getters / Setters

        public string Name => _name ?? "Unknown";

        public ItemTypes Type { get; }

        public bool IsEmpty => !_notEmpty;

        public int Serial { get; }

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

        public static bool operator ==(BaseItem i1, BaseItem i2)
        {
            return i1.Equals(i2);
        }

        public static bool operator !=(BaseItem i1, BaseItem i2)
        {
            return !(i1 == i2);
        }

        public override bool Equals(object value)
        {
            if (ReferenceEquals(null, value))
                return false;

            return value.GetType() == GetType() && IsEqual((BaseItem) value);
        }

        private bool Equals(BaseItem item)
        {
            return IsEqual(item);
        }

        private bool IsEqual(BaseItem value)
        {
            return Type == value.Type
                   && Serial == value.Serial;
        }

        #endregion
    }
}