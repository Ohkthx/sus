using SUS.Shared;

namespace SUS.Server.Objects
{
    public abstract class Item
    {
        private string _name;
        private IEntity _owner;
        private ItemTypes _type;

        #region Constructors

        protected Item(ItemTypes type)
        {
            Serial = Serial.NewItem;
            Type = type;
            World.AddItem(this);
        }

        #endregion

        public BaseItem Base()
        {
            return new BaseItem(Type, Name, Serial);
        }

        #region Getters / Setters

        public Serial Serial { get; }

        public IEntity Owner
        {
            get => _owner;
            set
            {
                if (value != null)
                    _owner = value;
            }
        }

        public virtual string Name
        {
            get => _name ?? "Unknown";
            protected set
            {
                if (string.IsNullOrEmpty(value))
                    value = "Unknown";

                _name = value;
            }
        }

        public ItemTypes Type
        {
            get => _type;
            private set
            {
                if (value != ItemTypes.None && value != Type)
                    _type = value;
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
            if (ReferenceEquals(i1, i2))
                return true;

            return !ReferenceEquals(null, i1) && i1.Equals(i2);
        }

        public static bool operator !=(Item i1, Item i2)
        {
            return !(i1 == i2);
        }

        public override bool Equals(object value)
        {
            if (ReferenceEquals(null, value))
                return false;

            if (ReferenceEquals(this, value))
                return true;

            return value.GetType() == GetType() && IsEqual((Item) value);
        }

        private bool Equals(Item item)
        {
            if (ReferenceEquals(null, item))
                return false;

            return ReferenceEquals(this, item) || IsEqual(item);
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