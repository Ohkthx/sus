using System;

namespace SUS.Shared.Packets
{
    public enum PacketTypes
    {
        Ok,
        Error,

        Authenticate,
        ClientState,

        GetLocalMobiles,
        GetMobile,
        GetNode,

        MobileCombat,
        MobileMove,
        MobileResurrect,
        UseItem,
        UseVendor,

        SocketKill
    }

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

        private ulong _playerId; // Author / Owner of the packet.
        private Stages _stage;
        private PacketTypes _type; // Type of the packet.

        // Creates an instance of a Request based on supplied Type and Object.
        protected Packet(PacketTypes type, ulong playerId)
        {
            Type = type;
            PlayerId = playerId;
            Stage = Stages.One;
        }

        // Converts the object into a byte array to be passed over the network.
        public byte[] ToByte()
        {
            return Network.Serialize(this);
        }

        #region Getters / Setters

        public PacketTypes Type
        {
            get => _type;
            private set
            {
                if (value != Type) _type = value;
            }
        }

        public ulong PlayerId
        {
            get => _playerId;
            private set
            {
                if (value != PlayerId) _playerId = value;
            }
        }

        public Stages Stage
        {
            get => _stage;
            set
            {
                if (!Enum.IsDefined(typeof(Stages), value)) return;
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

        public OkPacket(string message) : base(PacketTypes.Ok, 0)
        {
            Message = message;
        }

        public string Message
        {
            get => _message;
            private set
            {
                if (string.IsNullOrEmpty(value)) return;

                if (Message == string.Empty)
                    _message = value;
                else if (Message != value) _message = value;
            }
        }
    }

    [Serializable]
    public class ErrorPacket : Packet
    {
        private string _error = string.Empty;

        public ErrorPacket(string message) : base(PacketTypes.Error, 0)
        {
            Message = message;
        }

        #region Getters / Setters

        public string Message
        {
            get => _error;
            private set
            {
                if (string.IsNullOrEmpty(value)) return;

                if (Message == string.Empty)
                    _error = value;
                else if (Message != value) _error = value;
            }
        }

        #endregion
    }
}