using System;
using System.Collections.Generic;
using SUS.Shared.Objects;

namespace SUS.Shared.Packets
{
    [Serializable]
    public sealed class GetMobilesPacket : Packet
    {
        private Locations m_Location;
        private HashSet<BasicMobile> m_Mobiles = null;

        public GetMobilesPacket(Locations location, BasicMobile relative) : base(PacketTypes.GetLocalMobiles, relative) { Location = location; }

        #region Getters / Setters
        public Locations Location
        {
            get { return m_Location; }
            set
            {
                if (Location != value)
                    m_Location = value;
            }
        }

        public HashSet<BasicMobile> Mobiles
        {
            get { return m_Mobiles; }
            set
            {
                if (value == null)
                    return;
                else if (Mobiles == null)
                    m_Mobiles = value;
                else
                    m_Mobiles = value;
            }
        }
        #endregion
    }

    [Serializable]
    public sealed class GetMobilePacket : Packet
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

        private BasicMobile m_Target;
        private RequestReason m_Reason = RequestReason.None;

        // Requested information to return.
        private string m_Paperdoll                              = null;
        private Locations m_Location                            = Locations.None;
        private bool m_IsDead                                   = false;
        private Dictionary<Guid, Item> m_Items                  = null;
        private Dictionary<ItemLayers, Equippable> m_Equipment  = null;


        public GetMobilePacket(BasicMobile target, RequestReason reason) : base(PacketTypes.GetMobile, null)
        {
            Target = target;
            Reason = reason;
        }

        #region Getters / Setters
        public BasicMobile Target
        {
            get { return m_Target; }
            private set
            {
                if (value == null)
                    return;
                else if (Target == null)
                    m_Target = value;

                if (Target != value)
                    m_Target = value;
            }
        }

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
 
        public Locations Location
        {
            get { return m_Location; }
            set
            {
                if (value == Locations.None || value == Location)
                    return;

                m_Location = value;
            }
        }

        public bool IsDead
        {
            get { return m_IsDead; }
            set
            {
                if (value != IsDead)
                    m_IsDead = value;
            }
        }

        public Dictionary<Guid, Item> Items
        {
            get
            {
                if (m_Items == null)
                    m_Items = new Dictionary<Guid, Item>();

                return m_Items;
            }
            set
            {
                if (value == null)
                    return;

                m_Items = value;
            }

        }

        public Dictionary<ItemLayers, Equippable> Equipment
        {
            get
            {
                if (m_Equipment == null)
                    m_Equipment = new Dictionary<ItemLayers, Equippable>();

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
    }

    [Serializable]
    public sealed class GetNodePacket : Packet
    {
        private Locations m_Location;
        private BasicNode m_NewLocation = null;

        public GetNodePacket(Locations location) : base(PacketTypes.GetNode, null) { Location = location; }

        #region Getters / Setters
        public Locations Location
        {
            get { return m_Location; }
            set
            {
                if (Location != value)
                    m_Location = value;
            }
        }

        public BasicNode NewLocation
        {
            get { return m_NewLocation; }
            set
            {
                if (value == null)
                    return;
                else if (NewLocation == null)
                    m_NewLocation = value;

                if (NewLocation != value)
                    m_NewLocation = value;
            }
        }
        #endregion
    }
}
