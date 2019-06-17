﻿using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Map.Regions
{
    public class Wilderness : Spawnable
    {
        public Wilderness()
            : base(RegionTypes.OpenWorld | RegionTypes.PvP, Shared.Regions.Wilderness, 150, 150)
        {
            Description = "A vast and open world!";

            AddSpawner(20, 20, 16, 5, SpawnTypes.Ettin | SpawnTypes.Orc);
            AddSpawner(50, 125, 60, 10, SpawnTypes.Lizardman);
            AddSpawner(120, 60, 60, 10, SpawnTypes.Ettin | SpawnTypes.Orc);
            AddSpawner(MaxX/2, MaxY/2, MaxX/2, 5, SpawnTypes.Cyclops | SpawnTypes.Titan);

            AddConnection(Shared.Regions.Britain | Shared.Regions.Graveyard | Shared.Regions.Despise);

            // Despise Entrance
            AddZone(Shared.Regions.Despise, new Point2D(MaxX / 2, MaxY), 3, 3);
        }
    }
}