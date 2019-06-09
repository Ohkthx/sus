using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SUS.Server.Objects.Mobiles;
using SUS.Server.Objects.Mobiles.Spawns;
using SUS.Shared;

namespace SUS.Server.Objects
{
    [Flags]
    public enum Spawnables
    {
        None = 0x00000000,

        Skeleton = 0x00000001,
        Zombie = 0x00000002,
        Ghoul = 0x00000004,
        Wraith = 0x00000008,

        Unused1 = 0x00000010,

        Orc = 0x00000020,
        Cyclops = 0x00000040,
        Titan = 0x00000080,

        Lizardman = 0x00000100,
        Ettin = 0x00000200,
        Unused4 = 0x00000400,
        Unused5 = 0x00000800,
        Unused6 = 0x00001000,
        Unused7 = 0x00002000,
        Unused8 = 0x00004000,
        Unused9 = 0x00008000,
        Unused10 = 0x00010000,
        Unused11 = 0x00020000,
        Unused12 = 0x00040000,
        Unused13 = 0x00080000,
        Unused14 = 0x00100000,
        Unused15 = 0x00200000,
        Unused16 = 0x00400000,
        Unused17 = 0x00800000,
        Unused18 = 0x01000000,
        Unused19 = 0x02000000,
        Unused20 = 0x04000000,
        Unused21 = 0x08000000,
        Unused22 = 0x10000000,
        Unused23 = 0x20000000,
        Unused24 = 0x40000000,

        Graveyard = Skeleton | Zombie | Ghoul | Wraith
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
        private Spawnables _spawns; // Types of spawns that are acceptable.

        #region Constructors

        public Spawner(Regions loc, Spawnables spawns, int baseX, int baseY, int range, int limit, int mapX, int mapY)
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

        #region Getters / Setters

        public Guid ID
        {
            get
            {
                if (_id == Guid.Empty) _id = Guid.NewGuid();

                return _id;
            }
        }

        public Point2D HomeLocation
        {
            get => _location;
            private set
            {
                if (value != HomeLocation) _location = value;
            }
        }

        private Regions Region { get; }

        private Spawnables Spawns
        {
            get => _spawns;
            set
            {
                if (value != Spawns) _spawns = value;
            }
        }

        private HashSet<Mobile> Spawned => _spawned ?? (_spawned = new HashSet<Mobile>());

        public int HomeRange { get; }

        #endregion

        #region Spawning

        private void Spawn(object source, ElapsedEventArgs e)
        {
            // Clean the spawner.
            Spawned.RemoveWhere(x => x.IsDeleted);

            if (Spawned.Count >= _limit) return;

            var spawns = Utility.EnumToIEnumerable<Spawnables>(_spawns, true);
            if (spawns == null || !spawns.Any())
                return;

            var amount = Utility.RandomMinMax(0, 2);
            for (var i = 0; i < amount; i++)
            {
                var pos = Utility.RandomMinMax(0, spawns.Count() - 1);
                var mob = spawnOffType(spawns.ElementAt(pos));
                if (mob == null) continue;

                mob.Spawned(this, Region, ValidCoordinate(HomeLocation.X, HomeLocation.Y, HomeRange));
                Spawned.Add(mob); // Add it to the tracked spawned.
            }
        }

        private BaseCreature spawnOffType(Spawnables spawn)
        {
            switch (spawn)
            {
                case Spawnables.Skeleton:
                    return new Skeleton();
                case Spawnables.Zombie:
                    return new Zombie();
                case Spawnables.Ghoul:
                    return new Ghoul();
                case Spawnables.Wraith:
                    return new Wraith();
                case Spawnables.Lizardman:
                    return new Lizardman();
                case Spawnables.Ettin:
                    return new Ettin();
                case Spawnables.Orc:
                    return new Orc();
                case Spawnables.Cyclops:
                    return new Cyclops();
                case Spawnables.Titan:
                    return new Titan();
                default:
                    return null;
            }
        }

        #endregion

        #region Coordinates

        public Point2D ValidCoordinate(int baseX, int baseY, int range)
        {
            var newX = validAxis(baseX, _canvasMaxX, range);
            var newY = validAxis(baseY, _canvasMaxY, range);

            return new Point2D(newX, newY);
        }

        private int validAxis(int baseN, int maxN, int range)
        {
            if (baseN < 0) baseN = 0;

            if (baseN > maxN) baseN = maxN;

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