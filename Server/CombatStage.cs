using System.Collections.Generic;
using SUS.Server.Objects;
using SUS.Server.Objects.Items;
using SUS.Server.Objects.Mobiles;
using SUS.Shared;

namespace SUS.Server
{
    public static class CombatStage
    {
        public static List<string> Combat(Mobile m1, Mobile m2)
        {
            var log = new List<string>();

            #region Self Harm

            if (m1 == m2 && m1.IsPlayer)
            {
                // Is the initiator attacking themselves? Do the damage and return.
                log.Add($"You perform {m1.Damage(m1.Attack()) * -1} damage on yourself.");
                if (!m1.Alive) log.Add("You have died.");

                return log;
            }

            #endregion

            m1.Target = m2;
            m2.Target = m1;

            var d = Point2D.Distance(m1, m2);
            if (m1.Weapon.Range < d && m2.Weapon.Range < d) CloseDistance(ref log, m1, m2);

            PerformAttack(m1, ref log, m1, m2);
            PerformAttack(m1, ref log, m2, m1);

            return log;
        }

        private static void PerformAttack(Mobile init, ref List<string> log, Mobile attacker, Mobile target)
        {
            var attackLimit = 1;
            if (attacker.IsPlayer && !target.IsPlayer)
            {
                // Only perform the extra attack if the aggressor is a player.
                var cr = ((IDamageable) attacker).CR;
                var attackChances = cr > 4 ? cr > 10 ? cr > 19 ? 3 : 2 : 1 : 0;
                attackLimit = Utility.Random(20) > attackChances ? 1 : 2;
            }

            var attacksRemaining = attackLimit;
            do
            {
                --attacksRemaining;
                CombatTurn(init, ref log, attacker, target, attackLimit > 1 && attacksRemaining == 0);
            } while (attacksRemaining > 0);
        }

        private static void CloseDistance(ref List<string> log, Mobile m1, Mobile m2)
        {
            var paces = 0;

            var d = m1.Weapon.Range >= m2.Weapon.Range ? m1.Weapon.Range : m2.Weapon.Range;

            // Move to the range of the mobile with the largest attack range.
            while (d < Point2D.Distance(m1, m2))
            {
                m1.Location = Point2D.MoveTowards(m1, m2, m1.Speed);
                paces += m1.Speed;
                if (Point2D.Distance(m1, m2) <= d) break;

                m2.Location = Point2D.MoveTowards(m2, m1, m2.Speed);
                paces += m2.Speed;
            }

            log.Add($"{m1.Name} and {m2.Name} moved {paces} paces towards one another.");
        }

