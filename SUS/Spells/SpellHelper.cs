using SUS.Objects;

namespace SUS.Spells
{
    public static class SpellHelper
    {
        public static int Damage(Mobile from, Mobile to, int damage, DamageTypes type)
        {
            damage = to.ApplyResistance(type, damage);
            return to.Damage(damage, from, isMagical: true);
        }

        public static int Heal(Mobile to, int amount)
        {
            int tHits = to.Hits;
            to.Hits += amount;
            return to.Hits - tHits;
        }
    }
}
