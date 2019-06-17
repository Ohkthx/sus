namespace SUS.Server.Objects.Items.Equipment
{
    public class Dagger : OneHanded
    {
        public Dagger(Materials material)
            : base(WeaponTypes.Dagger, material, "1d6")
        {
            Name = "Dagger";
            RequiredSkill = SkillName.Fencing;
            Stat = StatCode.Dexterity;
            DamageType = DamageTypes.Piercing;
        }
    }

    public class Kryss : OneHanded
    {
        public Kryss(Materials material)
            : base(WeaponTypes.Kryss, material, "1d8")
        {
            Name = "Kryss";
            RequiredSkill = SkillName.Fencing;
            Stat = StatCode.Dexterity;
            DamageType = DamageTypes.Piercing;
        }
    }
}