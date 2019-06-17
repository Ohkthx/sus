namespace SUS.Server.Objects.Items.Equipment
{
    public class ShortSword : OneHanded
    {
        public ShortSword(Materials material)
            : base(WeaponTypes.ShortSword, material, "1d6")
        {
            Name = "Short Sword";
            RequiredSkill = SkillName.Swordsmanship;
            Stat = StatCode.Strength;
            DamageType = DamageTypes.Slashing;
        }
    }

    public class TwoHandedSword : TwoHanded
    {
        public TwoHandedSword(Materials material)
            : base(WeaponTypes.TwoHandedSword, material, "2d6")
        {
            Name = "Two-Handed Sword";
            RequiredSkill = SkillName.Swordsmanship;
            Stat = StatCode.Strength;
            DamageType = DamageTypes.Slashing;
        }
    }
}