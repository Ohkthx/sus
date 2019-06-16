using System;
using System.Collections.Generic;
using SUS.Shared;

namespace SUS.Server.Objects.Mobiles
{
    public abstract class NPC : Mobile
    {
        [Flags]
        public enum Services
        {
            None = 0,
            Buy = 1,
            Sell = 2,
            Repair = 4
        }

        #region Constructors

        protected NPC(NPCTypes npcType, Services service)
            : base(MobileTypes.Npc)
        {
            NPCType = npcType;
            Service = service;
            Name = Enum.GetName(typeof(NPCTypes), npcType);
        }

        #endregion

        public abstract Dictionary<int, BaseItem> ServiceableItems(Mobile mobile = null);

        public abstract int ServiceCost(Item item);

        public abstract int PerformService(Mobile mobile, Item item);

        #region Overrides

        public override void Kill()
        {
            Hits = 0;
            IsDeleted = true;
        }

        public override void Resurrect()
        {
            Hits = HitsMax / 2;
        }

        #endregion

        #region Getters / Setters

        public NPCTypes NPCType { get; protected set; }

        public Services Service { get; protected set; }

        #endregion
    }
}