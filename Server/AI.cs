using System.Collections.Generic;
using SUS.Server.Objects;
using SUS.Server.Objects.Items;
using SUS.Server.Objects.Items.Equipment;

namespace SUS.Server
{
    public static class Ai
    {
        public enum Actions
        {
            Attack
        }

        public enum Types
        {
            Melee,
            Archer,
            Mage
        }

        public static void PerformAction(Mobile creature, Actions action)
        {
            switch (action)
            {
                case Actions.Attack:
                    var distance = 0;
                    if (creature.Target != null && creature.Target.Location.IsValid)
                        distance = Point2D.Distance(creature, creature.Target);

                    Attack(creature, distance);
                    break;
            }
        }

        private static void Attack(Mobile creature, int distance)
        {
            var weapons = new List<Weapon>();
            foreach (var i in creature.Items)
            {
                if (i.IsEquippable && ((Equippable) i).IsWeapon)
                    weapons.Add(i as Weapon);
            }

            var weapon = creature.Weapon;
            foreach (var w in weapons)
            {
                if (w.IsBow && creature.Arrows.Amount == 0)
                    continue;

                // Attempt to equip the longest ranged weapon.
                if (distance > 1 && w.Range > weapon.Range)
                    weapon = w; // Range is greater than our current range.
                else if (distance > 1 && w.Range == weapon.Range && w.Rating > creature.AttackRating)
                    weapon = w; // Range is equal, but the attack rating is higher.
                else if (w.Rating > weapon.Rating && w.Rating > creature.AttackRating)
                    weapon = w;
            }

            if (!(weapon is Unarmed) && weapon != creature.Weapon)
                creature.Equip(weapon);
        }
    }
}