using SUS.Server.Objects.Mobiles.NPCs;
using SUS.Shared;

namespace SUS.Server.Map.Zones
{
    public class Britain : Town
    {
        public Britain()
            : base(RegionTypes.Town, Regions.Britain)
        {
            Description = "The greatest city of Britania.";

            AddConnection(Regions.Sewers | Regions.Graveyard | Regions.Wilderness);

            AddNpc(new Repairer());
            AddNpc(new Armorsmith());
            AddNpc(new Weaponsmith());
        }
    }
}