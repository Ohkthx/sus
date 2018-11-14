using System;
using System.Linq;
using System.Data.SQLite;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.SQLite;
using SUS.Shared.Utilities;

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

            // Serialize and convert to bytes for transport.
            return Network.Serialize(this);
        }

        #region Overrides
        public void ToInsert(ref SQLiteCommand cmd)
        {
            cmd.Parameters.Add(new SQLiteParameter("@p1", this.Account.ID));
            cmd.Parameters.Add(new SQLiteParameter("@p2", this.ToByte()));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, Account.ID) ? Account.ID.GetHashCode() : 0);
                hash = (hash * 7) + (!Object.ReferenceEquals(null, Account.Type) ? Account.Type.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(GameState gs1, GameState gs2)
        {
            if (Object.ReferenceEquals(gs1, gs2)) return true;
            if (Object.ReferenceEquals(null, gs1)) return false;
            return (gs1.Equals(gs2));
        }

        public static bool operator !=(GameState gs1, GameState gs2)
        {
            return !(gs1 == gs2);
        }

        public override bool Equals(object value)
        {
            if (Object.ReferenceEquals(null, value)) return false;
            if (Object.ReferenceEquals(this, value)) return true;
            if (value.GetType() != this.GetType()) return false;
            return IsEqual((GameState)value);
        }

        public bool Equals(GameState gamestate)
        {
            if (Object.ReferenceEquals(null, gamestate)) return false;
            if (Object.ReferenceEquals(this, gamestate)) return true;
            return IsEqual(gamestate);
        }

        private bool IsEqual(GameState value)
        {
            return (value != null)
                && (value.Account != null)
                && (Account.Type == value.Account.Type)
                && (Account.ID == value.Account.ID);
        }
        #endregion

        public UInt64 ID()
        {
            return Account.ID.ToInt();
        }

        public Player GetPlayer()
        {
            return Account;
        }

        #region Client-Side Updating
        /// <summary>
        ///     Refreshes the account for the gamestate.
        /// </summary>
        /// <param name="player">New mobile</param>
        /// <returns>True - Success, False - Failure</returns>
        public bool Refresh(Mobile player)
        {
            if (player.Name == this.Account.Name && player.ID == this.Account.ID)
            {
                this.Account = player as Player;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Updates a mobile with the current provided mobile.
        /// </summary>
        /// <param name="mobile">Mobile to update.</param>
        /// <returns></returns>
        public bool UpdateMobile(Mobile mobile)
        {
            if (this.Account.ID == mobile.ID && mobile.IsPlayer())
                this.Account = mobile as Player;    // Updates the Gamestate's account.

            if (updateNodesMobile(this.Location, mobile))
                return true;

            return updateNodesMobile(this.LocationLast, mobile);
        }

        /// <summary>
        ///     Takes a MobileModifier and applies the changes to the currently tracked mobiles.
        /// </summary>
        /// <param name="moddedMobile">Modifications to apply the local mobile.</param>
        public bool UpdateMobile(MobileModifier moddedMobile, out Mobile mobile)
        {
            mobile = updateNodesMobile(Location, moddedMobile);
            if (mobile != null)
                return true;    // Mobile was found and updated, return true.

            mobile = updateNodesMobile(LocationLast, moddedMobile);
            if (mobile != null)
                return true;    // Mobile was found and updated, return true.

            return false;       // Mobile was never found, returning false and null.
        }

        /// <summary>
        ///     Attempts to find the mobile located in the node, and process the changes.
        /// </summary>
        /// <param name="node">Node to be searched.</param>
        /// <param name="mobile">Mobile to be update.</param>
        /// <returns>Newly updated mobile.</returns>
        private bool updateNodesMobile(Node node, Mobile mobile)
        {
            foreach (Mobile m in node.Mobiles)
            {   // Iterate each of our locale mobiles seeing if any match by type and serial.
                if (m.ID == mobile.ID && m.Type == mobile.Type)
                {   // We found a match, process changes.
                    if (mobile.IsDead())
                    {   // Server said the mobile is dead, so we kill it.
                        m.Kill();            // Kill the mobile.
                        if (!m.IsPlayer())
                            this.Kill(m);        // Remove the mobile from the GameState (if it is not a player.)
                    }
                    else
                        node.AddMobile(m);  // Update the mobile in the node.

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Attempts to find the mobile located in the node, and process the changes.
        /// </summary>
        /// <param name="node">Node to be searched.</param>
        /// <param name="moddedMobile">Modifications to the mobile to be made.</param>
        /// <returns>Newly updated mobile.</returns>
        private Mobile updateNodesMobile(Node node, MobileModifier moddedMobile)
        {
            foreach (Mobile m in node.Mobiles)
            {   // Iterate each of our locale mobiles seeing if any match by type and serial.
                if (m.ID == moddedMobile.ID && m.Type == moddedMobile.Type)
                {   // We found a match, process changes.
                    if (moddedMobile.IsDead)
                    {   // Server said the mobile is dead, so we kill it.
                        m.Kill();            // Kill the mobile.
                        if (!m.IsPlayer())
                            this.Kill(m);        // Remove the mobile from the GameState (if it is not a player.)
                    }
                    else
                    {
                        m.TakeDamage(moddedMobile.ModHits * -1);    // The mobile takes the damage provided.
                        node.AddMobile(m);  // Update the mobile in the node.
                    }

                    return m;    // Mobile was found and updated. Return it.
                }
            }

            return null;   // We never found our mobile.
        }

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

        /// <summary>
        ///     Swaps the locations between a new and old location.
        /// </summary>
        /// <param name="oldLocation">Location to be the last location.</param>
        /// <param name="newLocation">Location to be the current location.</param>
        private void SwapLocation(Node oldLocation, Node newLocation)
        {
            this.LocationLast = oldLocation;
            this.LocationLast.Clean();

            this.Location = newLocation;
            this.Account.Location = Location.GetLocation();
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
            if (this.Location.RemoveMobile(mobile) == false)    // Attempts to remove from the current location, if it wasn't found/removed...
                this.LocationLast.RemoveMobile(mobile);         //  it then attempts to remove from the last location.
        }

        public void Ressurrect(Ressurrect rez)
        {
            Location = rez.Node;    // Change our current location to this location.
            LocationLast = null;    // Blank out the last spot.

            Location.AddMobile(rez.Mobile); // Readd the mobile to the node.
            Account = rez.Mobile as Player; // Reassign the Gamestate's Account to that which is provided.
        }
    }
}
