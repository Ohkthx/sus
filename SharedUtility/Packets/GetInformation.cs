using System;
using System.Collections.Generic;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class GetMobilesPacket : Packet
    {
        private Regions m_Region;
        private HashSet<BaseMobile> m_Mobiles;

        #region Constructors
        public GetMobilesPacket(Regions region, UInt64 playerID) 
            : base(PacketTypes.GetLocalMobiles, playerID)
        {
            Region = region;
        }
        #endregion

        #region Getters / Setters
        public Regions Region
        {
            get { return m_Region; }
            set
            {
                if (Region != value)
                    m_Region = value;
            }
        }

        public HashSet<BaseMobile> Mobiles
        {
            get
            {
                if (m_Mobiles == null)
                    m_Mobiles = new HashSet<BaseMobile>();

                return m_Mobiles;
            }

            set
            {
                if (value == null)
                    return;

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
            None        = 0,
            Paperdoll   = 1,
            Location    = 2,
            IsDead      = 4,
            Items       = 8,
            Equipment   = 16,
        }

        private RequestReason m_Reason = RequestReason.None;

        // Requested information to return.
        private string m_Paperdoll                              = null;
        private Regions m_Region                                = Regions.None;
        private bool m_IsAlive                                  = true;
        private Dictionary<int, string> m_Items              = null;
        private Dictionary<int, string> m_Equipment          = null;

        #region Constructors
        public GetMobilePacket(RequestReason reason, UInt64 playerID) 
            : base(PacketTypes.GetMobile, playerID)
        {
            Reason = reason;
        }
        #endregion

        #region Getters / Setters
        public RequestReason Reason
        {
            get { return m_Reason; }
            private set
            {
                if (value == RequestReason.None || value == Reason)
                    return;

                m_Reason = value;
            }
        }

        public string Paperdoll
        {
            get
            {
                if (m_Paperdoll == null)
                    m_Paperdoll = string.Empty;

                return m_Paperdoll;
            }
            set
            {
                if (value == null || value == string.Empty)
                    return;

                m_Paperdoll = value;
            }
        }
 
        public Regions Region
        {
            get { return m_Region; }
            set
            {
                if (value == Regions.None || value == Region)
                    return;

                m_Region = value;
            }
        }

        public bool IsAlive
        {
            get { return m_IsAlive; }
            set
            {
                if (value != IsAlive)
                    m_IsAlive = value;
            }
        }

        public Dictionary<int, string> Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new Dictionary<int, string>();

                return m_Items;
            }

            set
            {
                if (value == null)
                    return;

                m_Items = value;
            }

        }

        public Dictionary<int, string> Equipment
        {
            get
            {
                if (m_Equipment == null)
                    m_Equipment = new Dictionary<int, string>();

                return m_Equipment;
            }
            set
            {
                if (value == null)
                    return;

                m_Equipment = value;
            }
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
    }

    [Serializable]
    public class GetNodePacket : Packet
    {
        private Regions m_Region;
        private BaseRegion m_NewRegion;

        #region Constructors
        public GetNodePacket(Regions region, UInt64 playerID) 
            : base(PacketTypes.GetNode, playerID)
        {
            Region = region;
        }
        #endregion

        #region Getters / Setters
        public Regions Region
        {
            get { return m_Region; }
            set
            {
                if (Region != value)
                    m_Region = value;
            }
        }

        public BaseRegion NewRegion
        {
            get { return m_NewRegion; }
            set
            {
                if (NewRegion != value)
                    m_NewRegion = value;
            }
        }
        #endregion
    }
}
