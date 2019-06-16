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
            AddEquipment(new ArmorSuit(Armor.Materials.Chainmail));
            AddEquipment(new CompositeBow(Weapon.Materials.Iron));

            AddItem(new TwoHandedSword(Weapon.Materials.Steel));
            AddItem(new ShortSword(Weapon.Materials.Steel));
            AddItem(new Shield());

            // Default Equipment
            AddItem(new ArmorSuit(Armor.Materials.Cloth));

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

            foreach (var skill in Skills.Values)
                paperdoll += $"  | +-- [{skill.Value,-5:F1} / {skill.Cap,-5:F1}] {skill.Name}\n";

            paperdoll += "  |\n  +-[ Equipment ]\n";
            foreach (var equippable in Equipment.Values)
            {
                if (equippable.IsWeapon && equippable.IsStarter) continue;
                paperdoll +=
                    $"  | +-- {"[" + equippable.Rating + "]",-4} {equippable.Name} {(!equippable.Invulnerable ? $"[Durability: {equippable.Durability} / {equippable.DurabilityMax}]" : string.Empty)}\n";
            }

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

        public void AddUnlockedRegion(Regions region)
        {
            UnlockedRegions |= region;
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

        public Regions UnlockedRegions { get; protected set; }

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