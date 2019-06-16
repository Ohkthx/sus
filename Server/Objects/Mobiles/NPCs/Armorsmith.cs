using System;
using System.Collections.Generic;
using SUS.Server.Objects.Items;
using SUS.Server.Objects.Items.Equipment;
using SUS.Shared;

namespace SUS.Server.Objects.Mobiles.NPCs
{
    public class Armorsmith : NPC
    {
        private int _stockSize;

        private void Restock()
        {
            // Restock our vendor.
            while (Inventory.Count < StockSize)
            {
                Armor.Materials mat;
                do
                {
                    mat = Utility.RandomEnum<Armor.Materials>();
                } while (mat == Armor.Materials.Cloth);

                var armorStock = new ArmorSuit(mat);
                Inventory.Add(armorStock.Serial, armorStock);
            }
        }

        #region Constructors

        public Armorsmith() : this(5)
        {
        }

        public Armorsmith(int stockSize) : base(NPCTypes.Armorsmith, Services.Sell)
        {
            PriceModifier = 100; // _priceModifier * armor.Rating() = price.
            StockSize = stockSize;
        }

        #endregion

        #region Getters / Setters

        public Dictionary<Serial, Armor> Inventory { get; } = new Dictionary<Serial, Armor>();

        private int PriceModifier { get; }

        public int StockSize
        {
            get => _stockSize;
            private set => _stockSize = value < 0 ? 0 : value;
        }

        #endregion

        #region Overrides

        public override int Attack()
        {
            throw new NotImplementedException();
        }

        public override int ServicePrice(Item item)
        {
            if (item == null)
                throw new ItemNotFoundException("That item no longer exists.");

            if (!Inventory.ContainsKey(item.Serial))
                throw new ItemNotFoundException($"{item.Name} is no longer available.");

            return Inventory[item.Serial].Rating * PriceModifier;
        }

        public override Dictionary<BaseItem, int> ServiceableItems(Mobile mobile = null)
        {
            Restock();

            var armorSuits = new Dictionary<BaseItem, int>();
            foreach (var (_, item) in Inventory) armorSuits.Add(item.Base(), item.Rating * PriceModifier);

            return armorSuits;
        }

        public override int PerformService(Mobile mobile, Item item)
        {
            if (mobile == null)
                throw new MobileNotFoundException("An unknown mobile attempted to use the Armorsmith.");
            if (item == null || !(item is Armor armor))
                throw new ItemNotFoundException("That item no longer exists.");
            if (!Inventory.ContainsKey(armor.Serial))
                throw new ItemNotFoundException("The vendor no longer has that item.");
            if (mobile.Gold.Amount < armor.Rating * PriceModifier)
                throw new NotEnoughGoldException();

            Inventory.Remove(armor.Serial);
            mobile.Gold -= armor.Rating * PriceModifier;
            mobile.AddItem(item);

            return armor.Rating * PriceModifier;
        }

        #endregion
    }
}