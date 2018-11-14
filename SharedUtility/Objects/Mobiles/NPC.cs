using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles
{
    [Serializable]
    public abstract class NPC : Mobile
    {
        #region Constructors
        public NPC(): base(MobileType.NPC) { ID = Serial.NewObject; }
        #endregion

        #region Combat
        public override void Kill() { Hits = 0; }

        public override void Ressurrect()
        {
            Hits = HitsMax / 2;
            Mana = ManaMax / 2;
            Stam = StamMax / 2;
        }
        #endregion
    }
}