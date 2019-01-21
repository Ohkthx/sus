using System;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class SocketKillPacket : Packet
    {
        private bool m_Kill;

        public SocketKillPacket(ulong playerId, bool kill = true)
            : base(PacketTypes.SocketKill, playerId)
        {
            Kill = kill;
        }

        #region Getters / Setters

        public bool Kill
        {
            get => m_Kill;
            private set
            {
                if (value != Kill) m_Kill = value;
            }
        }

        #endregion
    }
}