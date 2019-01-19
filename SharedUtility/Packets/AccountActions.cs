using System;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class AccountAuthenticatePacket : Packet
    {
        private string m_Name;

        #region Constructors

        public AccountAuthenticatePacket(ulong playerId, string name)
            : base(PacketTypes.Authenticate, playerId)
        {
            Name = name;
        }

        #endregion

        #region Getters / Setters

        public string Name
        {
            get => m_Name;
            private set
            {
                if (value == string.Empty || value == Name) return;

                m_Name = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class AccountClientPacket : Packet
    {
        private ClientState m_ClientState;

        #region Constructors

        public AccountClientPacket(ulong playerId)
            : base(PacketTypes.ClientState, playerId)
        {
        }

        #endregion

        #region Getters / Setters

        public ClientState ClientState
        {
            get => m_ClientState;
            set
            {
                if (value == null) return;

                if (ClientState != value) m_ClientState = value;
            }
        }

        #endregion
    }
}