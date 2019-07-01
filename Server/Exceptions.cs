using System;

namespace SUS.Server
{
    public class UnknownItemException : Exception
    {
        public UnknownItemException(string message) : base(message)
        {
        }
    }

    public class UnknownMobileException : Exception
    {
        public UnknownMobileException(string message) : base(message)
        {
        }
    }

    public class NotEnoughGoldException : Exception
    {
        public NotEnoughGoldException()
        {
        }

        private NotEnoughGoldException(string message) : base(message)
        {
        }
    }

    public class UnknownRegionException : Exception
    {
        public UnknownRegionException(int mobileId, string message)
            : base(message)
        {
            MobileId = mobileId;
        }

        public int MobileId { get; }
    }

    public class InvalidFactoryException : Exception
    {
        public InvalidFactoryException(string message) : base($"[Factory] {message}")
        {
        }
    }
}