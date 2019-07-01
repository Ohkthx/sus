using System;
using System.Collections.Generic;
using SUS.Shared;

namespace SUS.Server.Objects.Mobiles.NPCs
{
    public class Repairer : NPC
    {
        private int _repairRate;

        #region Getters / Setters

        public int RepairRate
        {
            get => _repairRate;
            protected set
            {
                if (value < 0)
                    value = 0;

                _repairRate = value;
            }
        }

        #endregion

        #region Constructors

        public Repairer() : this(3)
        {
        }

        public Repairer(int repairRate) : base(NPCTypes.Repairer, Services.Repair)
        {
            RepairRate = repairRate;
        }

        #endregion

        #region Overrides

        public override int Attack()
        {
            throw new NotImplementedException();
        }

        public override int ServicePrice(Item item)
        {
            if (!(item is IDestroyable destroyable))
                return 0;

            if (destroyable.Invulnerable)
                return 0;

            if (destroyable.Durability == destroyable.DurabilityMax)
                return 0;

            return (destroyable.DurabilityMax - destroyable.Durability) * RepairRate;
        }

        public override Dictionary<BaseItem, int> ServiceableItems(Mobile mobile = null)
        {
            if (mobile == null)
                return null; // Used for error.

            var items = new Dictionary<BaseItem, int>();
            foreach (var item in mobile.Items)
            {
                // Ignore the item if there is no cost to service.
                if (ServicePrice(item) == 0)
                    continue;

                // Add the item.
                items.Add(item.Base(), ServicePrice(item));
            }

            return items.Count == 0 ? null : items;
        }

        public override int PerformService(Mobile mobile, Item item)
        {
            if (!(item is IDestroyable destroyable))
                return 0;

            if (destroyable.Invulnerable)
                return 0;

            var missingDurability = destroyable.DurabilityMax - destroyable.Durability;
            if (missingDurability <= 0)
                return 0;

            // Amount the gold will cost.
            var goldCost = missingDurability * RepairRate;

            // Adjust the repair rate.
            if (goldCost > mobile.Gold.Amount)
                missingDurability = mobile.Gold.Amount / RepairRate;

            // Remove the gold from the user.
            mobile.RemoveConsumable(ConsumableTypes.Gold, goldCost);

            // Restore the amount that can be afforded.
            destroyable.Restore(missingDurability);

            return goldCost;
        }

        #endregion
    }
}