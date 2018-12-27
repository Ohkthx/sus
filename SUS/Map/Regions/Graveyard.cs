using SUS.Shared;
using SUS.Objects;

namespace SUS.Map
{
    public class Graveyard : Spawnable
    {
        public Graveyard() 
            : base(RegionType.OpenWorld | RegionType.PvP, Regions.Graveyard, "Full of bones and bruises.", 45, 60)
        {
            NPCs = Spawnables.Graveyard;

            SpawnerAdd(22, 30, 21, 15);
        }
    }
}
