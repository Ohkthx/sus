using System;

namespace SUS.Shared.Packets
{
    [Serializable]
    public abstract class Packet : IPacket
    {
        public enum Stages
        {
            One,
            Two,
            Three,
            Four,
            Five
        }

        private Stages _stage;

        // Creates an instance of a Request based on supplied Type and Object.
        protected Packet()
        {
            Stage = Stages.One;
        }

        // Converts the object into a byte array to be passed over the network.
        public byte[] ToByte()
        {
            Clean();
            return Network.Serialize(this);
        }

        /// <summary>
        ///     By default, nothing needs to be cleaned.
        /// </summary>
        protected virtual void Clean()
        {
        }

        #region Getters / Setters

        public ulong PlayerId { get; set; }

        public Stages Stage
        {
            get => _stage;
            set
            {
                if (!Enum.IsDefined(typeof(Stages), value))
                    throw new ArgumentOutOfRangeException(nameof(value), "Attempted to adjust packet stage.");

                _stage = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class OkPacket : Packet
    {
        private string _message = string.Empty;

        public OkPacket() : this(string.Empty)
        {
        }

        public OkPacket(string message)
        {
            Message = message;
        }

        public string Message
        {
            get => _message;
            private set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                if (Message == string.Empty)
                    _message = value;
                else if (Message != value)
                    _message = value;
            }
        }
    }

    [Serializable]
    public class ErrorPacket : Packet
    {
        private string _error = string.Empty;

        public ErrorPacket(string message)
        {
            Message = message;
        }

        #region Getters / Setters

        public string Message
        {
            get => _error;
            private set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                if (Message == string.Empty)
                    _error = value;
                else if (Message != value)
                    _error = value;
            }
        }

        #endregion
    }
}