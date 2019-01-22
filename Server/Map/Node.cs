using System;
using SUS.Shared;

namespace SUS.Server.Map
{
    public abstract class Node
    {
        private string _description = string.Empty;
        private string _name;

        #region Constructors

        protected Node(RegionType type, Shared.Regions region, string description)
        {
            Name = Enum.GetName(typeof(Shared.Regions), region);
            Description = description;

            Type = type;
            Region = region;
        }

        #endregion

        public bool IsSpawnable { get; protected set; } = false;

        /// <summary>
        ///     Validates that the location is not invalid and not a combination of locations.
        /// </summary>
        /// <param name="loc">Location to evaluate.</param>
        public static bool IsValidRegion(Shared.Regions loc)
        {
            // Check if it is not a 'None' location. If it is not, verifies that it is a power of 2.
            return loc != Shared.Regions.None && (loc & (loc - 1)) == 0;
        }

        public abstract Point2D StartingLocation();

        public BaseRegion GetBase()
        {
            return new BaseRegion(Type, Region, Connections, IsSpawnable);
        }

        #region Overrides

        public override bool Equals(object obj)
        {
            if (!(obj is Node node)) return false;
            return Id == node.Id && (int) Region == (int) node.Region && (int) Type == (int) node.Type;
        }

        public override int GetHashCode()
        {
            var hash = 37;
            hash += Id.GetHashCode();
            hash *= 397;
            hash += Region.GetHashCode();
            hash *= 397;
            hash += Type.GetHashCode();
            return hash * 397;
        }

        #endregion

        #region Getters / Setters

        public int Id => (int) Region;

        public string Name
        {
            get => _name ?? "Unknown";
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _name = value;
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (value != null && value != _description)
                    _description = value;
            }
        }

        public RegionType Type { get; }

        public Shared.Regions Region { get; }

        public Shared.Regions Connections { get; private set; } = Shared.Regions.None;

        public int ConnectionsCount
        {
            get
            {
                var value = 0;
                foreach (Shared.Regions region in Enum.GetValues(typeof(Shared.Regions)))
                    if ((Connections & region) == region)
                        ++value;
                return value;
            }
        }

        #endregion

        #region Updates

        public void AddConnection(Shared.Regions connections)
        {
            if (connections == Shared.Regions.None || connections == Region)
                return;
            if ((connections & Region) == Region) connections &= ~Region;

            Connections |= connections;
        }

        public bool HasConnection(Shared.Regions connection)
        {
            return (Connections & connection) == connection;
        }

        /// <summary>
        ///     Converts a string to a Location, then attempts to verify that the parsed Location is a connection.
        /// </summary>
        /// <param name="location">String data to be parsed.</param>
        /// <returns>Location if one is found, returns Locations.None if not found.</returns>
        public Shared.Regions StringToConnection(string location)
        {
            if (location == null)
                return Shared.Regions.None;

            foreach (Shared.Regions region in Enum.GetValues(typeof(Shared.Regions)))
            {
                if (region == Shared.Regions.None) // A connection cannot be 'None'
                    continue;
                if ((region & (region - 1)) != 0
                ) // Check if this is not a power of two (indicating it is a combination location)
                    continue; //  It was a combination.
                if (!HasConnection(region)) // Validate if it is not a connection.
                    continue; //  It is not a connection, return.

                if (string.Equals(location, Enum.GetName(typeof(Shared.Regions), region),
                    StringComparison.CurrentCultureIgnoreCase)) return region;
            }

            // Location never found through string parsing.
            return Shared.Regions.None;
        }

        #endregion
    }
}