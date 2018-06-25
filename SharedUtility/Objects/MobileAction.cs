using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public UInt64 Initator;
        public List<UInt64> Affected = new List<UInt64>();

        public MobileAction(UInt64 initator)
        {
            this.Initator = initator;
            this.Affected.Add(Initator);
        }

        public byte[] ToByte()
        {
            return Utility.Utility.Serialize(this);
        }
    }
}
