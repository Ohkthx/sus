using System;
using System.Collections.Concurrent;
using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map
{
    public abstract class Spawnable : Node
    {
        private const int MinAxis = 10;

        private static readonly ConcurrentDictionary<Guid, Spawner> _spawners =
            new ConcurrentDictionary<Guid, Spawner>();

        public Spawnables _NPCs = Spawnables.None;

        #region Constructors

        protected Spawnable(RegionType type, Shared.Regions region, string desc, int maxX, int maxY) : base(type,
            region, desc)
        {
            if (maxX < MinAxis)
                maxX = MinAxis;

            if (maxY < MinAxis)
                maxY = MinAxis;

            MaxX = maxX - 1;
            MaxY = maxY - 1;

            IsSpawnable = true;
        }

        #endregion

        public void SpawnerAdd(int x, int y, int range, int limit)
        {
            if (_NPCs == Spawnables.None)
                return;

            var spawner = new Spawner(Region, _NPCs, x, y, range, limit, MaxX, MaxY);
            _spawners.TryAdd(spawner.ID, spawner);
            Utility.ConsoleNotify($"Spawner created @{spawner.HomeLocation.ToString()} in {Region.ToString()}");
        }

        public override Point2D StartingLocation()
        {
            return new Point2D(MaxX / 2, MaxY / 2);
        }

        #region Getters / Setters

        public int MaxX { get; }
        public int MaxY { get; }

        #endregion
    }
}