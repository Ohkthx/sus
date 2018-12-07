using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.SQLite;
using SUS.Shared.Utilities;
using SUS.Shared.Packets;


namespace SUS.Shared.Objects
{
    [Serializable]
    public class GameState : ISQLCompatibility
    {
        private static readonly double m_Version = 1.0;
        private BasicMobile m_Account = null;
        private BasicNode m_Location = null;
        private BasicNode m_LocationLast = null;
        private bool m_IsDead;
        private Locations m_Unlocked = Locations.None;

        // Objects that need to be requested from the server.
        private HashSet<BasicMobile> m_Mobiles;                   // Local / Nearby creatures.
        private Dictionary<Guid, Item> m_Items;                          // Items in the inventory.
        private Dictionary<ItemLayers, Equippable> m_Equipped;  // Equipped items.

        #region Constructors
        public GameState(BasicMobile account) : this(account, null, Locations.Basic) { }
        public GameState(BasicMobile account, Locations unlocked) : this(account, null, unlocked) { }
        public GameState(BasicMobile account, BasicNode location, Locations unlocked)
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

        public BasicMobile Account
        {
            get { return m_Account; }
            set
            {
                if (value == null || !value.IsPlayer)
                    return;

                m_Account = value;
            }
        }

        public BasicNode NodeCurrent
        {
            get { return m_Location; }
            set
            {
                if (value == null)
                    return;

                if (NodeCurrent == null)
                {
                    m_Location = value;
                    return;
                }
                else if (!value.IsValid || value.Location == NodeCurrent.Location)
                    return;

                NodeLast = NodeCurrent; // Swap the Node.
                m_Location = value;     // Assign the new
            }
        }

        public BasicNode NodeLast
        {
            get { return m_LocationLast; }
            set
            {
                if (value == null)
                    return;

                if (NodeLast == null)
                {
                    m_LocationLast = value;
                    return;
                }
                else if (!value.IsValid || value.Location == NodeLast.Location)
                    return;

                m_LocationLast = value;     // Updates our Last Node accessed.
            }
        }

        public HashSet<BasicMobile> Mobiles
        {
            get
            {
                if (m_Mobiles == null)
                    m_Mobiles = new HashSet<BasicMobile>();

                return m_Mobiles;
            }
            set
            {
                if (value == null)
                    return;

                m_Mobiles = value;
            }
        }

