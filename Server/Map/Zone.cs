using SUS.Shared;

namespace SUS.Server.Map
{
    public class Zone
    {
        private Regions _parentRegion;

        /// <summary>
        ///     Checks if the current location is located inside the zone.
        /// </summary>
        /// <param name="location">Coordinates of the object.</param>
        /// <returns>True / False if it is located within.</returns>
        public bool InArea(IPoint2D location)
        {
            if (location.X < Location.X || location.X > Location.X + (Width - 1))
                return false;

            return location.Y >= Location.Y && location.Y <= Location.Y + (Width - 1);
        }

        private Point2D GetValidArea(IPoint2D suggestedPoint2D, IDimension2D mapSize)
        {
            var entranceLocation = new Point2D();

            // Override the Width if it is greater than the maps width.
            if (Width > mapSize.Width)
                Dimensions.SetWidth(mapSize.Width);

            // Override the Length if it is greater than the maps length.
            if (Length > mapSize.Length)
                Dimensions.SetLength(mapSize.Length);

            // If the suggested X is greater than the map width, set the initial point in
            //  relation to the maps maximum width and entrance width.
            if (suggestedPoint2D.X + (Width - 1) > mapSize.Width - 1)
                entranceLocation.X = mapSize.Width - Width;
            else
                entranceLocation.X = suggestedPoint2D.X;

            // If the suggested Y is greater than the map length, set the initial point in
            //  relation to the maps maximum length and entrance length.
            if (suggestedPoint2D.Y + (Length - 1) > mapSize.Length - 1)
                entranceLocation.Y = mapSize.Length - Length;
            else
                entranceLocation.Y = suggestedPoint2D.Y;

            return entranceLocation;
        }

        #region Constructors

        public Zone(Regions parentRegion, IPoint2D suggestedLocation, IDimension2D mapSize) : this(parentRegion,
            suggestedLocation, mapSize, 2, 2)
        {
        }

        public Zone(Regions parentRegion, IPoint2D suggestedLocation, IDimension2D mapSize, int width,
            int length)
        {
            ParentRegion = parentRegion;
            Dimensions = new Dimension2D(width, length);
            Location = GetValidArea(suggestedLocation, mapSize);
        }

        #endregion

        #region Getters / Setters

        public Point2D Location { get; protected set; }

        private Dimension2D Dimensions { get; }

        public int Length => Dimensions.Length;

        public int Width => Dimensions.Width;

        public Regions ParentRegion
        {
            get => _parentRegion;
            protected set
            {
                // Debug: Throw error due to being potentially invalid.
                if (value == Regions.None || !Region.IsValidRegion(value))
                    return;

                _parentRegion = value;
            }
        }

        #endregion
    }
}