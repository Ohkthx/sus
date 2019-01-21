using SUS.Objects;
using SUS.Shared;

namespace SUS.Map
{
    public class Sewers : Spawnable
    {
        public Sewers()
            : base(RegionType.Dungeon | RegionType.PvP, Regions.Sewers, "EW! Sticky!", 30, 35)
        {
            NPCs = Spawnables.Skeleton | Spawnables.Zombie | Spawnables.Ghoul;

            SpawnerAdd(15, 17, 15, 15);
        }
    }
}