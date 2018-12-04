using System;
using System.Collections.Generic;
using SUS.Shared.Objects;

namespace SUS.Server
{
    public static class CombatStage
    {
        public static List<string> Combat(ref Mobile aggressor, ref Mobile target)
        {
            List<string> output = new List<string>();
            int initAtk = aggressor.Attack();

            #region Self Harm
            if (aggressor == target && aggressor.IsPlayer)
            {   // Is the initiator attacking themself? Do the damage and return.
                output.Add($"You perform {aggressor.TakeDamage(initAtk) * -1} damage on yourself.");
                if (aggressor.IsDead)
                    output.Add("You have died.");
                return output;
            }
            #endregion

            bool inRange = CombatStage.inRange(aggressor, target);
            if (!aggressor.Weapon.IsBow && !target.Weapon.IsBow && !inRange)
            {   // Have the targets move towards each other.
                closeDistance(ref aggressor, ref target);
                output.Add($"{aggressor.Name} and {target.Name} move {aggressor.Coordinate.Distance(target.Coordinate)} paces towards each other.");
            }

            #region Aggressor's Turn
            if (inRange || aggressor.Weapon.IsBow)
            {
                output.Add($"You perform {target.TakeDamage(initAtk) * -1} damage to {target.Name}.");
                if (target.IsDead)
                {
                    output.Add($"You have killed {target.Name}.");
                    return output;
                }
            }
            else if (!inRange)
            {   // Move the target 1 space towards the aggressor.
                aggressor.Coordinate.MoveTowards(target.Coordinate);
                output.Add($"{aggressor.Name} moves towards {target.Name} and is now {target.Coordinate.Distance(aggressor.Coordinate)} paces away.");
            }
            #endregion

            #region TARGET's Turn
            if (inRange || target.Weapon.IsBow)
            {
                int oppAtk = target.Attack();
                output.Add($"You take {aggressor.TakeDamage(oppAtk) * -1} damage from {target.Name}.");
                if (aggressor.IsDead)
                    output.Add("You have died.");
            }
            else if (!inRange)
            {   // Move the aggressor towards the target.
                target.Coordinate.MoveTowards(aggressor.Coordinate);
                output.Add($"{target.Name} moves towards {aggressor.Name} and is now {target.Coordinate.Distance(aggressor.Coordinate)} paces away.");
            }
            #endregion

            return output;
        }

        private static bool inRange(Mobile m1, Mobile m2)
        {
            return m1.Coordinate.Distance(m2.Coordinate) <= 1;
        }

        private static void closeDistance(ref Mobile m1, ref Mobile m2)
        {
            while (!inRange(m1, m2))
            {
                m1.Coordinate.MoveTowards(m2.Coordinate);
                if (inRange(m1, m2))
                    break;

                m2.Coordinate.MoveTowards(m1.Coordinate);
            }
        }
    }
}
