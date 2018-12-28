using System;
using System.Collections.Generic;

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
    };

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
        private RegionType m_Type;
        private Regions m_Region;
        private Regions m_Connections;
        public bool CanTraverse { get; private set; }

        #region Constructors
        public BaseRegion(RegionType type, Regions region, Regions connections, bool isTraversable)
        {
            m_Type = type;
            m_Region = region;
            m_Connections = connections;
            CanTraverse = isTraversable;
        }
        #endregion

        #region Getters / Settersa
        public string Name { get { return Enum.GetName(typeof(Regions), Location); } }

        public RegionType Type
        {
            get { return m_Type; }
            private set
            {
                if (value != m_Type)
                    m_Type = value;
            }
        }

        public Regions Location
        {
            get { return m_Region; }
            private set
            {
                if (value != m_Region)
                    m_Region = value;
            }
        }

        public Regions Connections
        {
            get { return m_Connections; }
            private set
            {
                if (value != Connections)
                    m_Connections = value;
            }
        }

        public bool IsValid { get { return Location != Regions.None && (Location & (Location - 1)) == 0; } }
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
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_Region) ? m_Region.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_Type) ? m_Type.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(BaseRegion v1, BaseRegion v2)
        {
            if (Object.ReferenceEquals(v1, v2)) return true;
            if (Object.ReferenceEquals(null, v1)) return false;
            return (v1.Equals(v2));
        }

        public static bool operator !=(BaseRegion v1, BaseRegion v2)
        {
            return !(v1 == v2);
        }

        public override bool Equals(object value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            if (value.GetType() != this.GetType()) return false;
            return IsEqual((BaseRegion)value);
        }

        public bool Equals(BaseRegion value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            return IsEqual(value);
        }

        private bool IsEqual(BaseRegion value)
        {
            return (value != null)
                && (m_Type == value.m_Type)
                && (m_Region == value.m_Region);

        }
        #endregion
    }
}
