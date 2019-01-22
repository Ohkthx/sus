using System;

namespace SUS.Shared
{
    #region Enums

    [Flags]
    public enum RegionType
    {
        Town = 1,
        Dungeon = 2,
        OpenWorld = 4,
        PvP = 8
    }

    [Flags]
    public enum Regions
    {
        None = 0x00000000,
        Moongate = 0x00000001,

        Unused1 = 0x00000002,

        Britain = 0x00000004,
        BuccaneersDen = 0x00000008,
        Cove = 0x00000010,
        Minoc = 0x00000020,
        SkaraBrae = 0x00000040,
        Trinsic = 0x00000080,
        Vesper = 0x00000100,
        Yew = 0x00000200,

        Unused2 = 0x00000400,

        Destard = 0x00000800,
        Despise = 0x00001000,
        Covetous = 0x00002000,
        Shame = 0x00004000,
        Wind = 0x00008000,
        Wrong = 0x00010000,

        Unused3 = 0x00020000,
        Unused4 = 0x00040000,

        SolenHive = 0x00080000,
        OrcCaves = 0x00100000,

        Unused5 = 0x00200000,
        Unused6 = 0x00400000,
        Unused7 = 0x00800000,

        Graveyard = 0x01000000,
        Sewers = 0x02000000,
        Swamp = 0x04000000,
        Wilderness = 0x08000000,

        Unused8 = 0x10000000,
        Unused9 = 0x20000000,
        Unused10 = 0x40000000,

        Basic = Britain | Graveyard | Sewers | Wilderness
    }

    #endregion

    [Serializable]
    public struct BaseRegion
    {
        public bool Navigable { get; }

        #region Constructors

        public BaseRegion(RegionType type, Regions region, Regions connections, bool navigable)
        {
            Type = type;
            Location = region;
            Connections = connections;
            Navigable = navigable;
        }

        #endregion

        #region Getters / Settersa

        private string Name => Enum.GetName(typeof(Regions), Location);

        private RegionType Type { get; }

        public Regions Location { get; }

        public Regions Connections { get; }

        public bool IsValid => Location != Regions.None && (Location & (Location - 1)) == 0;

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
                hash = hash * 7 + Location.GetHashCode();
                hash = hash * 7 + Type.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(BaseRegion v1, BaseRegion v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(BaseRegion v1, BaseRegion v2)
        {
            return !(v1 == v2);
        }

        public override bool Equals(object value)
        {
            if (ReferenceEquals(null, value)) return false;

            return value.GetType() == GetType() && IsEqual((BaseRegion) value);
        }

        private bool Equals(BaseRegion value)
        {
            return IsEqual(value);
        }

        private bool IsEqual(BaseRegion value)
        {
            return Type == value.Type
                   && Location == value.Location;
        }

        #endregion
    }
}