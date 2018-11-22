using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles
{
    [Serializable]
    public class Ressurrect
    {
        private Locations m_Location = Locations.None;  // Location to be sent to.
        private MobileTag m_Mobile = null;              // Mobile to act upon.
        private bool m_Success = false;

        #region Constructors
        public Ressurrect(Locations loc, Mobile mobile) :this(loc, new MobileTag(mobile)) { }

        public Ressurrect(Locations loc, MobileTag mobile, bool success = false)
        {
            Location = loc;
            Mobile = mobile;
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

        public MobileTag Mobile
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

        public bool isSuccessful
        {
            get { return m_Success; }
            private set
            {
                if (value != isSuccessful)
                    m_Success = value;
            }
        }
        #endregion

        public byte[] ToByte() { return Network.Serialize(this); }
    }
}
