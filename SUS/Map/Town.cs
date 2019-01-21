using SUS.Shared;

namespace SUS.Map
{
    public abstract class Town : Node
    {
        #region Constructors

        protected Town(RegionType type, Regions region, string desc)
            : base(type, region, desc)
        {
            isSpawnable = false;
        }

        #endregion

        public override Point2D StartingLocation()
        {
            return new Point2D(-1, -1, true);
        }
    }
}