using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class ShortSword : OneHanded
    {
        public ShortSword(Materials material) : base(material, "1d6")
        {
            Name = "Short Sword";
            RequiredSkill = SkillCode.Swordsmanship;
            Stat = StatCode.Strength;
            DamageType = DamageTypes.Slashing;
        }
    }

    [Serializable]
    public class TwoHandedSword : TwoHanded
    {
        public TwoHandedSword(Materials material) : base(material, "2d6")
        {
            Name = "Two-Handed Sword";
            RequiredSkill = SkillCode.Swordsmanship;
            Stat = StatCode.Strength;
            DamageType = DamageTypes.Slashing;
        }
    }
}
