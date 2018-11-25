using System;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Objects.Nodes
{
    [Serializable]
    public class Graveyard : Spawnable
    {
        public Graveyard() : base(Types.OpenWorld | Types.PvP, Locations.Graveyard, "Full of bones and bruises.")
        {
            MaxSpawns = 6;
            NPCs = Spawnables.Graveyard;
        }
    }
}
