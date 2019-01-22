using SUS.Shared;

namespace SUS.Server.Map.Regions
{
    public class Britain : Town
    {
        public Britain()
            : base(RegionType.Town, Shared.Regions.Britain, "The greatest city of Britainia.")
        {
        }
    }
}