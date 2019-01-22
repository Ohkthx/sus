using SUS.Server.Objects.Items;
using SUS.Server.Objects.Items.Equipment;
using SUS.Shared;

namespace SUS.Server.Objects.Mobiles
{
    public class Player : Mobile, IPlayer
    {
        private ulong _playerId;

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

        public bool IsLoggedIn { get; private set; }

        #region Overrides

        public override string ToString()
        {
            var paperdoll = base.ToString();
            paperdoll += "\n  +-[ Statistics ]\n" +
                         $"  | +-- Deaths: {Deaths}\n" +
                         $"  | +-- Kill Count: {Kills}\n" +
                         "  |\n" +
                         "  +-[ Skills ]\n" +
                         $"  | +-- Skills Total: {SkillTotal:F1}\n";

            foreach (var skill in Skills)
                paperdoll += $"  | +-- [{skill.Value.Value,-5:F1} / {skill.Value.Cap,-5:F1}] {skill.Value.Name}\n";

            paperdoll += "  |\n  +-[ Equipment ]\n";
            foreach (var item in Equipment)
                paperdoll += $"  | +-- {"[" + item.Value.Rating + "]",-4} {item.Value.Name}\n";

            paperdoll += "  +---------------------------------------------------+";

            return paperdoll;
        }

        #endregion

        public void Logout()
        {
            IsLoggedIn = false;
        }

        public void Login()
        {
            IsLoggedIn = true;
        }

        #region Getters / Setters

        public int CR => (int) SkillTotal / 36;

        public ulong PlayerID
        {
            get => _playerId;
            set
            {
                if (value != PlayerID) _playerId = value;
            }
        }

        protected override DamageTypes Resistances
        {
            get
            {
                var resist = DamageTypes.None;
                if (Equipment.ContainsKey(ItemLayers.Armor))
                    resist |= ((Armor) Equipment[ItemLayers.Armor]).Resistances;

                if (Equipment.ContainsKey(ItemLayers.Offhand) && Equipment[ItemLayers.Offhand].IsArmor)
                    resist |= ((Armor) Equipment[ItemLayers.Offhand]).Resistances;

                return resist;
            }
        }

        private int Deaths { get; set; }

        private int Kills { get; set; }

        #endregion

        #region Combat

        public override int Attack()
        {
            return Weapon.Damage + ProficiencyModifier + AbilityModifier;
        }

        public void AddKill()
        {
            ++Kills;
        }

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