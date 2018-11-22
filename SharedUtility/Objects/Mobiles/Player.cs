using System;
using System.Collections.Generic;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Mobiles
{
    [Serializable]
    public class Player : Mobile
    {
        private int m_Deaths = 0;
        private int m_Kills = 0;
        public bool isLoggedIn { get; private set; } = false;
        private TypeOfDamage DamageType = TypeOfDamage.Melee;

        #region Constructors
        public Player(ulong id, string name, int rawStr, int rawDex, int rawInt) : base(MobileType.Player)
        {
            ID = id;
            Name = name;

            InitStats(rawStr, rawDex, rawInt);
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
                $"  |   +-- Attack: {0}\n" +
                $"  |   +-- Defense: {0}\n" +
                $"  |\n" +
                $"  +-[ Items ]\n" +
                $"  | +-- Bandaids: {0}\t\tBandaid Heal Amount: {0}\n" +
                $"  | +-- Arrows: {0}\t\tReagents: {0}\n" +
                $"  | +-- Gold: {0}\n" +
                $"  | +-- Weapon Type: {DamageType.ToString()}\n" +
                $"  |\n" +
                $"  +-[ Statistics ]\n" +
                $"  | +-- Deaths: {Deaths}\n" +
                $"  | +-- Kill Count: {Kills}\n" +
                $"  |\n" +
                $"  +-[ Skills ]\n";

            foreach (KeyValuePair<int, Skill> skill in Skills)
                paperdoll += $"  | +-- Skill: {skill.Value.Name} => [{skill.Value.Value}  /  {skill.Value.Max}]\n";

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
            int Damage = 0;

            switch (this.DamageType)
            {
                case TypeOfDamage.Archery:
                    Damage = (Dex / 2) + (Str / 3);
                    break;
                case TypeOfDamage.Magic:
                    Damage = (int)((double)Int / 1.5);
                    break;
                case TypeOfDamage.Melee:
                    Damage = (Str / 2) + (Dex / 3);
                    break;
                default:
                    Damage = 4;
                    break;
            }

            return Utility.RandomMinMax(Damage / 2, Damage);
        }

        private void statIncrease()
        {
            int rng = Utility.RandomMinMax(0, 100);
            if (rng > 5)
                return;

            switch (DamageType)
            {
                case TypeOfDamage.Archery:
                    RawDex++;
                    break;
                case TypeOfDamage.Magic:
                    RawInt++;
                    break;
                case TypeOfDamage.Melee:
                    RawStr++;
                    break;
            }
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
