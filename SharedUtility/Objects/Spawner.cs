using System;
using System.Timers;
using System.Collections.Generic;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.Utilities;
using SUS.Shared.Objects.Mobiles.Spawns;

namespace SUS.Shared.Objects
{
    [Flags, Serializable]
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

    public sealed class Spawner
    {
        private Guid m_Guid;
        private Coordinate m_Coord;     // Location of the Spawner on the map.
        private System.Timers.Timer m_Timer;          // Responsible for Keeping track.
        private Locations m_Location;   // Location the Spawner exists.
        private Spawnables m_Spawns;    // Types of spawns that are acceptable.

        private int m_Range;            // Range the Spawner can spawn in.
        private int m_Limit;
        private int m_CanvasMaxX;
        private int m_CanvasMaxY;

        private const int SPAWNTIME = 15000;

        #region Constructors
        public Spawner(Locations loc, Spawnables spawns, int baseX, int baseY, int range, int limit, int mapX, int mapY)
        {
            m_Location = loc;
            Spawns = spawns;

            m_Range = range;
            m_Limit = limit;
            m_CanvasMaxX = mapX;
            m_CanvasMaxY = mapY;

            Coordinate = ValidCoordinate(baseX, baseY, 0);


            // Start the timer.
            m_Timer = new System.Timers.Timer(SPAWNTIME);     // Create the timer with a 15sec counter.
            m_Timer.Elapsed += Spawn;           // Calls "CheckSpawns" when it hits the interval.
            m_Timer.AutoReset = true;           // Timer to reset or not once it hits it's limit.
            m_Timer.Enabled = true;             // Enable it.
        }
        #endregion

        #region Getters / Setters
        public Guid Guid
        {
            get
            {
                if (m_Guid == null || m_Guid == Guid.Empty)
                    m_Guid = Guid.NewGuid();

                return m_Guid;
            }
        }

        public Coordinate Coordinate
        {
            get { return m_Coord; }
            private set
            {
                if (value == null)
                    return;
                else if (Coordinate == null)
                    m_Coord = value;

                if (value != Coordinate)
                    m_Coord = value;
            }
        }

        public Locations Location
        {
            get { return m_Location; }
            private set
            {
                if (value != m_Location)
                    m_Location = value;
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
        #endregion

        #region Spawning
        public void Spawn(Object source, ElapsedEventArgs e)
        {
            if (World.SpawnersCount(Location, Guid) >= m_Limit)
                return;

            List<Spawnables> spawns = spawnablesToList(m_Spawns);
            if (spawns == null)
                return;

            int amount = Utility.RandomMinMax(0, 2);
            for (int i = 0; i < amount; i++)
            {
                int pos = Utility.RandomMinMax(0, spawns.Count - 1);
                BaseCreature mob = spawnOffType(spawns[pos]);
                mob.Coordinate = ValidCoordinate(Coordinate.X, Coordinate.Y, m_Range);

                World.Spawn(mob as Mobile, Location, Guid);
            }
        }

        private List<Spawnables> spawnablesToList(Spawnables spawns)
        {
            List<Spawnables> creatures = new List<Spawnables>();

            // While our spawnables passed are not "None", continue to try and build a list of potential creatures.
            while (spawns != Spawnables.None)
            {
                foreach (Spawnables s in Enum.GetValues(typeof(Spawnables)))
                {
                    if ((spawns & s) == s && s != Spawnables.None)
                    {   // Found a match.
                        creatures.Add(s);    // Spawn based on its type.
                        spawns &= ~s;                   // Remove our value from spawns.
                    }
                }
            }

            if (creatures.Count == 0)
                return null;

            return creatures;
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
        public Coordinate ValidCoordinate(int baseX, int baseY, int range)
        {
            int newX = validAxis(baseX, m_CanvasMaxX, range);
            int newY = validAxis(baseY, m_CanvasMaxY, range);

            return new Coordinate(newX, newY);
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
