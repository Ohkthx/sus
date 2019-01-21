using System;
using SUS.Shared;

namespace SUS
{
    public abstract class Node
    {
        private string m_Description = string.Empty;
        private string m_Name;
        private Regions m_Region;

        private RegionType m_Type;

        #region Constructors

        public Node(RegionType type, Regions region, string description)
        {
            Name = Enum.GetName(typeof(Regions), region);
            Description = description;

            Type = type;
            Region = region;
        }

        #endregion

        public bool isSpawnable { get; protected set; } = false;

        /// <summary>
        ///     Validates that the location is not invalid and not a combination of locations.
        /// </summary>
        /// <param name="loc">Location to evaluate.</param>
        public static bool isValidRegion(Regions loc)
        {
            // Check if it is not a 'None' location. If it is not, verifies that it is a power of 2.
            return loc != Regions.None && (loc & (loc - 1)) == 0;
        }

        public abstract Point2D StartingLocation();

        public BaseRegion GetBase()
        {
            return new BaseRegion(Type, Region, Connections, isSpawnable);
        }

        #region Overrides

        public override bool Equals(object obj)
        {
            var node = obj as Node;
            return ID == node.ID && (int) Region == (int) node.Region && (int) Type == (int) node.Type;
        }

        public override int GetHashCode()
        {
            var hash = 37;
            hash += ID.GetHashCode();
            hash *= 397;
            hash += Region.GetHashCode();
            hash *= 397;
            hash += Type.GetHashCode();
            return hash *= 397;
        }

        #endregion

        #region Getters / Setters

        public int ID => (int) Region;

        public string Name
        {
            get
            {
                if (m_Name != null)
                    return m_Name;
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
            get => m_Description;
            set
            {
                if (value != null && value != m_Description)
                    m_Description = value;
            }
        }

        public RegionType Type
        {
            get => m_Type;
            set
            {
                if (value != m_Type)
                    m_Type = value;
            }
        }

        public Regions Region
        {
            get => m_Region;
            set
            {
                if (value != m_Region)
                    m_Region = value;
            }
        }

        public Regions Connections { get; private set; } = Regions.None;

        public int ConnectionsCount
        {
            get
            {
                var value = 0;
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
                return;
            if ((connections & Region) == Region) connections &= ~Region;

            Connections |= connections;
        }

        public bool HasConnection(Regions connection)
        {
            return (Connections & connection) == connection;
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
                if (region == Regions.None) // A connection cannot be 'None'
                    continue;
                if ((region & (region - 1)) != 0
                ) // Check if this is not a power of two (indicating it is a combination location)
                    continue; //  It was a combination.
                if (!HasConnection(region)) // Validate if it is not a connection.
                    continue; //  It is not a connection, return.

                if (location.ToLower() == Enum.GetName(typeof(Regions), region).ToLower()) return region;
            }

            // Location never found through string parsing.
            return Regions.None;
        }

        #endregion
    }
}