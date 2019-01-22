using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map.Regions
{
    public class Graveyard : Spawnable
    {
        public Graveyard()
            : base(RegionType.OpenWorld | RegionType.PvP, Shared.Regions.Graveyard, "Full of bones and bruises.", 45,
                60)
        {
            _NPCs = Spawnables.Graveyard;

            SpawnerAdd(22, 30, 21, 15);
        }
    }
}