        public bool IsDead
        {
            get { return m_IsDead; }
            set
            {
                if (value != IsDead)
                    m_IsDead = value;
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
        private BasicMobile FindMobile(Mobile.Types type, Serial serial)
        {   // Iterate our hashset of mobiles.
            foreach (BasicMobile m in Mobiles)
                if (m.Type == type && m.ID == serial)
                    return m;   // If the type and serial match, return it. If it is type of 'Any', return it.

            return null;    // Nothing was found, return null.
        }
        #endregion

        #region Mobile Actions
        public void MobileActionHandler(CombatMobilePacket cmp)
        {
            List<string> u = cmp.GetUpdates();
            Console.WriteLine("\nServer response:");
            if (u == null)
            {
                Utility.ConsoleNotify("Server sent back a bad combat log.");
                return;
            }

            foreach (string str in u)
                Console.WriteLine(str);
        }

        public Packet Ressurrect(RessurrectMobilePacket rez)
        {
            if (rez.Author.IsPlayer && rez.Author.ID == ID)
            {   // If we are talking about our account...
                if (rez.isSuccessful)
                {   // And the server reported it was successful...
                    return new MoveMobilePacket(rez.Location, Account);
                }
            }

            return null;
        }

        public void UseItemResponse(UseItemPacket uip)
        {
            Console.WriteLine(uip.Response);
        }

        public Packet UseItems()
        {
            if (m_Items == null)
                return new GetMobilePacket(Account, GetMobilePacket.RequestReason.Items);

            int pos = 0;
            foreach (KeyValuePair<Guid, Item> i in m_Items)
            {
                ++pos;
                Console.WriteLine($" [{pos}] {i.Value.Name}");
            }

            Console.WriteLine();

            int opt;
            string input;
            do
            {
                Console.Write(" Selection: ");
                input = Console.ReadLine();
            } while (int.TryParse(input, out opt) && (opt < 1 || opt > m_Items.Count));

            Item item = null;
            pos = 0;
            foreach (KeyValuePair<Guid, Item> i in m_Items)
            {
                ++pos;
                if (pos == opt)
                {
                    item = i.Value;
                    break;
                }
            }

            if (item == null)
            {
                Console.WriteLine("Bad value.");
                return null;
            }

            return new UseItemPacket(Account, item.Type, item.Guid);
        }

        public Packet Heal()
        {
            if (m_Items == null)
                return new GetMobilePacket(Account, GetMobilePacket.RequestReason.Items);

            Potion p = null;
            foreach (KeyValuePair<Guid, Item> i in m_Items)
                if (i.Value.Type == ItemTypes.Consumable
                    && (i.Value as Consumable).ConsumableType == Consumable.ConsumableTypes.HealthPotion)
                    p = i.Value as Potion;

            if (p == null)
            {
                Utility.ConsoleNotify("You do not have any potions in your inventory. Request an updated inventory.");
                return null;
            }

            return new UseItemPacket(Account, ItemTypes.Consumable, p.Guid);
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
            if (int.TryParse(location, out pos) && pos < 0)
                return Locations.None;                      // User attempted a negative number.
            else if (pos == 0)
                return NodeCurrent.Location;

            int count = 0;
            foreach (Locations loc in NodeCurrent.ConnectionsToList())
            {
                if (loc == Locations.None)          // A connection cannot be 'None'
                    continue;
                else if ((loc & (loc - 1)) != 0)    // Check if this is not a power of two (indicating it is a combination location)
                    continue;                       //  It was a combination.

                ++count;
                if (count == pos)   // Attempts to check the integer conversion
                {
                    return loc;     //  if a match is found, return it.
                }
            }

            return Locations.None;
        }

        public Mobile.Directions StringToDirection(string location)
        {
            if (!NodeCurrent.CanTraverse)
                return Mobile.Directions.None;

            foreach (Mobile.Directions dir in Enum.GetValues(typeof(Mobile.Directions)))
            {
                if (dir == Mobile.Directions.None) 
                    continue;
                else if (Enum.GetName(typeof(Mobile.Directions), dir).ToLower() == location.ToLower())
                    return dir;
            }

            return Mobile.Directions.None;
        }
        #endregion

        #region Packet Parsing
        public void ParseGetMobilePacket(Packet p)
        {
            GetMobilePacket gmp = p as GetMobilePacket;
            if (gmp == null)
                return;

            GetMobilePacket.RequestReason reason = gmp.Reason;

            Console.WriteLine();
            while (reason != GetMobilePacket.RequestReason.None)
            {
                foreach (GetMobilePacket.RequestReason r in Enum.GetValues(typeof(GetMobilePacket.RequestReason)))
                {
                    if (r == GetMobilePacket.RequestReason.None || (r & (r - 1)) != 0)
                        continue;

                    switch (reason & r)
                    {
                        case GetMobilePacket.RequestReason.Paperdoll:
                            Console.WriteLine("Paper Doll Information:");
                            Console.WriteLine(gmp.Paperdoll);
                            break;
                        case GetMobilePacket.RequestReason.Location:
                            Console.WriteLine("Location Information:");
                            Console.WriteLine(gmp.Location.ToString());
                            break;
                        case GetMobilePacket.RequestReason.IsDead:
                            Console.WriteLine("Is Dead?");
                            Console.WriteLine(gmp.IsDead.ToString());
                            if (gmp.Target == Account)
                                IsDead = gmp.IsDead;
                            break;
                        case GetMobilePacket.RequestReason.Items:
                            Console.WriteLine("Received updated items.");
                            if (gmp.Target == Account)
                                m_Items = gmp.Items;
                            break;
                        case GetMobilePacket.RequestReason.Equipment:
                            Console.WriteLine("Received updated equipment.");
                            if (gmp.Target == Account)
                                m_Equipped = gmp.Equipment;
                            break;
                    }

                    reason &= ~(r);
                }
            }
        }
        #endregion

        public byte[] ToByte()
        {
            return Network.Serialize(this);
        }
    }
}
