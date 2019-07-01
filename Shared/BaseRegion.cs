using System;
using System.ComponentModel;

namespace SUS.Shared
{
    #region Enums

    [Flags]
    public enum RegionTypes
    {
        Town = 1,
        Dungeon = 2,
        OpenWorld = 4,
        PvP = 8
    }

    [Flags]
    public enum Regions
    {
        None = 0,
        Moongate = 1 << 0,

        Wilderness = 1 << 1,

        Britain = 1 << 2,
        BuccaneersDen = 1 << 3,
        Cove = 1 << 4,
        Minoc = 1 << 5,
        SkaraBrae = 1 << 6,
        Trinsic = 1 << 7,
        Vesper = 1 << 8,
        Yew = 1 << 9,

        Destard = 1 << 10,
        Despise = 1 << 11,
        Covetous = 1 << 12,
        Shame = 1 << 13,
        Wind = 1 << 14,
        Wrong = 1 << 15,

        SolenHive = 1 << 16,
        OrcCaves = 1 << 17,

        Graveyard = 1 << 18,
        Sewers = 1 << 19,
        Swamp = 1 << 20
    }

    #endregion

    [Serializable]
    public struct BaseRegion
    {
        public bool Navigable { get; }

        #region Constructors

        public BaseRegion(RegionTypes type, Regions region, Regions connections, bool navigable)
        {
            if (!IsValidRegionId(region))
                throw new InvalidEnumArgumentException(nameof(region), (int) region, typeof(Regions));

            Type = type;
            Id = region;
            Connections = connections;
            Navigable = navigable;
        }

        #endregion

        /// <summary>
        ///     Checks if the Region is valid and not a combination of regions.
        /// </summary>
        /// <param name="region">Region to check against.</param>
        public static bool IsValidRegionId(Regions region)
        {
            return region != Regions.None && (region & (region - 1)) == 0;
        }

        #region Getters / Setters

        private string Name => Enum.GetName(typeof(Regions), Id);

        private RegionTypes Type { get; }

        public Regions Id { get; }

        public Regions Connections { get; }

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
                hash = hash * 7 + Id.GetHashCode();
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
            if (ReferenceEquals(null, value))
                return false;

            return value.GetType() == GetType() && IsEqual((BaseRegion) value);
        }

        private bool Equals(BaseRegion value)
        {
            return IsEqual(value);
        }

        private bool IsEqual(BaseRegion value)
        {
            return Type == value.Type
                   && Id == value.Id;
        }

        #endregion
    }
}