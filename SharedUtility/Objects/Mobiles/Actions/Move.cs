using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles
{
    [Serializable]
    public class MobileMove
    {
        public Locations Node { get; private set; }     // Node the Mobile will be sent to.
        public MobileTag Mobile { get; private set; }   // Mobile to look up.

        public MobileMove(Locations node, Mobile mobile) : this (node, new MobileTag(mobile)) { }

        public MobileMove(Locations node, MobileTag mobile)
        {
            Node = node;
            Mobile = mobile;
        }

        public byte[] ToByte() { return Network.Serialize(this); }
    }
}
