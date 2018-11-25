using System;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Objects.Nodes
{
    [Serializable]
    public class Sewers : Spawnable
    {
        public Sewers() : base(Types.Dungeon | Types.PvP, Locations.Sewers, "EW! Sticky!")
        {
            MaxSpawns = 6;
            NPCs = Spawnables.Skeleton | Spawnables.Zombie | Spawnables.Ghoul;
        }
    }
}
