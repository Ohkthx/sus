using SUS.Objects;
using SUS.Spells;

namespace SUS.Shared.Spells
{
    public class Heal : Spell
    {
        public Heal()
            : base(SpellName.Heal, SpellCircle.Second)
        { }

        public override int Effect(Mobile caster, Mobile target)
        {
            if (caster.Mana >= ManaRequired)
            {
                caster.Mana -= ManaRequired;
                return SpellHelper.Heal(target, Utility.RandomMinMax(10, 17));
            }

            return 0;
        }

    }
}
