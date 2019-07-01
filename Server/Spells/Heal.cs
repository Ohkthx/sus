using SUS.Server.Objects;
using SUS.Shared;

namespace SUS.Server.Spells
{
    public class Heal : Spell
    {
        public Heal()
            : base(SpellName.Heal, SpellCircle.Second)
        {
        }

        public override int Effect(Mobile caster, Mobile target)
        {
            if (caster.Mana < ManaRequired)
                return 0;

            caster.Mana -= ManaRequired;
            return SpellHelper.Heal(target, Utility.RandomMinMax(10, 17));
        }
    }
}