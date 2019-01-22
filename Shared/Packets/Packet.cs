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

        SocketKill
    }

    [Serializable]
    public abstract class Packet : IPacket
    {
        private ulong m_PlayerId; // Author / Owner of the packet.
        private PacketTypes m_Type; // Type of the packet.

        // Creates an instance of a Request based on supplied Type and Object.
        protected Packet(PacketTypes type, ulong playerId)
        {
            Type = type;
            PlayerId = playerId;
        }

        // Converts the object into a byte array to be passed over the network.
        public byte[] ToByte()
        {
            return Network.Serialize(this);
        }

        #region Getters / Setters

        public PacketTypes Type
        {
            get => m_Type;
            private set
            {
                if (value != Type) m_Type = value;
            }
        }

        public ulong PlayerId
        {
            get => m_PlayerId;
            private set
            {
                if (value != PlayerId) m_PlayerId = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class OkPacket : Packet
    {
        public OkPacket() : base(PacketTypes.Ok, 0)
        {
        }
    }

    [Serializable]
    public class ErrorPacket : Packet
    {
        private string m_Error = string.Empty;

        public ErrorPacket(string message) : base(PacketTypes.Error, 0)
        {
            Message = message;
        }

        #region Getters / Setters

        public string Message
        {
            get => m_Error;
            private set
            {
                if (string.IsNullOrEmpty(value)) return;

                if (Message == string.Empty)
                    m_Error = value;
                else if (Message != value) m_Error = value;
            }
        }

        #endregion
    }
}