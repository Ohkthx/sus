using System;
using System.Collections.Concurrent;
using SUS.Shared;
using SUS.Objects;

namespace SUS
{
    public abstract class Spawnable : Node
    {
        public Spawnables NPCs = Spawnables.None;
        private static ConcurrentDictionary<Guid, Spawner> m_Spawners = new ConcurrentDictionary<Guid, Spawner>();
        private const int MINAXIS = 10;
        private int m_MaxX = 0;
        private int m_MaxY = 0;
        private int[,] m_Map;

        #region Constructors
        public Spawnable(RegionType type, Regions region, string desc, int maxX, int maxY) : base(type, region, desc)
        {
            if (maxX < MINAXIS)
                maxX = MINAXIS;

            if (maxY < MINAXIS)
                maxY = MINAXIS;

            m_MaxX = maxX-1;
            m_MaxY = maxY-1;

            isSpawnable = true;
            m_Map = new int[maxY, maxX];
        }
        #endregion

        #region Getters / Setters
        public int MaxX { get { return m_MaxX; } }
        public int MaxY { get { return m_MaxY; } }
        #endregion

        public void SpawnerAdd(int x, int y, int range, int limit)
        {
            if (NPCs == Spawnables.None)
                return;

            Spawner spawner = new Spawner(Region, NPCs, x, y, range, limit, m_MaxX, m_MaxY);
            m_Spawners.TryAdd(spawner.Guid, spawner);
            Utility.ConsoleNotify($"Spawner created @{spawner.Location.ToString()} in {Region.ToString()}");
        }

        public override Point2D StartingLocation()
        {
            return new Point2D(MaxX / 2, MaxY / 2);
        }
    }
}
