using System;
using System.Collections.Generic;
using SUS.Shared.Objects;

namespace SUS.Shared.Packets
{
    [Serializable]
    public sealed class GetMobilesPacket : Packet
    {
        private Locations m_Location;
        private HashSet<MobileTag> m_Mobiles = null;

        public GetMobilesPacket(Locations location) : base(PacketTypes.GetLocalMobiles, null) { Location = location; }

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

        public HashSet<MobileTag> Mobiles
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
        private MobileTag m_Tag;
        private Mobile m_Mobile = null;

        public GetMobilePacket(MobileTag mobile) : base(PacketTypes.GetMobile, null) { Tag = mobile; }

        #region Getters / Setters
        public MobileTag Tag
        {
            get { return m_Tag; }
            private set
            {
                if (value == null)
                    return;
                else if (Tag == null)
                    m_Tag = value;

                if (Tag != value)
                    m_Tag = value;
            }
        }

        public Mobile Mobile 
        {
            get { return m_Mobile; }
            set
            {
                if (value == null)
                    return;
                else if (Mobile == null)
                    m_Mobile = value;

                if (Mobile != value)
                    m_Mobile = value;
            }
        }
        #endregion
    }

    [Serializable]
    public sealed class GetNodePacket : Packet
    {
        private Locations m_Location;
        private Node m_NewLocation = null;

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

        public Node NewLocation
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
