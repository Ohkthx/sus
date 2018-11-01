using System;
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

        public UInt64 ID()
        {
            return Account.m_ID.ToInt();
        }

        public Player GetPlayer()
        {
            return Account;
        }

        /// <summary>
        ///     Refreshes the account for the gamestate.
        /// </summary>
        /// <param name="player">New mobile</param>
        /// <returns>True - Success, False - Failure</returns>
        public bool Refresh(Mobile player)
        {
            if (player.m_Name == this.Account.m_Name && player.m_ID == this.Account.m_ID)
            {
                this.Account = player as Player;
                return true;
            }
            return false;
        }

        #region User Actions
        public bool MoveTo(string location)
        {   // This can take an integer or a name to move too.

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
                        SwapLocation(this.Location, node);
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
            SwapLocation(this.Location, nodes[pos - 1]);

            return true;
        }

        private void SwapLocation(Node oldLocation, Node newLocation)
        {
            this.LocationLast = oldLocation;
            this.LocationLast.Clean();

            this.Location = newLocation;
            this.moved = true;
        }

        public bool UpdateMobileLocation(Mobile mobile)
        {   // Attempt updating our current location, if it fails process all connected locations.
            if (this.Location.UpdateMobile(mobile))
                return true;

            for (int i = 0; i < this.Location.Connections.Count; i++)
                if (this.Location.Connections.ElementAt(i).UpdateMobile(mobile))
                    return true;

            // Location + Mobile combination was never found, return false.
            return false;
        }
        #endregion

        /// <summary>
        ///     Removes a mobile from the current Gamestate.
        /// </summary>
        /// <param name="mobile">Mobile to remove.</param>
        public void Kill(Mobile mobile)
        {
            if (this.Location.RemoveMobile(mobile) <= 0)    // Attempts to remove from the current location, if it wasn't found/removed...
                this.LocationLast.RemoveMobile(mobile);     //  it then attempts to remove from the last location.
        }
    }
}
