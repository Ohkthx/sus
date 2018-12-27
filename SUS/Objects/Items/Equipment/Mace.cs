﻿namespace SUS.Objects.Items.Equipment
{
    public class Mace : OneHanded
    {
        public Mace(Materials material) 
            : base(material, "1d6")
        {
            Name = "Mace";
            RequiredSkill = SkillName.Macefighting;
            Stat = StatCode.Strength;
            DamageType = DamageTypes.Bludgeoning;
        }
    }

    public class Maul : TwoHanded
    {
        public Maul(Materials material) 
            : base(material, "2d6")
        {
            Name = "Maul";
            RequiredSkill = SkillName.Macefighting;
            Stat = StatCode.Strength;
            DamageType = DamageTypes.Bludgeoning;
        }
    }
}