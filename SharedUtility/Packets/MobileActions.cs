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
        public bool IsAlive { get; set; }   // Determines if the Initiator (Player) died.

        #region Constructors
        public CombatMobilePacket(ulong playerId)
            : base(PacketTypes.MobileCombat, playerId)
        { }
        #endregion

        #region Getters / Setters
        public List<int> Targets => m_Targets ?? (m_Targets = new List<int>());

        public List<string> Updates => m_Updates ?? (m_Updates = new List<string>());

        public string Result
        {
            get => m_Result;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

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
            {
                return;
            }

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

        public MoveMobilePacket(Regions region, ulong playerId, MobileDirections direction = MobileDirections.None)
            : base(PacketTypes.MobileMove, playerId)
        {
            Region = region;
            Direction = direction;
        }

        #endregion

        #region Getters / Setters
        public MobileDirections Direction
        {
            get => m_Direction;
            private set
            {
                if (value == MobileDirections.None || value == Direction)
                {
                    return; // Prevent assigning a bad value or reassigning.
                }

                m_Direction = value;
            }
        }

        public Regions Region
        {
            get => m_Region;
            private set
            {
                if (Region != value)
                {
                    m_Region = value;
                }
            }
        }

        public BaseRegion NewRegion
        {
            get => m_NewRegion;
            set
            {
                if (NewRegion != value)
                {
                    m_NewRegion = value;
                }
            }
        }
        #endregion
    }

    [Serializable]
    public class ResurrectMobilePacket : Packet
    {
        private Regions m_Region;        // Region to be sent to.
        private bool m_Success;

        #region Constructors
        public ResurrectMobilePacket(ulong playerId)
            : base(PacketTypes.MobileResurrect, playerId)
        { }
        #endregion

        #region Getters / Setters
        public Regions Region
        {
            get => m_Region;
            set
            {
                if (Region != value)
                {
                    m_Region = value;
                }
            }
        }

        public bool IsSuccessful
        {
            get => m_Success;
            set
            {
                if (value != IsSuccessful)
                {
                    m_Success = value;
                }
            }
        }
        #endregion
    }

    [Serializable]
    public class UseItemPacket : Packet
    {
        public int Item { get; }
        private string m_Response = string.Empty;

        #region Constructor
        public UseItemPacket(int serial, ulong playerId)
            : base(PacketTypes.UseItem, playerId)
        {
            Item = serial;
        }
        #endregion

        #region Getters / Setters
        public string Response
        {
            get => m_Response;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                m_Response = value;
            }
        }
        #endregion
    }
}
