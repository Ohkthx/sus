﻿using System;
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
        private List<Tuple<MobileType, UInt64>> Affected;   // List of Targets
        private List<Mobile> Updates = new List<Mobile>();  // TODO: Think of a way to reduce overhead of sending Mobile Objects.
        public string Result = string.Empty;

        public MobileAction(UInt64 initator)
        {
            this.Initator = initator;
            this.Affected = new List<Tuple<MobileType, ulong>>();   // Initialize our Affected.
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

            this.Affected.Add(new Tuple<MobileType, ulong>(type, targetId));
        }

        public void AddUpdate(Mobile mobile)
        {
            if (this.Updates.Contains(mobile))
                return;
            this.Updates.Add(mobile);
        }

        public List<Tuple<MobileType, UInt64>> GetTargets()
        {
            return this.Affected;
        }

        public List<Mobile> GetUpdates()
        {
            return this.Updates;
        }

        public byte[] ToByte()
        {
            return Utility.Network.Serialize(this);
        }
    }
}
