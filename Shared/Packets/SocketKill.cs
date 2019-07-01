using System;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class SocketKillPacket : Packet
    {
        private bool _kill;
        private string _message;

        public SocketKillPacket(string message = "", bool kill = true)
        {
            Message = message;
            Kill = kill;
        }

        #region Getters / Setters

        public bool Kill
        {
            get => _kill;
            private set
            {
                if (value != Kill)
                    _kill = value;
            }
        }

        public string Message
        {
            get => string.IsNullOrWhiteSpace(_message) ? string.Empty : _message;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = string.Empty;

                _message = value;
            }
        }

        #endregion
    }
}