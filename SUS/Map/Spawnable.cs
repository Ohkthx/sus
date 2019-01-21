using System;
using System.Collections.Concurrent;
using SUS.Objects;
using SUS.Shared;

namespace SUS
{
    public abstract class Spawnable : Node
    {
        private const int MINAXIS = 10;

        private static readonly ConcurrentDictionary<Guid, Spawner> m_Spawners =
            new ConcurrentDictionary<Guid, Spawner>();

        private int[,] m_Map;
        public Spawnables NPCs = Spawnables.None;

        #region Constructors

        public Spawnable(RegionType type, Regions region, string desc, int maxX, int maxY) : base(type, region, desc)
        {
            if (maxX < MINAXIS)
                maxX = MINAXIS;

            if (maxY < MINAXIS)
                maxY = MINAXIS;

            MaxX = maxX - 1;
            MaxY = maxY - 1;

            isSpawnable = true;
            m_Map = new int[maxY, maxX];
        }

        #endregion

        public void SpawnerAdd(int x, int y, int range, int limit)
        {
            if (NPCs == Spawnables.None)
                return;

            var spawner = new Spawner(Region, NPCs, x, y, range, limit, MaxX, MaxY);
            m_Spawners.TryAdd(spawner.ID, spawner);
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