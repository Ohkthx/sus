using System;
using System.Collections.Generic;
using SUS.Shared.Utilities;
using SUS.Shared.Objects.Mobiles;

namespace SUS.Shared.Objects
{
    public enum ActionType
    {
        None = 0,
        Attack = 1,
        Defend = 2,
        Communicate = 4,
        RangedAttack = 8,
        MeleeAttack = 16,
    }

    public enum AbilityType
    {
        None = 0,
        Primary = 1,
        Secondary = 2,
    }

    [Serializable]
    public class MobileAction
    {
        public ActionType Type = ActionType.None;           // Action being performed.
        public AbilityType Abiltiy = AbilityType.None;      // Ability being used.
        private UInt64 Initator;                            // ID of Player
        private List<Tuple<MobileType, UInt64>> Affected;   // List of Targets
        private List<MobileModifier> Updates = new List<MobileModifier>();
        public string Result = string.Empty;

        public MobileAction(UInt64 initator)
        {
            this.Initator = initator;
            this.Affected = new List<Tuple<MobileType, UInt64>>();   // Initialize our Affected.
        }

        public Serial GetInitator()
        {
            return new Serial(this.Initator);
        }

        public void AddTarget(MobileType type, UInt64 targetId)
        {
            foreach (Tuple<MobileType, UInt64> t in this.Affected)
                if (t.Item1 == type && t.Item2 == targetId)
                    return;

            this.Affected.Add(new Tuple<MobileType, UInt64>(type, targetId));
        }

        public void AddUpdate(MobileModifier mobile)
        {
            int loc = this.Updates.IndexOf(mobile);
            if (loc >= 0)
            {   // Mobile exists in the list, replace it with the new version.
                this.Updates[loc] = mobile;
                return;
            }

            // Loc was -1 indicating it does not exist, add it.
            this.Updates.Add(mobile);
        }

        public List<Tuple<MobileType, UInt64>> GetTargets()
        {
            return this.Affected;
        }

        public List<MobileModifier> GetUpdates()
        {
            return this.Updates;
        }

        public void CleanClientInfo()
        {
            this.Affected = null;
        }

        public byte[] ToByte() { return Network.Serialize(this); }
    }
}
