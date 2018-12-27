using System;
using System.Collections.Concurrent;
using SUS.Shared;
using SUS.Objects;

namespace SUS
{
    public abstract class Town : Node
    {
        #region Constructors
        public Town(RegionType type, Regions region, string desc) 
            : base(type, region, desc)
        {
            isSpawnable = false;
        }
        #endregion

        public override Point2D StartingLocation()
        {
            return new Point2D(-1, -1, invalid: true);
        }
    }
}
