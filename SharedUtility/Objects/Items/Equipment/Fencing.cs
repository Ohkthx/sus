using System;
using SUS.Shared.Utilities;

namespace SUS.Shared.Objects.Items.Equipment
{
    [Serializable]
    public class Dagger : OneHanded
    {
        public Dagger(Materials material) : base(material, "1d6")
        {
            Name = "Dagger";
            RequiredSkill = SkillCode.Fencing;
            Stat = StatCode.Dexterity;
            DamageType = DamageTypes.Piercing;
        }
    }

    [Serializable]
    public class Kryss : OneHanded
    {
        public Kryss(Materials material) : base(material, "1d8")
        {
            Name = "Kryss";
            RequiredSkill = SkillCode.Fencing;
            Stat = StatCode.Dexterity;
            DamageType = DamageTypes.Piercing;
        }
    }
}
