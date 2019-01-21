using System;
using System.Collections.Generic;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class GetMobilesPacket : Packet
    {
        private HashSet<BaseMobile> m_Mobiles;
        private Regions m_Region;

        #region Constructors

        public GetMobilesPacket(Regions region, ulong playerId)
            : base(PacketTypes.GetLocalMobiles, playerId)
        {
            Region = region;
        }

        #endregion

        #region Getters / Setters

        public Regions Region
        {
            get => m_Region;
            private set => m_Region = value;
        }

        public HashSet<BaseMobile> Mobiles
        {
            get => m_Mobiles ?? (m_Mobiles = new HashSet<BaseMobile>());
            set
            {
                if (value == null) return;

                m_Mobiles = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class GetMobilePacket : Packet
    {
        [Flags]
        public enum RequestReason
        {
            None = 0,
            Paperdoll = 1,
            Location = 2,
            IsDead = 4,
            Items = 8,
            Equipment = 16
        }

        private Dictionary<int, string> m_Equipment;
        private bool m_IsAlive = true;
        private Dictionary<int, string> m_Items;

        // Requested information to return.
        private string m_Paperdoll;

        private RequestReason m_Reason = RequestReason.None;
        private Regions m_Region = Regions.None;

        #region Constructors

        public GetMobilePacket(RequestReason reason, ulong playerId)
            : base(PacketTypes.GetMobile, playerId)
        {
            Reason = reason;
        }

        #endregion

        public void AddItem(int serial, string name)
        {
            Items.Add(serial, name);
        }

        public void AddEquipment(int serial, string name)
        {
            Equipment.Add(serial, name);
        }

        #region Getters / Setters

        public RequestReason Reason
        {
            get => m_Reason;
            private set
            {
                if (value == RequestReason.None || value == Reason) return;

                m_Reason = value;
            }
        }

        public string Paperdoll
        {
            get => m_Paperdoll ?? (m_Paperdoll = string.Empty);
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                m_Paperdoll = value;
            }
        }

        public Regions Region
        {
            get => m_Region;
            set
            {
                if (value == Regions.None || value == Region) return;

                m_Region = value;
            }
        }

        public bool IsAlive
        {
            get => m_IsAlive;
            set
            {
                if (value != IsAlive) m_IsAlive = value;
            }
        }

        public Dictionary<int, string> Items => m_Items ?? (m_Items = new Dictionary<int, string>());

        public Dictionary<int, string> Equipment => m_Equipment ?? (m_Equipment = new Dictionary<int, string>());

        #endregion
    }

    [Serializable]
    public class GetNodePacket : Packet
    {
        private BaseRegion m_NewRegion;
        private Regions m_Region;

        #region Constructors

        public GetNodePacket(Regions region, ulong playerId)
            : base(PacketTypes.GetNode, playerId)
        {
            Region = region;
        }

        #endregion

        #region Getters / Setters

        public Regions Region
        {
            get => m_Region;
            private set => m_Region = value;
        }

        public BaseRegion NewRegion
        {
            get => m_NewRegion;
            set
            {
                if (NewRegion != value) m_NewRegion = value;
            }
        }

        #endregion
    }
}