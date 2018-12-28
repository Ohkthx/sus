using System;
using System.Timers;
using System.Collections.Generic;
using SUS.Shared;
using SUS.Objects.Spawns;
using System.Linq;

namespace SUS.Objects
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

        Unused2 = 0x00000100,
        Unused3 = 0x00000200,
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

        Graveyard = Skeleton | Zombie | Ghoul | Wraith,
    };

    public class Spawner : ISpawner
    {
        private Guid m_ID;
        private Point2D m_Location;             // Location of the Spawner on the map.
        private System.Timers.Timer m_Timer;    // Responsible for Keeping track.
        private Regions m_Region;               // Location the Spawner exists.
        private Spawnables m_Spawns;            // Types of spawns that are acceptable.
        private HashSet<Mobile> m_Spawned;

        private int m_Range;                    // Range the Spawner can spawn in.
        private int m_Limit;
        private int m_CanvasMaxX;
        private int m_CanvasMaxY;

        private const int SPAWNTIME = 15000;

        #region Constructors
        public Spawner(Regions loc, Spawnables spawns, int baseX, int baseY, int range, int limit, int mapX, int mapY)
        {
            m_Region = loc;
            Spawns = spawns;

            m_Range = range;
            m_Limit = limit;
            m_CanvasMaxX = mapX;
            m_CanvasMaxY = mapY;

            HomeLocation = ValidCoordinate(baseX, baseY, 0);


            // Start the timer.
            m_Timer = new System.Timers.Timer(SPAWNTIME);     // Create the timer with a 15sec counter.
            m_Timer.Elapsed += Spawn;           // Calls "CheckSpawns" when it hits the interval.
            m_Timer.AutoReset = true;           // Timer to reset or not once it hits it's limit.
            m_Timer.Enabled = true;             // Enable it.
        }
        #endregion

        #region Getters / Setters
        public Guid ID
        {
            get
            {
                if (m_ID == null || m_ID == Guid.Empty)
                    m_ID = Guid.NewGuid();

                return m_ID;
            }
        }

        public Point2D HomeLocation
        {
            get { return m_Location; }
            private set
            {
                if (value == null)
                    return;
                else if (HomeLocation == null)
                    m_Location = value;

                if (value != HomeLocation)
                    m_Location = value;
            }
        }

        public Regions Region
        {
            get { return m_Region; }
            private set
            {
                if (value != m_Region)
                    m_Region = value;
            }
        }

        public Spawnables Spawns
        {
            get { return m_Spawns; }
            private set
            {
                if (value != Spawns)
                    m_Spawns = value;
            }
        }

        public HashSet<Mobile> Spawned
        {
            get
            {
                if (m_Spawned == null)
                    m_Spawned = new HashSet<Mobile>();

                return m_Spawned;
            }
        }

        public int HomeRange { get { return m_Range; } }
        #endregion

        #region Spawning
        public void Spawn(Object source, ElapsedEventArgs e)
        {   // Clean the spawner.
            Spawned.RemoveWhere(x => x.IsDeleted);
            
            if (Spawned.Count >= m_Limit)
                return;

            IEnumerable<Spawnables> spawns = Utility.EnumToIEnumerable<Spawnables>(m_Spawns, PowerOf2: true);
            if (spawns.Count() == 0)
                return;

            int amount = Utility.RandomMinMax(0, 2);
            for (int i = 0; i < amount; i++)
            {
                int pos = Utility.RandomMinMax(0, spawns.Count() - 1);
                BaseCreature mob = spawnOffType(spawns.ElementAt(pos));
                if (mob == null)
                    continue;
                mob.Spawned(this, Region, ValidCoordinate(HomeLocation.X, HomeLocation.Y, HomeRange));
                Spawned.Add(mob);       // Add it to the tracked spawned.
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
            int newX = validAxis(baseX, m_CanvasMaxX, range);
            int newY = validAxis(baseY, m_CanvasMaxY, range);

            return new Point2D(newX, newY);
        }

        private int validAxis(int baseN, int maxN, int range)
        {
            if (baseN < 0)
                baseN = 0;

            if (baseN > maxN)
                baseN = maxN;

            int baseModified = 0;
            do
            {
                baseModified = baseN + Utility.RandomMinMax((-range), range);
            } while (baseModified < 0 || baseModified > maxN);

            return baseModified;
        }
        #endregion

    }
}
