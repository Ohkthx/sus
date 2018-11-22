using System;
using System.Collections.Generic;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.Objects.Mobiles.Spawns;

namespace SUS.Shared.Objects
{
    [Flags, Serializable]
    public enum Spawnables
    {
        None        = 0x00000000,

        Skeleton    = 0x00000001,
        Zombie      = 0x00000002,
        Ghoul       = 0x00000004,
        Wraith      = 0x00000008,

        Unused1     = 0x00000010,

        Orc         = 0x00000020,
        Cyclops     = 0x00000040,
        Titan       = 0x00000080,

        Unused2     = 0x00000100,
        Unused3     = 0x00000200,
        Unused4     = 0x00000400,
        Unused5     = 0x00000800,
        Unused6     = 0x00001000,
        Unused7     = 0x00002000,
        Unused8     = 0x00004000,
        Unused9     = 0x00008000,
        Unused10    = 0x00010000,
        Unused11    = 0x00020000,
        Unused12    = 0x00040000,
        Unused13    = 0x00080000,
        Unused14    = 0x00100000,
        Unused15    = 0x00200000,
        Unused16    = 0x00400000,
        Unused17    = 0x00800000,
        Unused18    = 0x01000000,
        Unused19    = 0x02000000,
        Unused20    = 0x04000000,
        Unused21    = 0x08000000,
        Unused22    = 0x10000000,
        Unused23    = 0x20000000,
        Unused24    = 0x40000000,
    };

    [Serializable]
    public abstract class Spawnable : Node
    {
        public int MaxSpawns = 0;
        public Spawnables NPCs = Spawnables.None;

        public Spawnable(Types type, Locations loc, string desc) : base(type, loc, desc) { isSpawnable = true; }
    }
}
