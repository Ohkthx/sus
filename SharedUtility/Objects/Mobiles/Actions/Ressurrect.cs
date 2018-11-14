using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles
{
    [Serializable]
    public class Ressurrect
    {
        public Node Node { get; private set; }      // Node the Mobile will be sent to.
        public Mobile Mobile { get; private set; }  // Mobile that was ressurrected.

        public Ressurrect(Node node, Mobile mobile) { this.Node = node; this.Mobile = mobile; }

        public byte[] ToByte() { return Network.Serialize(this); }
    }
}
