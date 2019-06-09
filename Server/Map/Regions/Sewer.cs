using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map.Regions
{
    public class Sewers : Spawnable
    {
        public Sewers()
            : base(RegionTypes.Dungeon | RegionTypes.PvP, Shared.Regions.Sewers, 30, 35)
        {
            Description = "Ew! Sticky!";

            NPCs = Spawnables.Skeleton | Spawnables.Zombie | Spawnables.Ghoul;
            AddSpawner(15, 17, 15, 15);

            AddConnection(Shared.Regions.Britain);
        }
    }
}