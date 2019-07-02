using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SUS.Server.Objects.Mobiles;
using SUS.Shared;

namespace SUS.Server.Map
{
    public abstract class Region
    {
        private readonly ConcurrentDictionary<NpcTypes, Npc> _localNpcs;
        private string _description = string.Empty;
        private string _name;

        #region Constructors

        protected Region(RegionTypes type, Regions id)
        {
            Name = Enum.GetName(typeof(Regions), id);

            Type = type;
            Id = id;

            _localNpcs = new ConcurrentDictionary<NpcTypes, Npc>();
        }

        #endregion

        /// <summary>
        ///     Validates that the location is not invalid and not a combination of locations.
        /// </summary>
        /// <param name="loc">Location to evaluate.</param>
        public static bool IsValidRegion(Regions loc)
        {
            // Check if it is not a 'None' location. If it is not, verifies that it is a power of 2.
            return loc != Regions.None && (loc & (loc - 1)) == 0;
        }

        public abstract Point2D StartingLocation();

        public BaseRegion GetBase()
        {
            return new BaseRegion(Type, Id, ConnectedRegions, IsSpawnable);
        }

        #region Overrides

        public override bool Equals(object obj)
        {
            if (!(obj is Region node))
                return false;

            return (int) Id == (int) node.Id && (int) Type == (int) node.Type;
        }

        public override int GetHashCode()
        {
            var hash = 37;
            hash += Id.GetHashCode();
            hash *= 397;
            hash += Type.GetHashCode();
            return hash * 397;
        }

        #endregion

        #region Getters / Setters

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
            protected set
            {
                if (value != null && value != _description)
                    _description = value;
            }
        }

        public virtual BaseRegion Base { get; }

        public RegionTypes Type { get; }

        public Regions Id { get; }

        public Regions ConnectedRegions { get; protected set; }

        public int TotalConnectedCount
        {
            get
            {
                var value = 0;
                foreach (Regions region in Enum.GetValues(typeof(Regions)))
                {
                    if ((ConnectedRegions & region) == region)
                        ++value;
                }

                return value;
            }
        }

        public bool IsSpawnable { get; protected set; } = false;

        #endregion

        #region Updates

        protected void AddConnection(Regions connections)
        {
            if (connections == Regions.None || connections == Id)
                return;

            if ((connections & Id) == Id)
                connections &= ~Id;

            ConnectedRegions |= connections;
        }

        public bool HasConnection(Regions connection)
        {
            return (ConnectedRegions & connection) == connection;
        }

        public Npc FindNpc(NpcTypes type)
        {
            return _localNpcs.TryGetValue(type, out var npc) ? npc : null;
        }

        protected bool AddNpc(Npc npc)
        {
            return _localNpcs.TryAdd(npc.NpcType, npc);
        }

        public Dictionary<int, NpcTypes> GetLocalNpcs(bool includeNone = false)
        {
            var npcs = new Dictionary<int, NpcTypes>();
            if (includeNone)
                npcs[0] = NpcTypes.None;

            var i = 0;
            foreach (var (key, _) in _localNpcs)
            {
                ++i;
                npcs[i] = key;
            }

            return npcs;
        }

        #endregion
    }
}