using System;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class AccountAuthenticatePacket : Packet
    {
        private string _name;

        #region Constructors

        public AccountAuthenticatePacket(ulong playerId, string name)
        {
            PlayerId = playerId;
            Name = name;
        }

        #endregion

        #region Getters / Setters

        public string Name
        {
            get => _name;
            private set
            {
                if (value == string.Empty || value == Name)
                    return;

                _name = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class AccountClientPacket : Packet
    {
        private ClientState _clientState;

        #region Getters / Setters

        public ClientState ClientState
        {
            get => _clientState;
            set
            {
                if (value == null)
                    return;

                if (ClientState != value)
                    _clientState = value;
            }
        }

        #endregion
    }
}