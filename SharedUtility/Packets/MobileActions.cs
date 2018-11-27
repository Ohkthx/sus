using System;
using System.Collections.Generic;
using SUS.Shared.Objects;

namespace SUS.Shared.Packets
{
    [Serializable]
    public sealed class CombatMobilePacket : Packet
    {
        private List<MobileTag> Affected;       // List of Targets
        private List<MobileModifier> Updates;   // Updates on all.
        public string Result = string.Empty;

        public CombatMobilePacket(MobileTag mobile) : base(PacketTypes.MobileCombat, mobile) { }

        public void AddTarget(MobileTag tag)
        {
            if (Affected == null)
            {   // List is unassigned, create and add.
                Affected = new List<MobileTag>();
                Affected.Add(tag);
                return;
            }
            else if (!Affected.Contains(tag))
            {   // Tag is not already in the list, add.
                Affected.Add(tag);
            }
        }

        public void AddUpdate(MobileModifier mobile)
        {
            if (Updates == null)
            {   // List does not exist. Create it, add, and return.
                Updates = new List<MobileModifier>();
                Updates.Add(mobile);
                return;
            }

            int loc = Updates.IndexOf(mobile);
            if (loc >= 0)
            {   // Mobile exists in the list, replace it with the new version.
                Updates[loc] = mobile;
                return;
            }

            // Loc was -1 indicating it does not exist, add it.
            Updates.Add(mobile);
        }

        public List<MobileTag> GetTargets()
        {
            return Affected;
        }

        public List<MobileModifier> GetUpdates()
        {
            return Updates;
        }

        public void CleanClientInfo()
        {
            this.Affected = null;
        }
    }

    [Serializable]
    public sealed class MoveMobilePacket : Packet
    {
        private Locations m_Location;
        private NodeTag m_NewLocation = null;

        #region Constructors
        public MoveMobilePacket(Locations location, Mobile mobile) : this(location, new MobileTag(mobile)) { }
        public MoveMobilePacket(Locations location, MobileTag mobile) : base(PacketTypes.MobileMove, mobile)
        {
            Location = location;
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

        public NodeTag NewLocation
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
        public RessurrectMobilePacket(Locations loc, Mobile mobile) : this(loc, new MobileTag(mobile)) { }
        public RessurrectMobilePacket(Locations loc, MobileTag mobile, bool success = false) : base(PacketTypes.MobileResurrect, mobile)
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

