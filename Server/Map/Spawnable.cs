using System;
using System.Collections.Concurrent;
using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map
{
    public abstract class Spawnable : Region
    {
        private static readonly ConcurrentDictionary<Guid, Spawner> Spawners =
            new ConcurrentDictionary<Guid, Spawner>();

        private static readonly ConcurrentBag<Zone> UnlockableAreas =
            new ConcurrentBag<Zone>();

        #region Constructors

        protected Spawnable(RegionTypes types, Regions id, int width, int length) : base(types, id)
        {
            Dimensions = new Dimension2D(width, length);
            IsSpawnable = true;
        }

        #endregion

        protected void AddSpawner(int x, int y, int range, int limit, SpawnTypes spawns)
        {
            if (spawns == SpawnTypes.None)
                return;

            var spawner = new Spawner(Id, spawns, x, y, range, limit, MaxX, MaxY);
            Spawners.TryAdd(spawner.ID, spawner);
            Utility.ConsoleNotify($"Spawner created @{spawner.HomeLocation.ToString()} in {Id.ToString()}");
        }

        /// <summary>
        ///     Add a subzone to this region.
        /// </summary>
        /// <param name="parentRegion">The region the subzone leads to.</param>
        /// <param name="suggestedLocation">Coordinates of entrance.</param>
        /// <param name="width">Width of the zone.</param>
        /// <param name="length">Length of the zone.</param>
        protected void AddZone(Regions parentRegion, IPoint2D suggestedLocation, int width, int length)
        {
            var zone = new Zone(parentRegion, suggestedLocation, Dimensions, width, length);
            UnlockableAreas.Add(zone);
        }

        /// <summary>
        ///     Returns a region if the point is in an unlockable area.
        /// </summary>
        /// <param name="objectLocation">Location of the object.</param>
        /// <returns>New unlocked area or Region.None if not found.</returns>
        public Regions InUnlockedArea(IPoint2D objectLocation)
        {
            var unlocked = Regions.None;

            foreach (var zone in UnlockableAreas)
                // If we are in that area, add it to the unlocked zones.
            {
                if (zone.InArea(objectLocation))
                    unlocked |= zone.ParentRegion; // Combining because it could be an overlapped area.
            }

            return unlocked;
        }

        public override Point2D StartingLocation()
        {
            return new Point2D(MaxX / 2, MaxY / 2);
        }

        #region Getters / Setters

        public Dimension2D Dimensions { get; }

        public int MaxX => Dimensions.Width - 1;
        public int MaxY => Dimensions.Length - 1;

        #endregion
    }
}