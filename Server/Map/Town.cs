using SUS.Shared;

namespace SUS.Server.Map
{
    public abstract class Town : Node
    {
        #region Constructors

        protected Town(RegionType type, Shared.Regions region, string desc)
            : base(type, region, desc)
        {
            IsSpawnable = false;
        }

        #endregion

        public override Point2D StartingLocation()
        {
            return new Point2D(-1, -1, true);
        }
    }
}