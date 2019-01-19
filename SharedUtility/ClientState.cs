using SUS.Shared.Packets;
using System;
using System.Collections.Generic;

namespace SUS.Shared
{
    [Serializable]
    public class ClientState
    {
        private ulong m_PlayerId;
        private BaseMobile m_Account;
        private BaseRegion m_Region;
        private BaseRegion m_LastRegion;
        private BaseMobile m_LastTarget;
        private bool m_IsAlive;
        private readonly Regions m_Unlocked = Regions.None;

        // Objects that need to be requested from the server.
        private HashSet<BaseMobile> m_Mobiles;       // Local / Nearby creatures.
        private Dictionary<int, string> m_Items;     // Items in the inventory.
        private Dictionary<int, string> m_Equipped;  // Equipped items.

        #region Constructors
        public ClientState(ulong playerID, BaseMobile account, BaseRegion region, Regions unlocked)
        {
            PlayerId = playerID;
            Account = account;
            Region = region;
            m_Unlocked |= unlocked;
        }
        #endregion

        #region Getters / Setters
        public ulong PlayerId
        {
            get => m_PlayerId;
            private set => m_PlayerId = value;
        }

        public BaseMobile Account
        {
            get => m_Account;
            private set
            {
                if (!value.IsPlayer)
                {
                    return;
                }

                m_Account = value;
            }
        }

        public BaseRegion Region
        {
            get => m_Region;
            set
            {
                if (!value.IsValid || value.Location == Region.Location)
                {
                    return;
                }

                LastRegion = Region; // Swap the Node.
                m_Region = value;     // Assign the new
            }
        }

        public BaseRegion LastRegion
        {
            get => m_LastRegion;
            private set
            {
                if (!value.IsValid || value.Location == LastRegion.Location)
                {
                    return;
                }

                m_LastRegion = value;     // Updates our Last Node accessed.
            }
        }

        public BaseMobile LastTarget
        {
            get => m_LastTarget;
            set => m_LastTarget = value;
        }

        public HashSet<BaseMobile> Mobiles
        {
            get => m_Mobiles ?? (m_Mobiles = new HashSet<BaseMobile>());
            set
            {
                if (value == null)
                {
                    return;
                }

                m_Mobiles = value;
            }
        }

        public bool IsAlive
        {
            get => m_IsAlive;
            private set => m_IsAlive = value;
        }
        #endregion

        #region Mobile Actions
        public void MobileActionHandler(CombatMobilePacket cmp)
        {
            if (!cmp.IsAlive)
            {
                IsAlive = false;
            }

            List<string> u = cmp.Updates;
            Console.WriteLine("\nServer response:");
            if (u == null)
            {
                Utility.ConsoleNotify("Server sent back a bad combat log.");
                return;
            }

            string fn = @"C:\Users\d0x1p2\Desktop\combat.txt";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fn, true))
            {
                file.WriteLine();
                file.WriteLine($"[{DateTime.Now}]");
                foreach (string str in u)
                {
                    file.WriteLine(str);
                }
            }

            foreach (string str in u)
            {
                Console.WriteLine(str);
            }
        }

        public Packet Resurrect(ResurrectMobilePacket rez)
        {
            if (rez.PlayerId != PlayerId) return null; // If we are talking about our account...

            IsAlive = true;
            return rez.IsSuccessful ? new GetNodePacket(rez.Region, PlayerId) : null;
        }

        public static void UseItemResponse(UseItemPacket uip)
        {
            Console.WriteLine(uip.Response);
        }

        public Packet UseItems()
        {
            if (m_Items == null)
            {
                return new GetMobilePacket(GetMobilePacket.RequestReason.Items, PlayerId);
            }

            var pos = 0;
            foreach (var i in m_Items.Values)
            {
                ++pos;
                Console.WriteLine($" [{pos}] {i}");
            }

            Console.WriteLine();

            int opt;
            string input;
            do
            {
                Console.Write(" Selection: ");
                input = Console.ReadLine();
            } while (int.TryParse(input, out opt) && (opt < 1 || opt > m_Items.Count));

            pos = 0;
            foreach (var i in m_Items.Keys)
            {
                ++pos;
                if (pos == opt)
                {
                    return new UseItemPacket(i, PlayerId);
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
        public Regions StringToLocation(string location)
        {
            if (int.TryParse(location, out var pos) && pos < 0)
            {
                return Regions.None;                      // User attempted a negative number.
            }

            if (pos == 0)
            {
                return Region.Location;
            }

            var count = 0;
            foreach (Regions loc in Utility.EnumToIEnumerable<Regions>(Region.Connections, true))
            {
                if (loc == Regions.None)          // A connection cannot be 'None'
                {
                    continue;
                }

                if ((loc & (loc - 1)) != 0)    // Check if this is not a power of two (indicating it is a combination location)
                {
                    continue;                       //  It was a combination.
                }

                ++count;
                if (count == pos)   // Attempts to check the integer conversion
                {
                    return loc;     //  if a match is found, return it.
                }
            }

            return Regions.None;
        }

        public MobileDirections StringToDirection(string location)
        {
            if (!Region.Navigable)
            {
                return MobileDirections.None;
            }

            foreach (MobileDirections dir in Enum.GetValues(typeof(MobileDirections)))
            {
                if (dir == MobileDirections.None)
                {
                    continue;
                }

                if (Enum.GetName(typeof(MobileDirections), dir)?.ToLower() == location.ToLower())
                {
                    return dir;
                }
            }

            return MobileDirections.None;
        }
        #endregion

        #region Packet Parsing
        public void ParseGetMobilePacket(GetMobilePacket gmp)
        {
            GetMobilePacket.RequestReason reason = gmp.Reason;

            Console.WriteLine();
            while (reason != GetMobilePacket.RequestReason.None)
            {
                foreach (GetMobilePacket.RequestReason r in Enum.GetValues(typeof(GetMobilePacket.RequestReason)))
                {
                    if (r == GetMobilePacket.RequestReason.None || (r & (r - 1)) != 0)
                    {
                        continue;
                    }

                    switch (reason & r)
                    {
                        case GetMobilePacket.RequestReason.Paperdoll:
                            Console.WriteLine("Paper Doll Information:");
                            Console.WriteLine(gmp.Paperdoll);
                            break;
                        case GetMobilePacket.RequestReason.Location:
                            Console.WriteLine("Location Information:");
                            Console.WriteLine(gmp.Region.ToString());
                            break;
                        case GetMobilePacket.RequestReason.IsDead:
                            Console.WriteLine($"Is Alive?: {gmp.IsAlive.ToString()}.");
                            break;
                        case GetMobilePacket.RequestReason.Items:
                            Console.WriteLine("Received updated items.");
                            m_Items = gmp.Items;
                            break;
                        case GetMobilePacket.RequestReason.Equipment:
                            Console.WriteLine("Received updated equipment.");
                            m_Equipped = gmp.Equipment;
                            break;
                    }

                    reason &= ~(r);
                }
            }
        }
        #endregion
    }
}
