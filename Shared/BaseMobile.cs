using System;

namespace SUS.Shared
{
    #region Enums

    [Flags]
    public enum MobileTypes
    {
        Player = 1,
        Npc = 2,
        Creature = 4,

        Mobile = Player | Npc | Creature
    }

    [Flags]
    public enum Directions
    {
        None = 0,

        North = 1,
        South = 2,
        East = 4,
        West = 8,

        Nearby = 16,

        NorthEast = North | East,
        NorthWest = North | West,
        SouthEast = South | East,
        SouthWest = South | West
    }

    public enum NpcTypes
    {
        None = 0,
        Armorsmith = 1,
        Weaponsmith = 2,
        Tailor = 4,
        Repairer = 8
    }

    #endregion

    [Serializable]
    public struct BaseMobile
    {
        private readonly string _name;
        private readonly bool _notEmpty;

        #region Constructors

        public BaseMobile(MobileTypes type, int serial, string name) : this()
        {
            Type = type;
            Serial = serial;
            _name = name;
            _notEmpty = true;
        }

        public int Serial { get; }

        #endregion

        #region Getters/Setters

        public MobileTypes Type { get; }

        public string Name => _name ?? "Unknown";

        public bool IsPlayer => Type == MobileTypes.Player;

        public bool IsEmpty => !_notEmpty;

        #endregion

        #region Overrides

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 13;
                hash = hash * 7 + Serial.GetHashCode();
                hash = hash * 7 + _name?.GetHashCode() ?? 0;
                hash = hash * 7 + Type.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(BaseMobile m1, BaseMobile m2)
        {
            return m1.Equals(m2);
        }

        public static bool operator !=(BaseMobile m1, BaseMobile m2)
        {
            return !(m1 == m2);
        }

        public override bool Equals(object value)
        {
            if (ReferenceEquals(null, value))
                return false;

            return value.GetType() == GetType() && IsEqual((BaseMobile) value);
        }

        private bool Equals(BaseMobile mobile)
        {
            return IsEqual(mobile);
        }

        private bool IsEqual(BaseMobile value)
        {
            return Type == value.Type && Serial == value.Serial;
        }

        #endregion
    }
}