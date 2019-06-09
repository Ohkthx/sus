namespace SUS.Server
{
    public struct Dimension2D : IDimension2D
    {
        private int _width;
        private int _length;

        #region Constructors

        public Dimension2D(int width, int length)
        {
            _length = _width = 1;

            Length = length;
            Width = width;
        }

        #endregion

        /// <summary>
        ///     Override the width of the dimension.
        /// </summary>
        /// <param name="width">New width to set.</param>
        /// <returns>True if the new matched the original.</returns>
        public bool SetWidth(int width)
        {
            Width = width;
            return Width == width;
        }

        /// <summary>
        ///     Override the length of the dimension.
        /// </summary>
        /// <param name="length">New length to set.</param>
        /// <returns>True if the new matched the original.</returns>
        public bool SetLength(int length)
        {
            Length = length;
            return Length == length;
        }

        #region Getters / Setters

        public int Length
        {
            get => _length;
            private set
            {
                if (value < 1)
                    value = 1;

                _length = value;
            }
        }

        public int Width
        {
            get => _width;
            private set
            {
                if (value < 1)
                    value = 1;

                _width = value;
            }
        }

        #endregion
    }
}