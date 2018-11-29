using System;
using System.Collections.Generic;
using SUS.Shared.Objects;

namespace SUS.Shared.Packets
{
    [Serializable]
    public sealed class CombatMobilePacket : Packet
    {
        private List<BasicMobile> Targets;      // List of Targets
        private List<string> Updates;           // Updates on all.
        private bool m_IsDead;                  // Determines if the Initator (Player) died.
        public string Result = string.Empty;

        #region Constructors
        public CombatMobilePacket(BasicMobile mobile) : base(PacketTypes.MobileCombat, mobile) { }
        #endregion

        #region Getters / Setters
        public bool IsDead
        {
            get { return m_IsDead; }
            set
            {
                if (value != IsDead)
                    m_IsDead = value;
            }
        }
        #endregion

        public void AddTarget(BasicMobile tag)
        {
            if (Targets == null)
            {   // List is unassigned, create and add.
                Targets = new List<BasicMobile>();
                Targets.Add(tag);
                return;
            }
            else if (!Targets.Contains(tag))
            {   // Tag is not already in the list, add.
                Targets.Add(tag);
            }
        }

        public void AddUpdate(List<string> info)
        {
            if (info == null)
                return;

            if (Updates == null)
            {   // List does not exist. Just assign it.
                Updates = info;
                return;
            }

            Updates.AddRange(info);
        }

        public List<BasicMobile> GetTargets()
        {
            return Targets;
        }

        public List<string> GetUpdates()
        {
            return Updates;
        }

        public void CleanClientInfo()
        {
            this.Targets = null;
        }
    }

    [Serializable]
    public sealed class MoveMobilePacket : Packet
    {
        private MobileDirections m_Direction = MobileDirections.None;
        private Locations m_Location;
        private BasicNode m_NewLocation = null;

        #region Constructors
        public MoveMobilePacket(Locations location, BasicMobile mobile) : this(location, mobile, MobileDirections.None) { }
        public MoveMobilePacket(Locations location, BasicMobile mobile, MobileDirections direction) : base(PacketTypes.MobileMove, mobile)
        {
            Location = location;
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

    [Serializable]
    public sealed class RessurrectMobilePacket : Packet
    {
        private Locations m_Location;       // Location to be sent to.
        private bool m_Success = false;

        #region Constructors
        public RessurrectMobilePacket(Locations loc, Mobile mobile) : this(loc, new BasicMobile(mobile)) { }
        public RessurrectMobilePacket(Locations loc, BasicMobile mobile, bool success = false) : base(PacketTypes.MobileResurrect, mobile)
        {
            Location = loc;
            isSuccessful = success;
        }
        #endregion

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
}

