using System;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class AccountAuthenticatePacket : Packet
    {
        private string m_Name;
        private BaseMobile m_Player;

        #region Constructors
        public AccountAuthenticatePacket(UInt64 playerID, string name)
            : base(PacketTypes.Authenticate, playerID)
        {
            Name = name;
        }
        #endregion

        #region Getters / Setters
        public string Name
        {
            get { return m_Name; }
            set
            {
                if (value == string.Empty || value == Name)
                    return;

                m_Name = value;
            }
        }

        public BaseMobile Player
        {
            get { return m_Player; }
            set
            {
                if (value == null)
                    return;

                if (Player != value)
                    m_Player = value;
            }
        }
        #endregion
    }

    [Serializable]
    public class AccountClientPacket : Packet
    {
        private ClientState m_ClientState;

        #region Constructors
        public AccountClientPacket(UInt64 playerID) 
            : base(PacketTypes.ClientState, playerID)
        { }
        #endregion

        #region Getters / Setters
        public ClientState ClientState 
        {
            get { return m_ClientState; }
            set
            {
                if (value == null)
                    return;

                if (ClientState != value)
                    m_ClientState = value;
            }
        }
        #endregion
    }
}
