using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map.Zones
{
    public class Sewers : Spawnable
    {
        public Sewers()
            : base(RegionTypes.Dungeon | RegionTypes.PvP, Regions.Sewers, 30, 35)
        {
            Description = "Ew! Sticky!";

            AddSpawner(15, 17, 15, 15, SpawnTypes.Skeleton | SpawnTypes.Ghoul | SpawnTypes.Zombie);

            AddConnection(Regions.Britain);
        }
    }
}