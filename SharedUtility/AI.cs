using System;
using System.Collections.Generic;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Items.Equipment;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared
{
    [Serializable]
    public static class AI
    {
        [Serializable]
        public enum Types
        {
            Melee,
            Archer,
            Mage,
        }

        [Serializable]
        public enum Actions
        {
            Attack,
            Move,
        }

        public static void PerformAction(ref Mobile creature, Actions action, Coordinate destination = null)
        {
            switch(action)
            {
                case Actions.Attack:
                    int distance = 0;
                    if (destination != null)
                        distance = creature.Coordinate.Distance(destination);
                    attack(ref creature, distance);
                    break;
            }
        }

        private static void attack(ref Mobile creature, int distance)
        {
            List<Weapon> weapons = new List<Weapon>();
            foreach (KeyValuePair<Guid, Item> i in creature.Items)
                if (i.Value.IsEquippable && (i.Value as Equippable).IsWeapon)
                    weapons.Add(i.Value as Weapon);

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
