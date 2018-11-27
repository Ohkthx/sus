using System;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Objects.Nodes
{
    [Serializable]
    public class Sewers : Spawnable
    {
        public Sewers() : base(LocationTypes.Dungeon | LocationTypes.PvP, Locations.Sewers, "EW! Sticky!", 30, 35)
        {
            NPCs = Spawnables.Skeleton | Spawnables.Zombie | Spawnables.Ghoul;

            SpawnerAdd(15, 17, 15, 15);
        }
    }
}
