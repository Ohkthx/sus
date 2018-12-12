using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SUS.Shared.Utilities;
using SUS.Shared.Objects.Items.Equipment;

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
        public Player(ulong id, string name, int rawStr, int rawDex, int rawInt) : base(Types.Player)
        {
            ID = id;
            Name = name;
            Speed = 3;

            InitStats(rawStr, rawDex, rawInt);

            // Create our consumables.
            InitConsumables(1000, 10, 50);

            // Give some basic armor and weapons.
            EquipmentAdd(new Armor(ItemLayers.Head, Armor.Materials.Leather, "Leather Cap"));
            EquipmentAdd(new Armor(ItemLayers.Neck, Armor.Materials.Leather, "Leather Gorget"));
            EquipmentAdd(new Armor(ItemLayers.Chest, Armor.Materials.Leather, "Leather Chest"));
            EquipmentAdd(new Armor(ItemLayers.Arms, Armor.Materials.Leather, "Leather Sleeves"));
            EquipmentAdd(new Armor(ItemLayers.Hands, Armor.Materials.Leather, "Leather Gloves"));
            EquipmentAdd(new Armor(ItemLayers.Legs, Armor.Materials.Leather, "Leather Leggings"));
            EquipmentAdd(new Armor(ItemLayers.Feet, Armor.Materials.Leather, "Leather Boots"));
            EquipmentAdd(new CompositeBow());

            ItemAdd(new TwoHandedSword());
            ItemAdd(new ShortSword());
            ItemAdd(new Armor(ItemLayers.Offhand, Armor.Materials.Leather, "Leather Shield"));
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
                $"  | +-- Arrows: {Arrows.Amount}\t\tReagents: {0}\n" +
                $"  | +-- Gold: {Gold.Amount}\n" +
                $"  | +-- Weapon: {Weapon.Name}\n" +
                $"  |\n" +
                $"  +-[ Statistics ]\n" +
                $"  | +-- Deaths: {Deaths}\n" +
                $"  | +-- Kill Count: {Kills}\n" +
                $"  |\n" +
                $"  +-[ Skills ]\n";

            foreach (KeyValuePair<Skill.Types, Skill> skill in Skills)
                paperdoll += $"  | +-- Skill: {skill.Value.Name} => [{skill.Value.Value}  /  {skill.Value.Cap}]\n";

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
            return Weapon.Damage + AbilityModifier;
        }

        private void statIncrease()
        {
        }

        private void skillIncrease() { }

        public void AddKill() { ++m_Kills; }

        public override void Kill()
        {
            ++m_Deaths;
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
