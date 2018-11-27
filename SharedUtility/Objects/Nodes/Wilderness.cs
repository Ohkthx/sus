using System;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Objects.Nodes
{
    [Serializable]
    public class Wilderness : Spawnable
    {
        public Wilderness() : base(LocationTypes.OpenWorld | LocationTypes.PvP, Locations.Wilderness, "A vast open world.", 150, 150)
        {
            NPCs = Spawnables.Graveyard | Spawnables.Orc | Spawnables.Titan | Spawnables.Cyclops;

            SpawnerAdd(20, 20, 16, 5);
            SpawnerAdd(50, 125, 60, 10);
            SpawnerAdd(120, 60, 60, 10);
        }
    }
}
