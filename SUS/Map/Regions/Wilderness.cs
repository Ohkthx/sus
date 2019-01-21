using SUS.Objects;
using SUS.Shared;

namespace SUS.Map
{
    public class Wilderness : Spawnable
    {
        public Wilderness()
            : base(RegionType.OpenWorld | RegionType.PvP, Regions.Wilderness, "A vast open world.", 150, 150)
        {
            NPCs = Spawnables.Graveyard | Spawnables.Orc | Spawnables.Titan | Spawnables.Cyclops;

            SpawnerAdd(20, 20, 16, 5);
            SpawnerAdd(50, 125, 60, 10);
            SpawnerAdd(120, 60, 60, 10);
        }
    }
}