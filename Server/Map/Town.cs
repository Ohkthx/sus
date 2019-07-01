using SUS.Shared;

namespace SUS.Server.Map
{
    public abstract class Town : Region
    {
        #region Constructors

        protected Town(RegionTypes types, Regions id)
            : base(types, id)
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