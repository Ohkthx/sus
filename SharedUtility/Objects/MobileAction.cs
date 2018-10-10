using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SUS.Shared.Utility;

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
        public ActionType Type = ActionType.None;
        public AbilityType Abiltiy = AbilityType.None;
        private UInt64 Initator;
        private List<UInt64> Affected = new List<UInt64>();
        public string Result = string.Empty;
        public bool Fulfilled = false;

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

        public List<UInt64> GetTargets()
        {
            return this.Affected;
        }

        public byte[] ToByte()
        {
            return Utility.Utility.Serialize(this);
        }
    }
}
