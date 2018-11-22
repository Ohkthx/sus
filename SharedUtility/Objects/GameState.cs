using System;
using System.Linq;
using System.Data.SQLite;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.SQLite;
using SUS.Shared.Utilities;
using System.Collections.Generic;

namespace SUS.Shared.Objects
{
    [Serializable]
    public class GameState : ISQLCompatibility
    {
        private static readonly double m_Version = 1.0;
        private Player m_Account = null;
        private Node m_Location = null;
        private Node m_LocationLast = null;
        private int m_Unlocked = (int)Locations.None;
        private HashSet<MobileTag> m_Mobiles;

        #region Constructors
        public GameState(Player account) : this(account, null, (int)Locations.Basic) { }
        public GameState(Player account, int unlocked) : this(account, null, unlocked) { }
        public GameState(Player account, Node location, int unlocked)
        {
            this.Account = account;
            this.NodeCurrent = location;
            this.m_Unlocked |= unlocked;
        }
        #endregion

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

        #region Getters / Setters
        public static double Version { get { return m_Version; } }

        public UInt64 ID { get { return Account.ID.ToInt(); } }

        public Player Account
        {
            get { return m_Account; }
            set
            {
                if (value == null || !value.IsPlayer)
                    return;

                m_Account = value;
            }
        }

        public Node NodeCurrent
        {
            get { return m_Location; }
            set
            {
                if (value == null)
                    return;

                if (NodeCurrent == null)
                    m_Location = value;
                else if (value.ID == NodeCurrent.ID)
                    return;

                NodeLast = NodeCurrent; // Swap the Node.
                m_Location = value;     // Assign the new
            }
        }

        public Node NodeLast
        {
            get { return m_LocationLast; }
            set
            {
                if (value == null)
                    return;

                if (NodeLast == null)
                    m_LocationLast = value;
                else if (value.ID == NodeLast.ID)
                    return;

                m_LocationLast = value;     // Updates our Last Node accessed.
            }
        }

        public HashSet<MobileTag> Mobiles
        {
            get
            {
                if (m_Mobiles == null)
                    m_Mobiles = new HashSet<MobileTag>();

                return m_Mobiles;
            }
            set
            {
                if (value == null)
                    return;

                m_Mobiles = value;
            }
        }
        #endregion

        #region Finding
        /// <summary>
        ///     Find a mobile based on it's type.
        /// </summary>
        /// <param name="type">Type of a mobile.</param>
        /// <param name="serial">Serial of the mobile to find.</param>
        /// <returns></returns>
        private MobileTag FindMobile(MobileType type, Serial serial)
        {   // Iterate our hashset of mobiles.
            foreach (MobileTag m in Mobiles)
                if (m.Type == type && m.ID == serial)
                    return m;   // If the type and serial match, return it. If it is type of 'Any', return it.

            return null;    // Nothing was found, return null.
        }
        #endregion

        #region Mobile Actions
        public bool UpdateMobile(MobileTag mobile, bool remove = false)
        {
            if (mobile == null)
                return false;

            if (remove)
            {
                if (mobile.IsPlayer && mobile.ID == Account.ID)
                {
                    Account.Kill(); // Kill the player, but skip removal.
                    return true;
                }
                else
                    return RemoveMobile(mobile);    // This case will on pass if it is not the player and is flagged for removal.
            }
            else
            {
                return AddMobile(mobile);       // Add the mobile.
            }
        }

        /// <summary>
        ///     Adds either a Player or NPC. Performs and update if the mobile already exists.
        /// </summary>
        /// <param name="mobile">Mobile to be added.</param>
        /// <returns>Succcess (true), or Failure (false)</returns>
        private bool AddMobile(MobileTag mobile)
        {
            if (Mobiles.Count > 0 && Mobiles.Contains(mobile))
                Mobiles.Remove(mobile);

            return Mobiles.Add(mobile); // Add the Mobile to the Node's tracked Mobiles.
        }

        /// <summary>
        ///     Removes the mobile from the correct list (NPCs or Players)
        /// </summary>
        /// <param name="mobile">Mobile to remove.</param>
        /// <returns>Number of elements removed.</returns>
        private bool RemoveMobile(MobileTag mobile)
        {
            if (Mobiles.Count == 0)
                return true;

            return Mobiles.Remove(mobile);
        }

