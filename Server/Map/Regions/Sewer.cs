using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map.Regions
{
    public class Sewers : Spawnable
    {
        public Sewers()
            : base(RegionType.Dungeon | RegionType.PvP, Shared.Regions.Sewers, "EW! Sticky!", 30, 35)
        {
            _NPCs = Spawnables.Skeleton | Spawnables.Zombie | Spawnables.Ghoul;

            SpawnerAdd(15, 17, 15, 15);
        }
    }
}