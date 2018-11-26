using System;
using SUS.Shared.Objects;

namespace SUS.Shared.Packets
{
    [Serializable]
    public sealed class SocketKillPacket : Packet
    {
        private bool m_Kill;

        public SocketKillPacket(MobileTag user, bool kill = true) : base(PacketTypes.SocketKill, user)
        {
            Kill = kill;
        }

        #region Getters / Setters
        public bool Kill
        {
            get { return m_Kill; }
            private set
            {
                if (value != Kill)
                    m_Kill = value;
            }
        }
        #endregion
    }
}
