using SUS.Objects;
using SUS.Shared;

namespace SUS.Spells
{
    public class Fireball : Spell
    {
        public Fireball()
            : base(SpellName.Fireball, SpellCircle.Second)
        {
        }

        public override int Effect(Mobile caster, Mobile target)
        {
            if (caster.Mana < ManaRequired) return 0;

            caster.Mana -= ManaRequired;
            return SpellHelper.Damage(caster, target, Utility.RandomMinMax(10, 17), DamageTypes.Fire);
        }
    }
}