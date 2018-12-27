using System;
using System.Collections.Generic;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class CombatMobilePacket : Packet
    {
        private List<int> m_Targets;     // List of Targets
        private List<string> m_Updates;     // Updates on all.
        private string m_Result;            // Result of the combat.
        public bool IsAlive { get; set; }   // Determines if the Initator (Player) died.

        #region Constructors
        public CombatMobilePacket(UInt64 playerID) 
            : base(PacketTypes.MobileCombat, playerID)
        { }
        #endregion

        #region Getters / Setters
        public List<int> Targets
        {
            get
            {
                if (m_Targets == null)
                    m_Targets = new List<int>();

                return m_Targets;
            }
        }

        public List<string> Updates
        {
            get
            {
                if (m_Updates == null)
                    m_Updates = new List<string>();

                return m_Updates;
            }
        }

        public string Result
        {
            get { return m_Result; }
            set
            {
                if (value == null || value == string.Empty)
                    return;

                m_Result = value;
            }
        }
        #endregion

        public void AddTarget(BaseMobile tag)
        {
            if (!Targets.Contains(tag.Serial))
            {   // Tag is not already in the list, add.
                Targets.Add(tag.Serial);
            }
        }

        public void AddUpdate(List<string> info)
        {
            if (info == null)
                return;

            Updates.AddRange(info);
        }
    }

    [Serializable]
    public class MoveMobilePacket : Packet
    {
        private MobileDirections m_Direction = MobileDirections.None;
        private Regions m_Region;
        private BaseRegion m_NewRegion;

        #region Constructors
        public MoveMobilePacket(Regions region, UInt64 playerID) : this(region, playerID, MobileDirections.None) { }
        public MoveMobilePacket(Regions region, UInt64 playerID, MobileDirections direction) 
            : base(PacketTypes.MobileMove, playerID)
        {
            Region = region;
            Direction = direction;
        }
        #endregion

        #region Getters / Setters
        public MobileDirections Direction
        {
            get { return m_Direction; }
            set
            {
                if (value == MobileDirections.None || value == Direction) 
                    return; // Prevent assigning a bad value or reassigning.

                m_Direction = value;
            }
        }

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
                if (value == null)
                    return;
                else if (NewRegion == null)
                    m_NewRegion = value;

                if (NewRegion != value)
                    m_NewRegion = value;
            }
        }
        #endregion
    }

    [Serializable]
    public class RessurrectMobilePacket : Packet
    {
        private Regions m_Region;        // Region to be sent to.
        private bool m_Success = false;

        #region Constructors
        public RessurrectMobilePacket(UInt64 playerID) 
            : base(PacketTypes.MobileResurrect, playerID)
        { }
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

        public bool isSuccessful
        {
            get { return m_Success; }
            set
            {
                if (value != isSuccessful)
                    m_Success = value;
            }
        }
        #endregion
    }

    [Serializable]
    public class UseItemPacket : Packet
    {
        public int Item { get; private set; }
        private string m_Response = string.Empty;

        #region Constructor
        public UseItemPacket(int serial, UInt64 playerID) 
            : base(PacketTypes.UseItem, playerID)
        {
            Item = serial;
        }
        #endregion

        #region Getters / Setters
        public string Response
        {
            get { return m_Response; }
            set
            {
                if (value == null || value == string.Empty)
                    return;

                m_Response = value;
            }
        }
        #endregion
    }
}
