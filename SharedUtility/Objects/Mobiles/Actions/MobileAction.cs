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
        private List<MobileTag> Affected;                   // List of Targets
        private List<MobileModifier> Updates;
        public string Result = string.Empty;

        public MobileAction(UInt64 initator)
        {
            Initator = initator;
        }

        public Serial GetInitator()
        {
            return new Serial(Initator);
        }

        public void AddTarget(MobileTag tag)
        {
            if (Affected == null)
            {   // List is unassigned, create and add.
                Affected = new List<MobileTag>();
                Affected.Add(tag);
                return;
            }
            else if (!Affected.Contains(tag))
            {   // Tag is not already in the list, add.
                Affected.Add(tag);
            }
        }

        public void AddUpdate(MobileModifier mobile)
        {
            if (Updates == null)
            {   // List does not exist. Create it, add, and return.
                Updates = new List<MobileModifier>();
                Updates.Add(mobile);
                return;
            }

            int loc = Updates.IndexOf(mobile);
            if (loc >= 0)
            {   // Mobile exists in the list, replace it with the new version.
                Updates[loc] = mobile;
                return;
            }

            // Loc was -1 indicating it does not exist, add it.
            Updates.Add(mobile);
        }

        public List<MobileTag> GetTargets()
        {
            return Affected;
        }

        public List<MobileModifier> GetUpdates()
        {
            return Updates;
        }

        public void CleanClientInfo()
        {
            this.Affected = null;
        }

        public byte[] ToByte() { return Network.Serialize(this); }
    }
}
