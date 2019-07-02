using System;
using System.Collections.Generic;
using SUS.Shared;

namespace SUS.Server.Objects.Mobiles
{
    public abstract class Npc : Mobile
    {
        [Flags]
        public enum Services
        {
            None = 0,
            Buy = 1 << 0,
            Sell = 1 << 1,
            Repair = 1 << 2,
            All = 1 << 3
        }

        private Services _services;

        #region Constructors

        protected Npc(NpcTypes npcType, Services service)
            : base(MobileTypes.Npc)
        {
            NpcType = npcType;
            Service = service;
            Name = Enum.GetName(typeof(NpcTypes), npcType);
        }

        #endregion

        public abstract Dictionary<BaseItem, int> ServiceableItems(Mobile mobile = null);

        public abstract int ServicePrice(Item item);

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

        public NpcTypes NpcType { get; protected set; }

        public Services Service
        {
            get => _services & ~Services.All;
            private set => _services = value;
        }

        public bool AllowAll => (_services & Services.All) == Services.All;

        #endregion
    }
}