        private static void CombatTurn(Mobile initiator, ref List<string> log, Mobile attacker, Mobile target,
            bool extra = false)
        {
            if (!attacker.Alive || !target.Alive) return;

            #region Check Ranges

            var distance = Point2D.Distance(attacker, target);
            if (attacker is BaseCreature) Ai.PerformAction(attacker, Ai.Actions.Attack);

            if (extra && attacker.Weapon.Range < distance)
                return; // Return because not in range to perform the extra attack.
            if (extra) log.Add("[Attempting an Extra Attack]");

            if (attacker.Weapon.Range < distance)
            {
                attacker.Location = Point2D.MoveTowards(attacker, target, attacker.Speed);
                distance = Point2D.Distance(attacker, target);
                var text = $"{attacker.Name} moves towards {target.Name}";
                if (distance >= 1) text += $" and is now {distance} pace{(distance > 1 ? "s" : "")} away";

                log.Add(text + ".");
            }

            #endregion

            if (attacker.Weapon.IsBow)
                if (attacker.Arrows.Amount == 0)
                {
                    // Remove the weapon from the aggressor due to not having anymore arrows.
                    log.Add($"{attacker.Name} ran out of arrows. [{attacker.Weapon.Name}] was unequipped.");
                    attacker.Unequip(attacker.Weapon);
                }

            // Base for determining miss, hit, or critical.
            var d20 = new DiceRoll("1d20");
            var d20Roll = d20.Roll(); // The rolls.

            // If the target's distance is less than (or equal) to the distance.
            if (attacker.Weapon.Range < Point2D.Distance(attacker, target)) return;

            // Remove required resource.
            if (attacker.Weapon.IsBow) --attacker.Arrows;

            var hit = false;

            if (d20Roll == 1)
                log.Add($"{attacker.Name} attempted but failed to land the attack.");
            else if (d20Roll + attacker.AbilityModifier < target.ArmorClass)
                log.Add($"{attacker.Name} performs an attack but fails to penetrate {target.Name}'s armor.");
            else
                hit = true;

            if (hit)
            {
                // Hit the target, need to calculate additional damage (if it was a critical)
                var critical = false;
                var atkDamage = attacker.Attack();

                if (d20Roll == 20)
                {
                    // A Critical hit, add another attack.
                    critical = true;
                    atkDamage += attacker.Weapon.Damage;
                }

                atkDamage = target.ApplyResistance(attacker.Weapon.DamageType, atkDamage);

                var damageDealt = target.Damage(atkDamage, attacker, attacker.Weapon.IsMagical);
                if (damageDealt == 0) // Failed to do enough damage versus the armor of the target.
                    log.Add($"{attacker.Name} performs an attack but fails to penetrate {target.Name}'s armor.");
                else if (critical) // Performed a critical hit, display the appropriate message.
                    log.Add($"{attacker.Name} performs a critical hit for {damageDealt} damage against {target.Name}.");
                else // Normal hit, nothing special.
                    log.Add($"{attacker.Name} performs {damageDealt} damage to {target.Name}.");

                // Attempt to damage a piece of equipment.
                if (target.IsPlayer)
                    foreach (var equippable in target.Equipment.Values)
                    {
                        if (!equippable.IsArmor || !equippable.DurabilityLoss()) continue;

                        // Log the damage, break because we only damage 1 piece of armor at a time.
                        if (equippable.IsBroken)
                        {
                            log.Add($"{equippable.Name}.");
                            target.Unequip(equippable);
                        }
                        else
                        {
                            log.Add($"{equippable.Name} has suffered durability loss.");
                        }

                        break;
                    }
            }

            // Check for skill increase.
            var skillIncrease = attacker.SkillIncrease(attacker.Weapon.RequiredSkill);
            if (initiator == attacker && skillIncrease != string.Empty) log.Add(skillIncrease);

            // Check for stat increase.
            var statIncrease = attacker.StatIncrease(attacker.Weapon.Stat);
            if (initiator == attacker && statIncrease != string.Empty) log.Add(statIncrease);

            // Attempt durability loss on attackers weapon.
            if (attacker.IsPlayer)
            {
                var weapon = attacker.Weapon;
                if (attacker.Weapon.DurabilityLoss())
                    log.Add(weapon.IsBroken ? $"{weapon.Name}." : $"{weapon.Name} has suffered durability loss.");
            }

            if (target.Alive) return;

            // Killed the target, print and loot.
            attacker.Target = null;
            log.Add($"{attacker.Name} has killed {target.Name}.");
            if (attacker.IsPlayer) (attacker as Player)?.AddKill();

            ExchangeLoot(ref log, attacker, target);
            World.Kill(target);
        }

        private static void ExchangeLoot(ref List<string> log, Mobile m1, Mobile m2)
        {
            if (m1.Alive)
                Loot(ref log, m2, m1);
            else
                Loot(ref log, m1, m2);
        }

        private static void Loot(ref List<string> log, Mobile from, Mobile to)
        {
            // Prevent looting from players. TODO: Subtract from players loots and re-enable.
            if (from.IsPlayer) return;

            foreach (var i in from.Items)
            {
                // Iterate all of the owned items from the target.
                if (i.Type != ItemTypes.Consumable) continue;

                var c = i as Consumable;
                if (c == null)
                    continue;

                var added = to.ConsumableAdd(c);
                if (added > 0) log.Add($"Looted: {added} {c.Name}.");
            }
        }
    }
}