using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map.Regions
{
    public class Graveyard : Spawnable
    {
        public Graveyard()
            : base(RegionTypes.OpenWorld | RegionTypes.PvP, Shared.Regions.Graveyard, 45, 60)
        {
            Description = "Full of bones and bruises.";

            NPCs = Spawnables.Graveyard;
            AddSpawner(22, 30, 21, 15);

            AddConnection(Shared.Regions.Britain | Shared.Regions.Wilderness);
        }
    }
}