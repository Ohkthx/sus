﻿using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map.Zones
{
    public class Graveyard : Spawnable
    {
        public Graveyard()
            : base(RegionTypes.OpenWorld | RegionTypes.PvP, Regions.Graveyard, 45, 60)
        {
            Description = "Full of bones and bruises.";

            AddSpawner(22, 30, 21, 15, SpawnTypes.Zombie | SpawnTypes.Ghoul | SpawnTypes.Skeleton);
            AddSpawner(MaxX / 2, MaxY / 2, MaxX / 2, 5, SpawnTypes.Wraith);

            AddConnection(Regions.Britain | Regions.Wilderness);
        }
    }
}