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
            StatCap = 255;

            // Create our consumables.
            Gold += 1000;
            HealthPotions += 10;
            Bandages += 20;
            Arrows += 50;

            // Give some basic armor and weapons.
            EquipmentAdd(new ArmorSuit(Armor.Materials.Chainmail));
            EquipmentAdd(new CompositeBow(Weapon.Materials.Iron));

            ItemAdd(new TwoHandedSword(Weapon.Materials.Steel));
            ItemAdd(new ShortSword(Weapon.Materials.Steel));
            ItemAdd(new Shield());

            // Skills
            Skills[SkillCode.Archery] += 83.0;
            Skills[SkillCode.Swordsmanship] += 72.0;
            Skills[SkillCode.Healing] += 67.0;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            string paperdoll = base.ToString();
            paperdoll += $"\n  +-[ Statistics ]\n" +
                $"  | +-- Deaths: {Deaths}\n" +
                $"  | +-- Kill Count: {Kills}\n" +
                $"  |\n" +
                $"  +-[ Skills ]\n" +
                $"  | +-- Skills Total: {SkillTotal.ToString("F1")}\n";

            foreach (KeyValuePair<SkillCode, Skill> skill in Skills)
                paperdoll += $"  | +-- [{skill.Value.Value.ToString("F1"),-5} / {skill.Value.Cap.ToString("F1"),-5}] {skill.Value.Name}\n";

            paperdoll += $"  |\n  +-[ Equipment ]\n";
            foreach (KeyValuePair<ItemLayers, Equippable> item in Equipment)
                paperdoll += $"  | +-- {("[" + item.Value.Rating + "]"),-4} {item.Value.Name}\n";

            paperdoll += "  +---------------------------------------------------+";

            return paperdoll;
        }
        #endregion

        #region Getters / Setters
        public override int CR { get { return (int)SkillTotal / 36; } }

        public override DamageTypes Resistances
        {
            get
            {
                DamageTypes resist = DamageTypes.None;
                if (Equipment.ContainsKey(ItemLayers.Armor))
                    resist |= (Equipment[ItemLayers.Armor] as Armor).Resistances;

                if (Equipment.ContainsKey(ItemLayers.Offhand) && Equipment[ItemLayers.Offhand].IsArmor)
                    resist |= (Equipment[ItemLayers.Offhand] as Armor).Resistances;

                return resist;
            }
        }

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
            return Weapon.Damage + ProficiencyModifier +  AbilityModifier;
        }

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
