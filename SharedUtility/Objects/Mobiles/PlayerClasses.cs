using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUS.Shared.Objects.Mobiles
{
    [Serializable]
    public class PArcher : Player
    {
        public PArcher(ulong id, string name) : base(id, name, 60, 40, 50, 10)
        {
            this.m_Skills[(int)Skill.Types.Archery].Value = 50.0;
        }
    }

    [Serializable]
    public class PMelee : Player
    {
        public PMelee(ulong id, string name) : base(id, name, 60, 50, 40, 10)
        {
            this.m_Skills[(int)Skill.Types.Fencing].Value = 50.0;
        }
    }

    [Serializable]
    public class PMage : Player
    {
        public PMage(ulong id, string name) : base(id, name, 40, 10, 50)
        {
            this.m_Skills[(int)Skill.Types.Magery].Value = 50.0;
        }
    }
}
