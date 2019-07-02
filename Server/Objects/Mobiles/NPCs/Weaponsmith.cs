using System;
using System.Collections.Generic;
using SUS.Server.Objects.Items;
using SUS.Shared;

namespace SUS.Server.Objects.Mobiles.NPCs
{
    public class Weaponsmith : Npc
    {
        private int _stockSize;

        private void Restock()
        {
            // Restock our vendor.
            while (Inventory.Count < StockSize)
            {
                var mat = Weapon.Materials.None;
                var type = WeaponTypes.None;
                do
                {
                    if (mat == Weapon.Materials.None)
                        mat = Utility.RandomEnum<Weapon.Materials>();

                    if (type == WeaponTypes.None)
                        type = Utility.RandomEnum<WeaponTypes>();
                } while (mat == Weapon.Materials.None || type == WeaponTypes.None);

                try
                {
                    var weaponStock = Factory.GetWeapon(type, mat);
                    Inventory.Add(weaponStock.Serial, weaponStock);
                }
                catch (InvalidFactoryException ife)
                {
                    Utility.ConsoleNotify(ife.Message);
                }
                catch (Exception e)
                {
                    Utility.ConsoleNotify("[Factory] => " + e.Message);
                }
            }
        }

        #region Constructors

        public Weaponsmith() : this(5)
        {
        }

        public Weaponsmith(int stockSize) : base(NpcTypes.Weaponsmith, Services.Sell)
        {
            PriceModifier = 100; // _priceModifier * armor.Rating() = price.
            StockSize = stockSize;
        }

        #endregion

        #region Getters / Setters

        public Dictionary<Serial, Weapon> Inventory { get; } = new Dictionary<Serial, Weapon>();

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
                throw new UnknownItemException("That item no longer exists.");

            if (!Inventory.ContainsKey(item.Serial))
                throw new UnknownItemException($"{item.Name} is no longer available.");

            return Inventory[item.Serial].Rating * PriceModifier;
        }

        public override Dictionary<BaseItem, int> ServiceableItems(Mobile mobile = null)
        {
            Restock();

            var weapons = new Dictionary<BaseItem, int>();
            foreach (var (_, item) in Inventory)
                weapons.Add(item.Base(), item.Rating * PriceModifier);

            return weapons;
        }

        public override int PerformService(Mobile mobile, Item item)
        {
            if (mobile == null)
                throw new UnknownMobileException("An unknown mobile attempted to use the Weaponsmith.");

            if (item == null || !(item is Weapon weapon))
                throw new UnknownItemException("That item no longer exists.");

            if (!Inventory.ContainsKey(weapon.Serial))
                throw new UnknownItemException("The vendor no longer has that item.");

            if (mobile.Gold.Amount < weapon.Rating * PriceModifier)
                throw new NotEnoughGoldException();

            Inventory.Remove(weapon.Serial);
            mobile.Gold -= weapon.Rating * PriceModifier;
            mobile.AddItem(item);

            return weapon.Rating * PriceModifier;
        }

        #endregion
    }
}