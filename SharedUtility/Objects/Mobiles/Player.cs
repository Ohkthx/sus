using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles
{
    public interface IPlayableClass
    {
        string ToString();
        int GetHashCode();
        bool Equals(Object obj);
        void ToInsert(ref SQLiteCommand cmd);
    }

    [Serializable]
    public class Player : Mobile
    {
        private int m_Deaths = 0;
        private int m_Kills = 0;
        public bool isLoggedIn { get; private set; } = false;

        #region Constructors
        public Player(ulong id, string name, int rawStr, int rawDex, int rawInt) : base(MobileType.Player)
        {
            ID = id;
            Name = name;

            InitStats(rawStr, rawDex, rawInt);

            // Create our consumables.
            InitConsumables();

            // Give some basic armor and weapons.
            EquipmentAdd(new Armor(ItemLayers.Chest, ArmorMaterials.Leather, "Leather Chest"));
            EquipmentAdd(new Armor(ItemLayers.Legs, ArmorMaterials.Leather, "Leather Leggings"));
            EquipmentAdd(new Armor(ItemLayers.Feet, ArmorMaterials.Leather, "Leather Boots"));

            EquipmentAdd(new Weapon(ItemLayers.MainHand, WeaponMaterials.Wooden, "Short Sword", "1d6"));
            EquipmentAdd(new Armor(ItemLayers.Offhand, ArmorMaterials.Leather, "Leather Shield"));

            EquipmentAdd(new Weapon(ItemLayers.Bow, WeaponMaterials.Wooden, "Composite Bow", "1d8"));
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            string paperdoll = $"                  ___________________\n" +
                $"                  [Character Profile]\n" +
                $"  + ---------------------------------------------------+\n" +
                $"  | Character Name: {Name}\n" +
                $"  | Title: {"The Player"}\n" +
                $"  | Location: {Location.ToString()}\n" +
                $"  |\n" +
                $"  +-[ Attributes ]\n" +
                $"  | +-- Health: {Hits}, Max Health: {HitsMax}\n" +
                $"  | +-- Strength: {Str}\n" +
                $"  | +-- Dexterity: {Dex}\t\tStamina: {Stam}\n" +
                $"  | +-- Intelligence: {Int}\tMana: {Mana}\n" +
                $"  |   +-- Attack: {WeaponRating}\n" +
                $"  |   +-- Defense: {ArmorRating}\n" +
                $"  |\n" +
                $"  +-[ Items ]\n" +
                $"  | +-- Bandaids: {0}\t\tBandaid Heal Amount: {0}\n" +
                $"  | +-- Arrows: {0}\t\tReagents: {0}\n" +
                $"  | +-- Gold: {0}\n" +
                $"  | +-- Weapon: {Weapon.Name}\n" +
                $"  |\n" +
                $"  +-[ Statistics ]\n" +
                $"  | +-- Deaths: {Deaths}\n" +
                $"  | +-- Kill Count: {Kills}\n" +
                $"  |\n" +
                $"  +-[ Skills ]\n";

            foreach (KeyValuePair<int, Skill> skill in Skills)
                paperdoll += $"  | +-- Skill: {skill.Value.Name} => [{skill.Value.Value}  /  {skill.Value.Max}]\n";

            paperdoll += $"  |\n  +-[ Equipment ]\n";
            foreach (KeyValuePair<ItemLayers, Equippable> item in Equipment)
                paperdoll += $"  | +-- Item: {item.Value.Name} => {item.Value.Rating}, {item.Value.Layer.ToString()}\n";

            paperdoll += "  +---------------------------------------------------+";

            return paperdoll;
        }
        #endregion

        #region Getters / Setters
        public int Deaths
        {
            get { return m_Deaths; }
            set
            {
                if (value < 0)
                    value = 0;

                if (value != m_Deaths)
                    m_Deaths = value;
            }
        }

        public int Kills
        {
            get { return m_Kills; }
            set
            {
                if (value < 0)
                    value = 0;

                if (value != m_Kills)
                    m_Kills = value;
            }
        }
        #endregion

        public void Logout() { isLoggedIn = false; }
        public void Login() { isLoggedIn = true; }

        #region Combat
        public override int Attack()
        {
            if (Weapon.IsBow && Arrows.Amount <= 0)
                Unequip(ItemLayers.Bow);

            int Damage = Weapon.Damage * (Weapon.Rating /2 );
            return Utility.RandomMinMax(Damage / 2, Damage);
        }

        private void statIncrease()
        {
        }

        private void skillIncrease() { }

        public void AddKill() { m_Kills++; }

        public override void Kill()
        {
            m_Deaths++;
            Hits = 0;
        }

        public override void Ressurrect()
        {
            Hits = HitsMax / 2;
            Mana = ManaMax / 2;
            Stam = StamMax / 2;
        }
        #endregion
    }
}
