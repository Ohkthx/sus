using SUS.Objects.Items;
using SUS.Objects.Items.Equipment;
using SUS.Shared;

namespace SUS.Objects.Mobiles
{
    public class Player : Mobile, IPlayer
    {
        private ulong m_PlayerId;
        public bool IsLoggedIn { get; private set; }

        #region Constructors
        public Player(string name, int rawStr, int rawDex, int rawInt, Regions region, Point2D location)
            : base(MobileTypes.Player)
        {
            Name = name;
            Speed = 3;

            InitStats(rawStr, rawDex, rawInt);
            StatCap = 255;

            Region = region;
            Location = location;

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
            Skills[SkillName.Archery].Value = 83.0;
            Skills[SkillName.Swordsmanship].Value = 72.0;
            Skills[SkillName.Healing].Value = 67.0;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            string paperdoll = base.ToString();
            paperdoll += "\n  +-[ Statistics ]\n" +
                $"  | +-- Deaths: {Deaths}\n" +
                $"  | +-- Kill Count: {Kills}\n" +
                "  |\n" +
                "  +-[ Skills ]\n" +
                $"  | +-- Skills Total: {SkillTotal:F1}\n";

            foreach (System.Collections.Generic.KeyValuePair<SkillName, Skill> skill in Skills)
            {
                paperdoll += $"  | +-- [{skill.Value.Value,-5:F1} / {skill.Value.Cap,-5:F1}] {skill.Value.Name}\n";
            }

            paperdoll += "  |\n  +-[ Equipment ]\n";
            foreach (System.Collections.Generic.KeyValuePair<ItemLayers, Equippable> item in Equipment)
            {
                paperdoll += $"  | +-- {("[" + item.Value.Rating + "]"),-4} {item.Value.Name}\n";
            }

            paperdoll += "  +---------------------------------------------------+";

            return paperdoll;
        }
        #endregion

        #region Getters / Setters
        public int CR => (int)SkillTotal / 36;

        public ulong PlayerID
        {
            get => m_PlayerId;
            set
            {
                if (value != PlayerID)
                {
                    m_PlayerId = value;
                }
            }
        }

        protected override DamageTypes Resistances
        {
            get
            {
                DamageTypes resist = DamageTypes.None;
                if (Equipment.ContainsKey(ItemLayers.Armor))
                {
                    resist |= ((Armor)Equipment[ItemLayers.Armor]).Resistances;
                }

                if (Equipment.ContainsKey(ItemLayers.Offhand) && Equipment[ItemLayers.Offhand].IsArmor)
                {
                    resist |= ((Armor)Equipment[ItemLayers.Offhand]).Resistances;
                }

                return resist;
            }
        }

        private int Deaths { get; set; }

        private int Kills { get; set; }

        #endregion

        public void Logout() { IsLoggedIn = false; }
        public void Login() { IsLoggedIn = true; }

        #region Combat
        public override int Attack()
        {
            return Weapon.Damage + ProficiencyModifier + AbilityModifier;
        }

        public void AddKill() { ++Kills; }

        public override void Kill()
        {
            ++Deaths;
            Hits = 0;
        }

        public override void Resurrect()
        {
            Hits = HitsMax / 2;
            Mana = ManaMax / 2;
            Stamina = StaminaMax / 2;
        }
        #endregion
    }
}
