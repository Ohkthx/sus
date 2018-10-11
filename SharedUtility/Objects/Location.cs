using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SUS.Shared.Utility;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.Objects;

namespace SUS.Shared.Objects
{
    [Flags, Serializable]
    public enum Types
    {
        Town = 1,
        Dungeon = 2,
        OpenWorld = 4,
        PvP = 8
    };

    [Flags, Serializable]
    public enum Locations
    {
        None          = 0x00000000,
        Moongate      = 0x00000001,

        Unused1       = 0x00000002,

        Britain       = 0x00000004,
        BuccaneersDen = 0x00000008,
        Cove          = 0x00000010,
        Minoc         = 0x00000020,
        SkaraBrae     = 0x00000040,
        Trinsic       = 0x00000080,
        Vesper        = 0x00000100,
        Yew           = 0x00000200,

        Unused2       = 0x00000400,

        Destard       = 0x00000800,
        Despise       = 0x00001000,
        Covetous      = 0x00002000,
        Shame         = 0x00004000,
        Wind          = 0x00008000,
        Wrong         = 0x00010000,

        Unused3       = 0x00020000,
        Unused4       = 0x00040000,

        SolenHive     = 0x00080000,
        OrcCaves      = 0x00100000,

        Unused5       = 0x00200000,
        Unused6       = 0x00400000,
        Unused7       = 0x00800000,

        Graveyard     = 0x01000000,
        Sewers        = 0x02000000,
        Swamp         = 0x04000000,
        Wilderness    = 0x08000000,

        Unused8       = 0x10000000,
        Unused9       = 0x20000000,
        Unused10      = 0x40000000,

        Basic         = Britain | Graveyard | Sewers | Wilderness
    }

    [Serializable]
    public class Node
    {
        public int ID;
        public string Name = string.Empty;
        public string Description = string.Empty;

        public HashSet<Node> Connections = new HashSet<Node>();
        public HashSet<Player> Players = new HashSet<Player>();
        public HashSet<NPC> NPCs = new HashSet<NPC>();

        protected Types Type;
        protected Locations Location;

        #region Constructors
        public Node(Types type, Locations location, string description)
        {
            this.Location = location;
            this.ID = (int)location;
            this.Name = Enum.GetName(typeof(Locations), location);
            this.Type = type;
            this.Description = description;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            Node node = obj as Node;
            return this.ID == node.ID && (int)this.Location == (int)node.Location && (int)this.Type == (int)node.Type;
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash += this.ID.GetHashCode();
            hash *= 397;
            hash += this.Location.GetHashCode();
            hash *= 397;
            hash += this.Type.GetHashCode();
            return hash *= 397;
        }
        #endregion

        public byte[] ToByte()
        {
            return Utility.Utility.Serialize(this);
        }

        public bool AddMobile(Mobile mobile)
        {
            if (mobile is Player)
                return this.Players.Add((Player)mobile);
            else 
                return this.NPCs.Add((NPC)mobile);
        }

        /// <summary>
        ///     Removes the mobile from the correct list (NPCs or Players)
        /// </summary>
        /// <param name="mobile">Mobile to remove.</param>
        /// <returns>Location of the Mobile in the list.</returns>
        public int RemoveMobile(Mobile mobile)
        {
            if (mobile is Player)
                return this.Players.RemoveWhere(p => p.m_ID == mobile.m_ID);
            else
                return this.NPCs.RemoveWhere(p => p.m_ID == mobile.m_ID);
        }

        public bool UpdateMobile(Mobile mobile)
        {   // Remove the mobile, if there is none that are removed- return early, else just readd the new.
            if (RemoveMobile(mobile) <= 0)
                return false;
            return AddMobile(mobile);
        }

        public void AddConnection(ref Node node)
        {
            node.Connections.Add(this);
            Connections.Add(node);
        }

        /// <summary>
        ///     Retrieves the internal Location.
        /// </summary>
        /// <returns></returns>
        public Locations GetLocation()
        {
            return this.Location;
        }

        // Will null out unimportant lists for transferring to server to reduce bandwidth.
        public void Clean()
        {
            this.Players = null;
            this.NPCs = null;
        }
    }
}
