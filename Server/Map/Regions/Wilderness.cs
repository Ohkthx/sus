using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map.Regions
{
    public class Wilderness : Spawnable
    {
        public Wilderness()
            : base(RegionType.OpenWorld | RegionType.PvP, Shared.Regions.Wilderness, "A vast open world.", 150, 150)
        {
            _NPCs = Spawnables.Graveyard | Spawnables.Orc | Spawnables.Titan | Spawnables.Cyclops;

            SpawnerAdd(20, 20, 16, 5);
            SpawnerAdd(50, 125, 60, 10);
            SpawnerAdd(120, 60, 60, 10);
        }
    }
}