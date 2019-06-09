using SUS.Shared;

namespace SUS.Server.Map
{
    public abstract class Town : Node
    {
        #region Constructors

        protected Town(RegionTypes types, Shared.Regions region)
            : base(types, region)
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