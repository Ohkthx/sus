using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SUS.Shared.Utility;
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
        private List<UInt64> Affected = new List<UInt64>(); // List of Targets
        private List<Mobile> Updates = new List<Mobile>();  // TODO: Think of a way to reduce overhead of sending Mobile Objects.
        public string Result = string.Empty;

        public MobileAction(UInt64 initator)
        {
            this.Initator = initator;
        }

        public Serial GetInitator()
        {
            return new Serial(this.Initator);
        }

        public void AddTarget(UInt64 targetId)
        {
            if (this.Affected.Contains(targetId))
                return;
            this.Affected.Add(targetId);
        }

        public void AddUpdate(Mobile mobile)
        {
            if (this.Updates.Contains(mobile))
                return;
            this.Updates.Add(mobile);
        }

        public List<UInt64> GetTargets()
        {
            return this.Affected;
        }

        public List<Mobile> GetUpdates()
        {
            return this.Updates;
        }

        public byte[] ToByte()
        {
            return Utility.Utility.Serialize(this);
        }
    }
}
