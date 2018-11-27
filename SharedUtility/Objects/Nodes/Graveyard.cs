using System;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Objects.Nodes
{
    [Serializable]
    public class Graveyard : Spawnable
    {
        public Graveyard() : base(LocationTypes.OpenWorld | LocationTypes.PvP, Locations.Graveyard, "Full of bones and bruises.", 45, 60)
        {
            NPCs = Spawnables.Graveyard;

            SpawnerAdd(22, 30, 21, 15);
        }
    }
}
