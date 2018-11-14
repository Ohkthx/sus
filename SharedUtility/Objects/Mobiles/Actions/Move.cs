using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles
{
    [Serializable]
    public class MobileMove
    {
        public Locations NodeID { get; private set; }     // Node the Mobile will be sent to.
        public Mobile Mobile { get; private set; }  // Mobile that was ressurrected.

        public MobileMove(Locations id, Mobile mobile) { this.NodeID = id; this.Mobile = mobile; }

        public byte[] ToByte() { return Network.Serialize(this); }
    }
}
