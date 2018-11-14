using System;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Objects
{
    public class Sewer : Spawnable
    {
        public Sewer(Types type, Locations loc, string str) : base(type, loc, str)
        {
            MaxSpawns = 15;
            NPCs = Spawnables.Skeleton | Spawnables.Zombie | Spawnables.Ghoul | Spawnables.Wraith;
        }
    }
}