        public bool HasMobile(MobileTag mobile)
        {
            if (Mobiles.Count == 0)
                return false;

            return Mobiles.Contains(mobile);
        }

        public bool HasMobile(MobileType mobileType, UInt64 mobileID)
        {
            if (Mobiles.Count == 0)
                return false;

            foreach (MobileTag m in Mobiles)
                if (m.Type == mobileType && m.ID == mobileID)
                    return true;

            return false;
        }

        /// <summary>
        ///     Updates a Node with a mobile.
        /// </summary>
        /// <param name="toLocation">Node to move the mobile to.</param>
        /// <param name="mobile">Mobile to update.</param>
        public bool MoveMobile(Locations toLocation, Mobile mobile)
        {
            if (mobile != Account)
                return false;

            if (!Node.isValidLocation(toLocation))
                return false;   // It must be a combination of locations.
            else if (toLocation == mobile.Location)
                return false;   // Trying to move within the same location.

            Account.Location = toLocation;
            return true;
        }

        public void MobileActionHandler(MobileAction ma)
        {
            Console.WriteLine($"\n Server Reponse: {ma.Result}");

            foreach (MobileModifier mm in ma.GetUpdates())
            {   // Attempt to update the gamestate with the modifications to the mobile.
                string attr = string.Empty;
                if (mm.ModStrength != 0)
                    attr += $"\n\tStrength: {mm.ModStrength}";
                if (mm.ModDexterity != 0)
                    attr += $"\n\tDexterity: {mm.ModDexterity}";
                if (mm.ModIntelligence != 0)
                    attr += $"\n\tIntelligence: {mm.ModIntelligence}";


                Console.WriteLine($"  => {mm.Name}'s health was changed by {mm.ModHits}. " +
                    $"\n\tStamina was changed by {mm.ModStamina}." +
                    $"{attr}" +
                    $"\n\tDead? {mm.IsDead}");

                if (mm.IsPlayer && (mm.ID == Account.ID))
                    Account.ApplyModification(mm);

                if (mm.IsDead)
                    UpdateMobile(FindMobile(mm.Type, mm.ID), remove: true);
            }
        }

        /// <summary>
        ///     Removes a mobile from the current Gamestate.
        /// </summary>
        /// <param name="mobile">Mobile to remove.</param>
        public void Kill(Mobile mobile)
        {
            mobile.Kill();
            UpdateMobile(mobile.getTag(), remove: true);
        }

        public Request Ressurrect(Ressurrect rez)
        {
            if (rez.Mobile.IsPlayer && rez.Mobile.ID == ID)
            {   // If we are talking about our account...
                if (rez.isSuccessful)
                {   // And the server reported it was successful...
                    Account.Ressurrect();               // Ressurrect our account.
                    MoveMobile(rez.Location, Account);  // Move the Account locally.
                    return new Request(RequestTypes.Node, (int)rez.Location);   // Fetch our new location.
                }
            }

            return null;
        }
        #endregion

        #region Node / Location Actions
        /// <summary>
        ///     Attempts to convert a string (either integer or location name) to a location that has a connection.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Locations StringToLocation(string location)
        {
            int pos = -1;
            if (int.TryParse(location, out pos) && pos <= 0)
                return Locations.None;
            else if (pos > NodeCurrent.ConnectionsCount)
                return Locations.None;

            int count = 0;
            foreach (Locations loc in Enum.GetValues(typeof(Locations)))
            {
                if (loc == Locations.None)          // A connection cannot be 'None'
                    continue;
                else if ((loc & (loc - 1)) != 0)                // Check if this is not a power of two (indicating it is a combination location)
                    continue;                                   //  It was a combination.
                else if (!NodeCurrent.HasConnection(loc))       // Validate if it is not a connection.
                    continue;                                   //  It is not a connection, return

                ++count;
                if (pos > 0)
                {
                    if (count == pos)   // Attempts to check the integer conversion
                    {
                        return loc;     //  if a match is found, return it.
                    }
                }
                else
                {
                    if (NodeCurrent.StringToConnection(Enum.GetName(typeof(Locations), loc)) != Locations.None)
                    {
                        return loc;
                    }
                }
            }

            return Locations.None;
        }
        #endregion

        // Serialize and convert to Byte[] to be sent over a socket.
        public byte[] ToByte() { return Network.Serialize(this); }
    }
}
