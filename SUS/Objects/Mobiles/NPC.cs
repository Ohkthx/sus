using SUS.Shared;

namespace SUS.Objects.Mobiles
{
    public abstract class Npc : Mobile
    {
        #region Constructors

        protected Npc()
            : base(MobileTypes.Npc)
        { }
        #endregion

        #region Combat
        public override void Kill() { Hits = 0; IsDeleted = true; }
        public override void Resurrect() { Hits = HitsMax / 2; }
        #endregion
    }
}