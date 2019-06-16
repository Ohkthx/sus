using SUS.Server.Objects.Mobiles.NPCs;
using SUS.Shared;

namespace SUS.Server.Map.Regions
{
    public class Britain : Town
    {
        public Britain()
            : base(RegionTypes.Town, Shared.Regions.Britain)
        {
            Description = "The greatest city of Britania.";

            AddConnection(Shared.Regions.Sewers | Shared.Regions.Graveyard | Shared.Regions.Wilderness);

            AddNPC(new Repairer());
        }
    }
}