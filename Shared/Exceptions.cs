using System;

namespace SUS.Shared
{
    public class InvalidPacketException : Exception
    {
        public InvalidPacketException(string message) : base(message)
        {
        }
    }

    public class InvalidSocketHandlerException : Exception
    {
        public InvalidSocketHandlerException(string message) : base(message)
        {
        }
    }
}