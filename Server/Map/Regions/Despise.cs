using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map.Regions
{
    public class Despise : Spawnable
    {
        public Despise()
            : base(RegionTypes.Dungeon | RegionTypes.PvP, Shared.Regions.Despise, 50, 50)
        {
            Description = "The stench of other humanoids lurk about!";

            NPCs = SpawnTypes.Lizardman | SpawnTypes.Ettin;
            AddSpawner(MaxX / 2, MaxY / 2, 20, 10);

            AddConnection(Shared.Regions.Wilderness);
        }
    }
}