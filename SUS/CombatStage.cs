using System.Collections.Generic;
using SUS.Shared;
using SUS.Objects;
using SUS.Objects.Items;

namespace SUS
{
    public static class CombatStage
    {
        public static List<string> Combat(Mobile m1, Mobile m2)
        {
            List<string> log = new List<string>();

            #region Self Harm
            if (m1 == m2 && m1.IsPlayer)
            {   // Is the initiator attacking themself? Do the damage and return.
                log.Add($"You perform {m1.Damage(m1.Attack()) * -1} damage on yourself.");
                if (!m1.Alive)
                    log.Add("You have died.");
                return log;
            }
            #endregion

            m1.Target = m2;
            m2.Target = m1;

            int d = Point2D.Distance(m1, m2);
            if (m1.Weapon.Range < d && m2.Weapon.Range < d)
            {   // Have the targets move towards each other.
                closeDistance(ref log, m1, m2);
            }

            performAttack(m1, ref log, m1, m2);
            performAttack(m1, ref log, m2, m1);

            return log;
        }

        private static void performAttack(Mobile init, ref List<string> log, Mobile attacker, Mobile target)
        {
            int attackLimit = 1;
            if (attacker.IsPlayer && !target.IsPlayer)
            {   // Only perform the extra attack if the aggressor is a player.
                int CR = attacker.CR;
                int attackChances = CR > 4 ? CR > 10 ? CR > 19 ? 3 : 2 : 1 : 0;
                attackLimit = Utility.Random(20) > attackChances ? 1 : 2;
            }

            int attacksRemaining = attackLimit;
            do
            {
                --attacksRemaining;
                combatTurn(init, ref log, attacker, target, extra: attackLimit > 1 && attacksRemaining == 0);
            } while (attacksRemaining > 0);
        }

        private static void closeDistance(ref List<string> log, Mobile m1, Mobile m2)
        {
            int paces, d;
            paces = d = 0;

            if (m1.Weapon.Range >= m2.Weapon.Range)
                d = m1.Weapon.Range;    // Assign the distance to m1's weapon range.
            else
                d = m2.Weapon.Range;    // Assign the distance to m2's weapon range.

            // Move to the range of the mobile with the largest attack range.
            while (d < Point2D.Distance(m1, m2))
            {
                m1.Location = Point2D.MoveTowards(m1, m2, m1.Speed);
                paces += m1.Speed;
                if (Point2D.Distance(m1, m2) <= d)
                    break;

                m2.Location = Point2D.MoveTowards(m2, m1, m2.Speed);
                paces += m2.Speed;
            }

            log.Add($"{m1.Name} and {m2.Name} moved {paces} paces towards one another.");
        }

        private static void combatTurn(Mobile initiator, ref List<string> log, Mobile atttacker, Mobile target, bool extra = false)
        {
            if (!atttacker.Alive || !target.Alive)
                return;

            #region Check Ranges
            int distance = Point2D.Distance(atttacker, target);
            if (atttacker is BaseCreature)
                AI.PerformAction(atttacker, AI.Actions.Attack);

            if (extra && atttacker.Weapon.Range < distance)
                return;     // Return because not in range to perform the extra attack.
            else if (extra)
                log.Add("[Attempting an Extra Attack]");

            if (atttacker.Weapon.Range < distance)
            {
                atttacker.Location = Point2D.MoveTowards(atttacker, target, atttacker.Speed);
                distance = Point2D.Distance(atttacker, target);
                string text = $"{atttacker.Name} moves towards {target.Name}";
                if (distance >= 1)
                    text += $" and is now {distance} pace{(distance > 1 ? "s" : "")} away";
                log.Add(text + ".");
            }
            #endregion

            if (atttacker.Weapon.IsBow)
            {
                if (atttacker.Arrows.Amount == 0)
                {   // Remove the weapon from the aggressor due to not having anymore arrows.
                    log.Add($"{atttacker.Name} ran out of arrows. [{atttacker.Weapon.Name}] was unequipped.");
                    atttacker.Unequip(atttacker.Weapon);
                }
            }

            // Base for determining miss, hit, or crit.
            DiceRoll d20 = new DiceRoll("1d20");
            int d20roll = d20.Roll();                                // The rolls.

            // If the target's distance is less than (or equal) to the distance.
            if (atttacker.Weapon.Range >= Point2D.Distance(atttacker, target))
            {
                if (atttacker.Weapon.IsBow)
                {   // Remove the consumable.
                    --atttacker.Arrows;
                }

                bool hit = false;

                if (d20roll == 1)
                    log.Add($"{atttacker.Name} attempted but failed to land the attack.");
                else if ((d20roll + atttacker.AbilityModifier) < target.ArmorClass)
                    log.Add($"{atttacker.Name} performs an attack but fails to penetrate {target.Name}'s armor.");
                else
                    hit = true;

                if (hit)
                {   // Hit the target, need to calculate additional damage (if it was a critical)
                    bool crit = false;
                    int atkDamage = atttacker.Attack();

                    if (d20roll == 20)
                    {   // A Critical hit, add another attack.
                        crit = true;
                        atkDamage += atttacker.Weapon.Damage;
                    }

                    int tatk = atkDamage;
                    atkDamage = target.ApplyResistance(atttacker.Weapon.DamageType, atkDamage);

                    int damageDealt = target.Damage(atkDamage, atttacker, isMagical: atttacker.Weapon.IsMagical);
                    if (damageDealt == 0)   // Failed to do enough damage versus the armor of the target.
                        log.Add($"{atttacker.Name} performs an attack but fails to penetrate {target.Name}'s armor.");
                    else if (crit)          // Performed a critical hit, display the appropriate message.
                        log.Add($"{atttacker.Name} performs a critical hit for {damageDealt} damage against {target.Name}.");
                    else                    // Normal hit, nothing special.
                        log.Add($"{atttacker.Name} performs {damageDealt} damage to {target.Name}.");
                }

                // Check for skill increase.
                string skillIncrease = atttacker.SkillIncrease(atttacker.Weapon.RequiredSkill);
                if (initiator == atttacker && skillIncrease != string.Empty)
                    log.Add(skillIncrease);

                // Check for stat increase.
                string statIncrease = atttacker.StatIncrease(atttacker.Weapon.Stat);
                if (initiator == atttacker && statIncrease != string.Empty)
                    log.Add(statIncrease);

                if (!target.Alive)
                {   // Killed the target, print and loot.
                    atttacker.Target = null;
                    log.Add($"{atttacker.Name} has killed {target.Name}.");
                    if (atttacker.IsPlayer)
                        (atttacker as Player).AddKill();
                    exchangeLoot(ref log, atttacker, target);
                    World.Kill(target);
                }
            }
        }

        private static void exchangeLoot(ref List<string> log, Mobile m1, Mobile m2)
        {
            if (m1.Alive)
                loot(ref log, m2, m1);
            else
                loot(ref log, m1, m2);
        }

        private static void loot(ref List<string> log, Mobile from, Mobile to)
        {   // Prevent looting from players. TODO: Subtract from players loots and re-enable.
            if (from.IsPlayer)
                return;

            foreach (Item i in from.Items.Values)
            {   // Iterate all of the owned items from the target.
                if (i.Type != ItemTypes.Consumable)
                    continue;

                Consumable c = i as Consumable;
                int added = to.ConsumableAdd(c);
                if (added > 0)
                {
                    log.Add($"Looted: {added} {c.Name}.");
                }
            }
        }
    }
}
