﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.SQLite;
using SUS.Shared.Utility;
using System.Data.SQLite;


namespace SUS.Shared.Objects
{
    [Serializable]
    public class GameState : ISQLCompatibility
    {
        public double _version = 1.0;
        public Player Account { get; private set; } = null;
        public Node Location { get; set; } = null;
        public Node LocationLast { get; set; } = null;
        public int Unlocked = (int)Locations.None;
        public bool moved { get; set; } = false;

        #region Constructors
        public GameState(Player account) : this(account, null, (int)Locations.Basic) { }
        public GameState(Player account, int unlocked) : this(account, null, unlocked) { }
        public GameState(Player account, Node location, int unlocked)
        {
            this.Account = account;
            this.Location = location;
            this.Unlocked |= unlocked;
        }
        #endregion

        // Serialize and convert to Byte[] to be sent over a socket.
        public byte[] ToByte()
        {
            // Clean our structure for transportation.
            if (this.LocationLast != null)
                this.LocationLast.Clean();
            //this.Location.Clean();

            // Serialize and convert to bytes for transport.
            return Utility.Utility.Serialize(this);
        }

        #region Overrides
        public void ToInsert(ref SQLiteCommand cmd)
        {
            cmd.Parameters.Add(new SQLiteParameter("@p1", this.Account.m_ID));
            cmd.Parameters.Add(new SQLiteParameter("@p2", this.ToByte()));
        }
        #endregion

        public ulong ID()
        {
            return Account.m_ID;
        }

        public Player GetPlayer()
        {
            return Account;
        }

        #region User Actions
        public bool MoveTo(string location)
        {
            // This can take an integer or a name to move too.

            // Try to get our int.
            int pos;
            if (!int.TryParse(location, out pos))
            {
                // Couldn't parse integer, try to get by name.
                foreach (Node node in this.Location.Connections)
                {   
                    if (node.Name.ToLower().Contains(location.ToLower()))
                    {
                        // Found the right place..
                        this.LocationLast = this.Location;
                        this.LocationLast.Clean();
                        this.Location = node;
                        this.moved = true;
                        return true;
                    }
                }

                // Couldn't find location...
                return false;
            }

            // Bad location, we don't accept negative numbers or numbers out of range.
            if (pos <= 0 || pos > this.Location.Connections.Count())
                return false;

            var nodes = this.Location.Connections.ToList();
            this.LocationLast = this.Location;
            this.LocationLast.Clean();
            this.Location = nodes[pos-1];
            this.moved = true;

            return true;
        }
        #endregion

    }
}