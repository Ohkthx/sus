using System;
using System.Collections.Generic;
using SUS.Shared.Utilities;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Objects
{
    [Flags, Serializable]
    public enum Types
    {
        Town = 1,
        Dungeon = 2,
        OpenWorld = 4,
        PvP = 8
    };

    [Flags, Serializable]
    public enum Locations
    {
        None          = 0x00000000,
        Moongate      = 0x00000001,

        Unused1       = 0x00000002,

        Britain       = 0x00000004,
        BuccaneersDen = 0x00000008,
        Cove          = 0x00000010,
        Minoc         = 0x00000020,
        SkaraBrae     = 0x00000040,
        Trinsic       = 0x00000080,
        Vesper        = 0x00000100,
        Yew           = 0x00000200,

        Unused2       = 0x00000400,

        Destard       = 0x00000800,
        Despise       = 0x00001000,
        Covetous      = 0x00002000,
        Shame         = 0x00004000,
        Wind          = 0x00008000,
        Wrong         = 0x00010000,

        Unused3       = 0x00020000,
        Unused4       = 0x00040000,

        SolenHive     = 0x00080000,
        OrcCaves      = 0x00100000,

        Unused5       = 0x00200000,
        Unused6       = 0x00400000,
        Unused7       = 0x00800000,

        Graveyard     = 0x01000000,
        Sewers        = 0x02000000,
        Swamp         = 0x04000000,
        Wilderness    = 0x08000000,

        Unused8       = 0x10000000,
        Unused9       = 0x20000000,
        Unused10      = 0x40000000,

        Basic         = Britain | Graveyard | Sewers | Wilderness
    }

    [Serializable]
    public abstract class Node
    {
        private string m_Name;
        private string m_Description = string.Empty;

        public bool isSpawnable { get; protected set; } = false;
        //private HashSet<MobileTag> m_Mobiles;

        private Types m_Type;
        private Locations m_Location;
        private Locations m_Connections = Locations.None;

        #region Constructors
        public Node(Types type, Locations location, string description)
        {
            Name = Enum.GetName(typeof(Locations), location);
            Description = description;

            Type = type;
            Location = location;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            Node node = obj as Node;
            return this.ID == node.ID && (int)this.Location == (int)node.Location && (int)this.Type == (int)node.Type;
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash += this.ID.GetHashCode();
            hash *= 397;
            hash += this.Location.GetHashCode();
            hash *= 397;
            hash += this.Type.GetHashCode();
            return hash *= 397;
        }
        #endregion

        #region Getters / Setters
        public int ID
        {
            get { return (int)Location; }
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

        public string Description
        {
            get { return m_Description; }
            set
            {
                if (value != null && value != m_Description)
                    m_Description = value;
            }
        }

        public Types Type
        {
            get { return m_Type; }
            set
            {
                if (value != m_Type)
                    m_Type = value;
            }
        }

        public Locations Location
        {
            get { return m_Location; }
            set
            {
                if (value != m_Location)
                    m_Location = value;
            }
        }

        public Locations Connections
        {
            get { return m_Connections; }
        }

        public int ConnectionsCount
        {
            get
            {
                int value = 0;
                foreach (Locations loc in Enum.GetValues(typeof(Locations)))
                    if ((Connections & loc) == loc)
                        ++value;
                return value;
            }
        }
        #endregion

        #region Updates
        public void AddConnection(Locations connections)
        {
            if (connections == Locations.None || connections == Location)
            {   // Location is empty or our location, do not add.
                return;
            }
            else if ((connections & Location) == Location)
            {   // Extract the location and remove it from connections.
                connections &= ~Location;
            }

            m_Connections |= connections;
        }

        public bool HasConnection(Locations connection)
        {
            return ((Connections & connection) == connection);
        }

        /// <summary>
        ///     Converts a string to a Location, then attempts to verify that the parsed Location is a connection.
        /// </summary>
        /// <param name="location">String data to be parsed.</param>
        /// <returns>Location if one is found, returns Locations.None if not found.</returns>
        public Locations StringToConnection(string location)
        {
            if (location == null)
                return Locations.None;

            foreach (Locations loc in Enum.GetValues(typeof(Locations)))
            {
                if (loc == Locations.None)          // A connection cannot be 'None'
                    continue;
                else if ((loc & (loc - 1)) != 0)    // Check if this is not a power of two (indicating it is a combination location)
                    continue;                       //  It was a combination.
                else if (!HasConnection(loc))       // Validate if it is not a connection.
                    continue;                       //  It is not a connection, return.

                if (location.ToLower() == Enum.GetName(typeof(Locations), loc).ToLower())
                {
                    return loc;
                }
            }

            // Location never found through string parsing.
            return Locations.None;
        }

        public List<Locations> ConnectionsToList()
        {
            List<Locations> conn = new List<Locations>();

            foreach (Locations loc in Enum.GetValues(typeof(Locations)))
            {
                if (loc == Locations.None || (loc & (loc - 1)) != 0)
                    continue;                   // Prevention of processing combination locations.

                if (HasConnection(loc))         // Checks if location is in our current connections.
                {
                    conn.Add(loc);
                }
            }

            return conn;
        }
        #endregion

        /// <summary>
        ///     Validates that the location is not invalid and not a combination of locations.
        /// </summary>
        /// <param name="loc">Location to evaluate.</param>
        public static bool isValidLocation(Locations loc)
        {   // Check if it is not a 'None' location. If it is not, verifies that it is a power of 2.
            return loc != Locations.None && (loc & (loc - 1)) == 0;
        }
    }
}
