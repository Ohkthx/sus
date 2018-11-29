using System;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Packets
{
    [Serializable]
    public sealed class AccountAuthenticatePacket : Packet
    {
        private BasicMobile m_Player;

        #region Constructors
        public AccountAuthenticatePacket(BasicMobile mobile) : base (PacketTypes.Authenticate, mobile) { }
        public AccountAuthenticatePacket(UInt64 id, string name) : this(new BasicMobile(Guid.Empty, id, MobileType.Player, name)) { }
        #endregion

        #region Getters / Setters
        public BasicMobile Player
        {
            get { return m_Player; }
            set
            {
                if (value == null)
                    return;
                else if (Player == null)
                    m_Player = value;

                if (Player != value)
                    m_Player = value;
            }
        }
        #endregion
    }

    [Serializable]
    public sealed class AccountGameStatePacket : Packet
    {
        private GameState m_GameState;

        #region Constructors
        public AccountGameStatePacket(BasicMobile mobile) : base(PacketTypes.GameState, mobile) { }
        #endregion

        #region Getters / Setters
        public GameState GameState 
        {
            get { return m_GameState; }
            set
            {
                if (value == null)
                    return;
                else if (GameState == null)
                    m_GameState = value;

                if (GameState != value)
                    m_GameState = value;
            }
        }
        #endregion
    }
}
