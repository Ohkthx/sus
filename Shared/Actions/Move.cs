using System;

namespace SUS.Shared.Actions
{
    [Serializable]
    public struct Move : IAction
    {
        public int MobileId { get; }
        public Regions Destination { get; }
        public Directions Direction { get; }

        public Move(int mobileId, Regions region, Directions direction = Directions.None)
        {
            MobileId = mobileId;
            Destination = region;
            Direction = direction;
        }
    }
}