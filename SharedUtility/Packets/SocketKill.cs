using System;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class SocketKillPacket : Packet
    {
        private bool m_Kill;

        public SocketKillPacket(UInt64 playerID, bool kill = true) 
            : base(PacketTypes.SocketKill, playerID)
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
