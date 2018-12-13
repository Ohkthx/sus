using System;
using System.Collections.Generic;
using SUS.Shared.Objects;
using SUS.Shared.Utilities;

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

            combatTurn(m1, ref log, ref m1, ref m2);
            combatTurn(m1, ref log, ref m2, ref m1);

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

        private static void combatTurn(Mobile init, ref List<string> log, ref Mobile aggressor, ref Mobile target)
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

            int distance = aggressor.Coordinate.Distance(target.Coordinate);
            if (aggressor.Weapon.Range < distance)
            {
                distance = aggressor.Coordinate.MoveTowards(target.Coordinate, aggressor.Speed);
                string text = $"{aggressor.Name} moves towards {target.Name}";
                if (distance >= 1)
                    text += $" and is now {distance} pace{(distance > 1 ? "s" : "")} away";
                log.Add(text + ".");
            }

            // Base for determining miss, hit, or crit.
            DiceRoll d20 = new DiceRoll("1d20");
            int d20roll = d20.Roll();                               // The roll.
            int totalroll = d20roll + aggressor.AbilityModifier;    // Roll w/ Ability Modifier.
            
            // If the target's distance is less than (or equal) to the distance.
            if (aggressor.Weapon.Range >= aggressor.Coordinate.Distance(target.Coordinate))
            {
                if (aggressor.Weapon.IsBow)
                {
                    --aggressor.Arrows;
                }

                if (d20roll == 1)
                {   // Attack misses.
                    log.Add($"{aggressor.Name} attempted but failed to land the attack.");
                    return;
                }
                else if (totalroll < target.ArmorRating)
                {
                    log.Add($"{aggressor.Name} performs an attack but fails to penetrate {target.Name}'s armor.");
                    return;
                }

                int atkDamage = aggressor.Attack();
                if (d20roll == 20)
                    log.Add($"{aggressor.Name} performs a critical hit for {target.TakeDamage(atkDamage*2)} damage against {target.Name}.");
                else
                    log.Add($"{aggressor.Name} performs {target.TakeDamage(atkDamage)} damage to {target.Name}.");

                // Check for skill increase.
                string skillIncrease = aggressor.SkillIncrease(aggressor.Weapon.RequiredSkill);
                if (init == aggressor && skillIncrease != string.Empty)
                    log.Add(skillIncrease);

                // Check for stat increase.
                string statIncrease = aggressor.StatIncrease(aggressor.Weapon.Stat);
                if (init == aggressor && statIncrease != string.Empty)
                    log.Add(statIncrease);

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
        {   // Prevent looting from players. TODO: Subtract from players loots and re-enable.
            if (from.IsPlayer)
                return;

            foreach (KeyValuePair<Guid, Item> i in from.Items)
            {   // Iterate all of the owned items from the target.
                if (i.Value.Type != ItemTypes.Consumable)
                    continue;

                Consumable c = i.Value as Consumable;
                int added = to.ConsumableAdd(c);
                if (added > 0)
                {
                    log.Add($"Looted: {added} {c.Name}.");
                }
            }
        }
    }
}
