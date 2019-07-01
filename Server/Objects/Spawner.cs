using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SUS.Server.Objects.Mobiles;
using SUS.Shared;

namespace SUS.Server.Objects
{
    [Flags]
    public enum SpawnTypes
    {
        None = 0,

        Skeleton = 1 << 0,
        Zombie = 1 << 1,
        Ghoul = 1 << 2,
        Wraith = 1 << 3,

        Orc = 1 << 4,
        Cyclops = 1 << 5,
        Titan = 1 << 6,

        Lizardman = 1 << 7,
        Ettin = 1 << 8
    }

    public class Spawner : ISpawner
    {
        private const int SpawnTimer = 15000;
        private readonly int _canvasMaxX;
        private readonly int _canvasMaxY;

        private readonly int _limit;
        private Guid _id;
        private Point2D _location; // Location of the Spawner on the map.
        private HashSet<Mobile> _spawned;
        private SpawnTypes _spawns; // Types of spawns that are acceptable.

        #region Constructors

        public Spawner(Regions loc, SpawnTypes spawns, int baseX, int baseY, int range, int limit, int mapX, int mapY)
        {
            Region = loc;
            Spawns = spawns;

            HomeRange = range;
            _limit = limit;
            _canvasMaxX = mapX;
            _canvasMaxY = mapY;

            HomeLocation = ValidCoordinate(baseX, baseY, 0);


            // Start the timer.
            var timer = new System.Timers.Timer(SpawnTimer);
            timer.Elapsed += Spawn; // Calls "CheckSpawns" when it hits the interval.
            timer.AutoReset = true; // Timer to reset or not once it hits it's limit.
            timer.Enabled = true; // Enable it.
        }

        #endregion

        #region Spawning

        private void Spawn(object source, ElapsedEventArgs e)
        {
            // Clean the spawner.
            Spawned.RemoveWhere(x => x.IsDeleted);

            if (Spawned.Count >= _limit)
                return;

            var spawns = Utility.EnumToIEnumerable<SpawnTypes>(_spawns, true);
            if (spawns == null || !spawns.Any())
                return;

            var amount = Utility.RandomMinMax(0, 2);
            while (--amount > 0)
            {
                BaseCreature mob;
                try
                {
                    var pos = Utility.RandomMinMax(0, spawns.Count() - 1);
                    mob = Factory.GetSpawn(spawns.ElementAt(pos));
                    if (mob == null)
                        throw new InvalidFactoryException(
                            $"Region: {Enum.GetName(typeof(Regions), Region)} {HomeLocation}, Spawn: {Enum.GetName(typeof(SpawnTypes), spawns.ElementAt(pos))}");
                }
                catch (InvalidFactoryException ise)
                {
                    Utility.ConsoleNotify(ise.Message);
                    ++amount;
                    continue;
                }

                mob.Spawned(this, Region, ValidCoordinate(HomeLocation.X, HomeLocation.Y, HomeRange));
                Spawned.Add(mob); // Add it to the tracked spawned.
            }
        }

        #endregion

        #region Getters / Setters

        public Guid ID
        {
            get
            {
                if (_id == Guid.Empty)
                    _id = Guid.NewGuid();

                return _id;
            }
        }

        public Point2D HomeLocation
        {
            get => _location;
            private set
            {
                if (value != HomeLocation)
                    _location = value;
            }
        }

        private Regions Region { get; }

        private SpawnTypes Spawns
        {
            get => _spawns;
            set
            {
                if (value != Spawns)
                    _spawns = value;
            }
        }

        private HashSet<Mobile> Spawned => _spawned ?? (_spawned = new HashSet<Mobile>());

        public int HomeRange { get; }

        #endregion

        #region Coordinates

        public Point2D ValidCoordinate(int baseX, int baseY, int range)
        {
            var newX = ValidAxis(baseX, _canvasMaxX, range);
            var newY = ValidAxis(baseY, _canvasMaxY, range);

            return new Point2D(newX, newY);
        }

        private static int ValidAxis(int baseN, int maxN, int range)
        {
            if (baseN < 0)
                baseN = 0;

            if (baseN > maxN)
                baseN = maxN;

            int baseModified;
            do
            {
                baseModified = baseN + Utility.RandomMinMax(-range, range);
            } while (baseModified < 0 || baseModified > maxN);

            return baseModified;
        }

        #endregion
    }
}