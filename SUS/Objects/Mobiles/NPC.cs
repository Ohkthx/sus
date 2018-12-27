using SUS.Shared;

namespace SUS.Objects
{
    public abstract class NPC : Mobile
    {
        #region Constructors
        public NPC()
            : base(MobileTypes.NPC)
        { }
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