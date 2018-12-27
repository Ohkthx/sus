using System.Collections.Generic;
using SUS.Objects;
using SUS.Objects.Items;
using SUS.Objects.Items.Equipment;

namespace SUS
{
    public static class AI
    {
        public enum Types
        {
            Melee,
            Archer,
            Mage,
        }

        public enum Actions
        {
            Attack,
            Move,
        }

        public static void PerformAction(Mobile creature, Actions action)
        {
            switch(action)
            {
                case Actions.Attack:
                    int distance = 0;
                    if (creature.Target != null && creature.Target.Location.IsValid)
                        distance = Point2D.Distance(creature, creature.Target);
                    attack(creature, distance);
                    break;
            }
        }

        private static void attack(Mobile creature, int distance)
        {
            List<Weapon> weapons = new List<Weapon>();
            foreach (Item i in creature.Items.Values)
                if (i.IsEquippable && (i as Equippable).IsWeapon)
                    weapons.Add(i as Weapon);

            Weapon weapon = creature.Weapon;
            foreach (Weapon w in weapons)
            {
                if (w.IsBow && creature.Arrows.Amount == 0)
                    continue;

                // Attempt to equip the longest ranged weapon.
                if (distance > 1 && w.Range > weapon.Range)
                    weapon = w;     // Range is greater than our current range.
                else if (distance > 1 && w.Range == weapon.Range && w.Rating > creature.AttackRating)
                    weapon = w;     // Range is equal, but the attack rating is higher.
                else if (w.Rating > weapon.Rating && w.Rating > creature.AttackRating)
                    weapon = w;
            }

            if (!(weapon is Unarmed) && weapon != creature.Weapon)
            {
                creature.Equip(weapon);
            }
        }
    }
}
