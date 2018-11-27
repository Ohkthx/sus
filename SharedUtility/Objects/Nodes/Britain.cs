using System;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Objects.Nodes
{
    [Serializable]
    public class Britain : Node 
    {
        public Britain() : base(LocationTypes.Town, Locations.Britain, "The greatest city of Britainia.") { }
    }
}
