using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map.Zones
{
    public class Despise : Spawnable
    {
        public Despise()
            : base(RegionTypes.Dungeon | RegionTypes.PvP, Regions.Despise, 50, 50)
        {
            Description = "The stench of other humanoids lurk about!";

            AddSpawner(0, MaxY, 10, 5, SpawnTypes.Ettin);
            AddSpawner(MaxX / 2, MaxY / 2, 15, 10, SpawnTypes.Lizardman);

            AddConnection(Regions.Wilderness);
        }
    }
}