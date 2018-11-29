using System;
using SUS.Shared.Objects;
using SUS.Shared.Utilities;

namespace SUS.Shared.Packets
{
    [Serializable]
    public enum PacketTypes
    {
        OK,
        Error,

        Authenticate,
        GameState,

        GetLocalMobiles,
        GetMobile,
        GetNode,

        MobileCombat,
        MobileMove,
        MobileResurrect,

        SocketKill
    }

    [Serializable]
    public abstract class Packet
    {
        private PacketTypes m_Type; // Type of the packet.
        private BasicMobile m_Author; // Author / Owner of the packet.

        // Creates an instance of a Request based on supplied Type and Object.
        public Packet(PacketTypes type, BasicMobile author)
        {
            Type = type;
            Author = author;
        }

        #region Getters / Setters
        public PacketTypes Type
        {
            get { return m_Type; }
            private set
            {
                if (value != Type)
                    m_Type = value;
            }
        }

        public BasicMobile Author 
        {
            get { return m_Author; }
            private set
            {
                if (value == null)
                    return;
                else if (Author == null)
                    m_Author = value;

                if (Author != value)
                    m_Author = value;
            }
        }
        #endregion

        // Converts the object into a byte array to be passed over the network.
        public byte[] ToByte() { return Network.Serialize(this); }
    }

    [Serializable]
    public sealed class OKPacket : Packet
    {
        public OKPacket() : base(PacketTypes.OK, null) { }
    }

    [Serializable]
    public sealed class ErrorPacket : Packet
    {
        private string m_Error = string.Empty;

        public ErrorPacket(string message) : base(PacketTypes.Error, null) { Message = message; }

        #region Getters / Setters
        public string Message
        {
            get { return m_Error; }
            private set
            {
                if (value == null || value == string.Empty)
                    return;
                else if (Message == string.Empty)
                    m_Error = value;

                if (Message != value)
                    m_Error = value;
            }
        }
        #endregion
    }
}
