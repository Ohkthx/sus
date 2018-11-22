using System;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Objects.Nodes
{
    [Serializable]
    public class Wilderness : Spawnable
    {
        public Wilderness() : base(Types.OpenWorld | Types.PvP, Locations.Wilderness, "A vast open world.")
        {
            MaxSpawns = 6;
            NPCs = Spawnables.Skeleton | Spawnables.Zombie | Spawnables.Ghoul | Spawnables.Wraith;
        }
    }
}
