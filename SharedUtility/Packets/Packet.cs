using System;

namespace SUS.Shared.Packets
{
    public enum PacketTypes
    {
        OK,
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
        private PacketTypes m_Type;     // Type of the packet.
        private UInt64 m_PlayerID;      // Author / Owner of the packet.

        // Creates an instance of a Request based on supplied Type and Object.
        public Packet(PacketTypes type, UInt64 playerID)
        {
            Type = type;
            PlayerID = playerID;
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

        public UInt64 PlayerID
        {
            get { return m_PlayerID; }
            private set
            {
                if (value != PlayerID)
                    m_PlayerID = value;
            }
        }
        #endregion

        // Converts the object into a byte array to be passed over the network.
        public byte[] ToByte() { return Network.Serialize(this); }
    }

    [Serializable]
    public class OKPacket : Packet
    {
        public OKPacket() : base(PacketTypes.OK, 0) { }
    }

    [Serializable]
    public class ErrorPacket : Packet
    {
        private string m_Error = string.Empty;

        public ErrorPacket(string message) : base(PacketTypes.Error, 0) { Message = message; }

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
