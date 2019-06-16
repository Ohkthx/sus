using System;

namespace SUS.Server
{
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string message) : base(message)
        {
        }
    }

    public class MobileNotFoundException : Exception
    {
        public MobileNotFoundException(string message) : base(message)
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
}