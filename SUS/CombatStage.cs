using System;
using System.Collections.Generic;
using SUS.Shared.Objects;

namespace SUS.Server
{
    public static class CombatStage
    {
        public static List<string> Combat(ref Mobile m1, ref Mobile m2)
        {
            List<string> log = new List<string>();

            #region Self Harm
            if (m1 == m2 && m1.IsPlayer)
            {   // Is the initiator attacking themself? Do the damage and return.
                log.Add($"You perform {m1.TakeDamage(m1.Attack()) * -1} damage on yourself.");
                if (m1.IsDead)
                    log.Add("You have died.");
                return log;
            }
            #endregion

            int d = m1.Coordinate.Distance(m2.Coordinate);
            if (m1.Weapon.Range < d && m2.Weapon.Range < d)
            {   // Have the targets move towards each other.
                closeDistance(ref log, ref m1, ref m2);
            }

            combatTurn(ref log, ref m1, ref m2);
            combatTurn(ref log, ref m2, ref m1);

            return log;
        }

        private static void closeDistance(ref List<string> log, ref Mobile m1, ref Mobile m2)
        {
            int paces, d;
            paces = d = 0;

            if (m1.Weapon.Range >= m2.Weapon.Range)
                d = m1.Weapon.Range;    // Assign the distance to m1's weapon range.
            else
                d = m2.Weapon.Range;    // Assign the distance to m2's weapon range.

            // Move to the range of the mobile with the largest attack range.
            while (d < m1.Coordinate.Distance(m2.Coordinate))
            {
                m1.Coordinate.MoveTowards(m2.Coordinate, m1.Speed);
                paces += m1.Speed;
                if (m1.Coordinate.Distance(m2.Coordinate) <= d)
                    break;

                m2.Coordinate.MoveTowards(m1.Coordinate, m2.Speed);
                paces += m2.Speed;
            }

            log.Add($"{m1.Name} and {m2.Name} moved {paces} paces towards one another.");
        }

        private static void combatTurn(ref List<string> log, ref Mobile aggressor, ref Mobile target)
        {
            if (aggressor.IsDead || target.IsDead)
                return;

            if (aggressor.Weapon.IsBow)
            {
                if (aggressor.Arrows.Amount == 0)
                {   // Remove the weapon from the aggressor due to not having anymore arrows.
                    log.Add($"{aggressor.Name} ran out of arrows. [{aggressor.Weapon.Name}] was unequipped.");
                    aggressor.Unequip(aggressor.Weapon);
                }
            }

            if (aggressor.Weapon.Range < aggressor.Coordinate.Distance(target.Coordinate))
            {
                aggressor.Coordinate.MoveTowards(target.Coordinate, aggressor.Speed);
                log.Add($"{aggressor.Name} moves towards {target.Name} and is now {target.Coordinate.Distance(aggressor.Coordinate)} paces away.");
            }

            // If the target's distance is less than (or equal) to the distance.
            if (aggressor.Weapon.Range >= aggressor.Coordinate.Distance(target.Coordinate))
            {
                if (aggressor.Weapon.IsBow)
                {
                    Consumable c = aggressor.Arrows;
                    --c;
                    aggressor.ItemAdd(c);
                }

                log.Add($"{aggressor.Name} performs {target.TakeDamage(aggressor.Attack()) * -1} damage to {target.Name}.");
                if (target.IsDead)
                {
                    log.Add($"{aggressor.Name} has killed {target.Name}.");
                    exchangeLoot(ref log, ref aggressor, ref target);
                }
            }
        }

        private static void exchangeLoot(ref List<string> log, ref Mobile m1, ref Mobile m2)
        {
            if (m1.IsDead)
                loot(ref log, ref m2, m1);
            else
                loot(ref log, ref m1, m2);
        }

        private static void loot(ref List<string> log, ref Mobile to, Mobile from)
        {
            Dictionary<Consumable.ConsumableTypes, Consumable> fromC = new Dictionary<Consumable.ConsumableTypes, Consumable>();
            foreach(KeyValuePair<Guid, Item> i in from.Items)
                if (i.Value.Type == ItemTypes.Consumable)
                    fromC.Add((i.Value as Consumable).ConsumableType, i.Value as Consumable);

            foreach (KeyValuePair<Guid, Item> i in to.Items)
            {
                if (i.Value.Type == ItemTypes.Consumable)
                {
                    Consumable c = i.Value as Consumable;
                    if (fromC.ContainsKey(c.ConsumableType))
                    {
                        Consumable fromCItem = fromC[c.ConsumableType];
                        if (fromCItem.Amount > 0)
                        {   // Only continue if there are more than 0 to loot.
                            int amt = c.Add(fromCItem.Amount);
                            if (amt > 0)
                            {   // Only add the item if 1 or more is looted.
                                log.Add($"Looted: {amt} => {c.Name}.");
                                to.ItemAdd(c);
                            }
                        }
                    }
                }
            }
        }
    }
}
