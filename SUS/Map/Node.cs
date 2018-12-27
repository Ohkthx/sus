using System;
using System.Collections.Generic;
using SUS.Shared;

namespace SUS
{
    public abstract class Node
    {
        private string m_Name;
        private string m_Description = string.Empty;

        public bool isSpawnable { get; protected set; } = false;

        private RegionType m_Type;
        private Regions m_Region;
        private Regions m_Connections = Regions.None;

        #region Constructors
        public Node(RegionType type, Regions region, string description)
        {
            Name = Enum.GetName(typeof(Regions), region);
            Description = description;

            Type = type;
            Region = region;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            Node node = obj as Node;
            return this.ID == node.ID && (int)this.Region == (int)node.Region && (int)this.Type == (int)node.Type;
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash += this.ID.GetHashCode();
            hash *= 397;
            hash += this.Region.GetHashCode();
            hash *= 397;
            hash += this.Type.GetHashCode();
            return hash *= 397;
        }
        #endregion

        #region Getters / Setters
        public int ID
        {
            get { return (int)Region; }
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

        public RegionType Type
        {
            get { return m_Type; }
            set
            {
                if (value != m_Type)
                    m_Type = value;
            }
        }

        public Regions Region
        {
            get { return m_Region; }
            set
            {
                if (value != m_Region)
                    m_Region = value;
            }
        }

        public Regions Connections
        {
            get { return m_Connections; }
        }

        public int ConnectionsCount
        {
            get
            {
                int value = 0;
                foreach (Regions region in Enum.GetValues(typeof(Regions)))
                    if ((Connections & region) == region)
                        ++value;
                return value;
            }
        }
        #endregion

        #region Updates
        public void AddConnection(Regions connections)
        {
            if (connections == Regions.None || connections == Region)
            {   // Location is empty or our location, do not add.
                return;
            }
            else if ((connections & Region) == Region)
            {   // Extract the location and remove it from connections.
                connections &= ~Region;
            }

            m_Connections |= connections;
        }

        public bool HasConnection(Regions connection)
        {
            return ((Connections & connection) == connection);
        }

        /// <summary>
        ///     Converts a string to a Location, then attempts to verify that the parsed Location is a connection.
        /// </summary>
        /// <param name="location">String data to be parsed.</param>
        /// <returns>Location if one is found, returns Locations.None if not found.</returns>
        public Regions StringToConnection(string location)
        {
            if (location == null)
                return Regions.None;

            foreach (Regions region in Enum.GetValues(typeof(Regions)))
            {
                if (region == Regions.None)          // A connection cannot be 'None'
                    continue;
                else if ((region & (region - 1)) != 0)    // Check if this is not a power of two (indicating it is a combination location)
                    continue;                       //  It was a combination.
                else if (!HasConnection(region))       // Validate if it is not a connection.
                    continue;                       //  It is not a connection, return.

                if (location.ToLower() == Enum.GetName(typeof(Regions), region).ToLower())
                {
                    return region;
                }
            }

            // Location never found through string parsing.
            return Regions.None;
        }

        public List<Regions> ConnectionsToList()
        {
            List<Regions> conn = new List<Regions>();

            foreach (Regions region in Enum.GetValues(typeof(Regions)))
            {
                if (region == Regions.None || (region & (region - 1)) != 0)
                    continue;                   // Prevention of processing combination locations.

                if (HasConnection(region))         // Checks if location is in our current connections.
                {
                    conn.Add(region);
                }
            }

            return conn;
        }
        #endregion

        /// <summary>
        ///     Validates that the location is not invalid and not a combination of locations.
        /// </summary>
        /// <param name="loc">Location to evaluate.</param>
        public static bool isValidRegion(Regions loc)
        {   // Check if it is not a 'None' location. If it is not, verifies that it is a power of 2.
            return loc != Regions.None && (loc & (loc - 1)) == 0;
        }

        public abstract Point2D StartingLocation();

        public BaseRegion GetBase()
        {
            return new BaseRegion(Type, Region, Connections, isSpawnable);
        }
    }
}
