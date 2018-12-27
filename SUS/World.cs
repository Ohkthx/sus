using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SUS.Shared;
using SUS.Objects;
using SUS.Map;
using System.Threading;

namespace SUS
{
    public static class World
    {
        #region Dictionaries and Variables.
        // Discord ID => GameState
        private static ConcurrentDictionary<UInt64, Gamestate> m_Gamestates = new ConcurrentDictionary<UInt64, Gamestate>();

        // Location.Type => Node
        private static ConcurrentDictionary<Regions, Node> m_Nodes = new ConcurrentDictionary<Regions, Node>();

        // Contains all currently active/alive mobiles.
        private static ConcurrentDictionary<Serial, Mobile> m_Mobiles = new ConcurrentDictionary<Serial, Mobile>();

        // Contains all items.
        private static ConcurrentDictionary<Serial, Item> m_Items = new ConcurrentDictionary<Serial, Item>();

        // Player ID => Client Socket
        private static ConcurrentDictionary<ulong, SocketHandler> m_Clients = new ConcurrentDictionary<ulong, SocketHandler>();
        #endregion

        #region Getters / Setters
        public static Regions StartingZone
        {
            get { return GetStartingZone().Region; }
        }
        #endregion

        /// <summary>
        ///     Starts our serials and creates our map. To be called first to initiate things.
        /// </summary>
        public static void Initiate()
        {
            // Start our serials.
            //Serial s = new Serial(0);

            // Create our map if there are no nodes yet.
            if (m_Nodes.Count() == 0)
                CreateMap();
        }

        #region Map Data
        /// <summary>
        ///     Creates the map, links the nodes, adds it to GameObject.
        /// </summary>
        public static void CreateMap()
        {   // Make sure we do not accidental recreate nodes...
            if (m_Nodes.Count() != 0)
                return;

            // Create some basic nodes.
            Node Britain = new Britain();
            Sewers Sewer = new Sewers();
            Node Wilderness = new Wilderness();
            Node Graveyard = new Graveyard();

            // Add pathing / connections to each. TODO: Move this to the classes.
            Britain.AddConnection(Regions.Sewers | Regions.Graveyard | Regions.Wilderness);
            Sewer.AddConnection(Regions.Britain);
            Wilderness.AddConnection(Regions.Britain | Regions.Graveyard);
            Graveyard.AddConnection(Regions.Britain | Regions.Wilderness);
            

            // Add the nodes to be held by the GameObject.
            AddNode(Britain);
            AddNode(Sewer);
            AddNode(Wilderness);
            AddNode(Graveyard);
        }

        /// <summary>
        ///     Gets the current starting zone for new players.
        /// </summary>
        /// <returns>A Node of the starting zone.</returns>
        private static Node GetStartingZone()
        {   // If the nodes have not been added, create the map first.
            if (m_Nodes.Count() == 0)
                CreateMap();

            // Returns the designated starting Node.
            return m_Nodes[Regions.Britain];
        }
        #endregion

        #region Gamestate Actions
        public static void AddGamestate(Gamestate gs)
        {
            if (gs == null)
                return;

            m_Gamestates[gs.PlayerID] = gs;
        }

        public static void RemoveGamestate(Gamestate gs)
        {
            if (gs == null)
                return;

            m_Gamestates.TryRemove(gs.PlayerID, out _);
        }

        /// <summary>
        ///     GameState to locate in GameObject.
        /// </summary>
        /// <param name="serial">ID of the GameState.</param>
        /// <returns>GameState provided by GameObject.</returns>
        public static Gamestate FindGamestate(UInt64 discordID)
        {
            Gamestate gs;   // Blank Gamestate.
            if (!m_Gamestates.TryGetValue(discordID, out gs))
                return null;    // Could not find the GameState, return null.
            return gs;          // Return the found GameState.
        }
        #endregion

        #region Items
        public static void AddItem(Item i)
        {
            if (i == null)
                return;

            m_Items[i.Serial] = i;
        }

        public static void RemoveItem(Item i)
        {
            if (i == null)
                return;

            m_Items.TryRemove(i.Serial, out _);
        }

        public static Item FindItem(Serial s)
        {
            if (m_Mobiles.ContainsKey(s))
            {
                Item i = null;
                if (m_Items.TryGetValue(s, out i))
                    return i;
            }

            return null;
        }
        #endregion

        #region Mobile Actions
        public static void AddMobile(Mobile m)
        {
            if (m == null)
                return;

            m_Mobiles[m.Serial] = m;
        }

        public static void RemoveMobile(Mobile m)
        {
            if (m == null)
                return;

            m_Mobiles.TryRemove(m.Serial, out _);
        }

        public static Mobile FindMobile(Serial s)
        {
            if (m_Mobiles.ContainsKey(s))
            {
                Mobile m = null;
                if (m_Mobiles.TryGetValue(s, out m))
                    return m;
            }

            return null;
        }

        /// <summary>
        ///     Find a mobile based on it's type.
        /// </summary>
        /// <param name="type">Type of a mobile.</param>
        /// <param name="serial">Serial of the mobile to find.</param>
        /// <returns></returns>
        public static Mobile FindMobile(MobileTypes type, Serial serial)
        {   // Iterate our hashset of mobiles.
            foreach (Mobile m in m_Mobiles.Values)
                if (((m.Type == type) && (m.Serial == serial))
                    || type == MobileTypes.Mobile)
                {
                    return m;   // If the type and serial match, return it. If it is type of 'Any', return it.
                }

            return null;    // Nothing was found, return null.
        }

        /// <summary>
        ///     Gets all of the Mobile Tags for a specific location of a specific type.
        /// </summary>
        /// <param name="region">Location to search.</param>
        /// <param name="type">Type of mobile."</param>
        /// <returns>List of Mobile Tags fitting the criteria.</returns>
        public static HashSet<BaseMobile> FindMobiles(Regions region, MobileTypes type)
        {
            HashSet<BaseMobile> tags = new HashSet<BaseMobile>();
            foreach (Mobile m in m_Mobiles.Values)
            {
                if (((m.Type == type) && (m.Region == region)) || (type == MobileTypes.Mobile && m.Region == region))
                {
                    tags.Add(m.Base());
                }
            }

            if (tags.Count > 0)
                return tags;

            return null;    // Nothing was found, return null.
        }

        public static HashSet<BaseMobile> FindNearbyMobiles(Regions region, Mobile mobile, int range)
        {
            if (mobile == null)
                return null;

            HashSet<BaseMobile> lm = new HashSet<BaseMobile>();
            foreach (Mobile m in m_Mobiles.Values)
            {
                if (m.Region != region || (m.IsPlayer && m.Serial == mobile.Serial))
                    continue;

                // Calculate the distance between the two coordinates.
                int distance = Point2D.Distance(mobile, m);

                if (distance <= range)
                    lm.Add(m.Base());
            }
            return lm;
        }

        public static Mobile FindNearestMobile(Mobile mobile)
        {
            if (mobile == null)
                return null;

            int v = mobile.Vision;
            HashSet<BaseMobile> localmobiles = new HashSet<BaseMobile>();
            int i = 0;
            while (localmobiles.Count == 0 && ++i * v < 120)
            {
                localmobiles = FindNearbyMobiles(mobile.Region, mobile, v * i);
                if (localmobiles == null)
                    localmobiles = new HashSet<BaseMobile>();
            }

            if (localmobiles.Count == 0)
                return null;   // Just return null since we found no nearby.

            Mobile closestMobile = null;
            foreach (BaseMobile m in localmobiles)
            {
                Mobile mm = FindMobile(m.Serial);
                if (mm == null)
                    continue;

                if (closestMobile == null)
                    closestMobile = mm;
                else if (Point2D.Distance(mobile, mm) < Point2D.Distance(mobile, closestMobile))
                    closestMobile = mm;
            }

            return closestMobile;
        }

        /// <summary>
        ///     Updates a Node with a mobile.
        /// </summary>
        /// <param name="toRegion">Node to move the mobile to.</param>
        /// <param name="mobile">Mobile to update.</param>
        /// <param name="forceMove">Overrides requirements if an admin is performing the action.</param>
        public static Node MoveMobile(Regions toRegion, Mobile mobile, MobileDirections direction = MobileDirections.None, bool forceMove = false)
        {
            if (mobile == null)
                return null;

            if (!forceMove)                                         // If it is not an admin move...
                if (!isConnectedLocation(mobile.Region, toRegion)
                    && toRegion != mobile.Region)                    //  And it is not a connected location...
                    return null;                                    //   Return failure.

            Node n = FindNode(toRegion);
            if (!n.isSpawnable)
            {
                mobile.Location.Invalidate();
            }
            else
            {
                Spawnable s = n as Spawnable;
                if (mobile.Location == null || toRegion != mobile.Region)
                    mobile.Location = s.StartingLocation();  // Resets or assigns the new coordinate.

                if (toRegion == mobile.Region)
                {   // Move our mobile into the targeted direction.
                    if (direction == MobileDirections.Nearby)
                    {   // Move to nearby NPC.
                        Mobile newM = FindNearestMobile(mobile);
                        if (newM != null)
                        {   // Moves the mobile to the nearest, 1 pace at a time until in vision.
                            for (int distance = Point2D.Distance(mobile, newM);
                                distance > mobile.Vision;
                                distance = Point2D.Distance(mobile, newM))
                            {
                                mobile.Location = Point2D.MoveTowards(mobile, newM, mobile.Speed);
                            }
                        }
                    }
                    else
                    {   // Move in a specific direction.
                        mobile.MoveInDirection(direction, s.MaxX, s.MaxY);
                    }
                }
            }

            mobile.Region = toRegion;    // Update the local mobile to the new location.
            return n;
        }

        public static void SwapMobileEquipment(Mobile mobile, Serial item)
        {
            if (mobile == null)
                return;

            Item i = mobile.FindItem(item);
            if (i == null || !i.IsEquippable)
                return;

            mobile.Equip(i as Equippable);
        }

        public static void Ressurrect(Regions region, Mobile mobile)
        {   // Validate we're not working with a null value.
            if (mobile == null)
                return;

            MoveMobile(region, mobile, forceMove: true);

            mobile.Ressurrect();    // Perform the ressurrection.
        }

        /// <summary>
        ///     Kills off a mobile from the GameObject.
        /// </summary>
        /// <param name="mobile">Mobile to kill.</param>
        public static void Kill(Mobile mobile)
        {
            if (mobile == null)
                return;

            mobile.Kill();  // Kill the mobile.

            if (!mobile.IsPlayer)
                RemoveMobile(mobile);    // Remove the mobile from the list of mobiles.
        }
        #endregion

        #region Nodes / Locations Actions
        public static void AddNode(Node n)
        {
            if (n == null || !Node.isValidRegion(n.Region))
                return;

            m_Nodes[n.Region] = n;
        }

        public static void RemoveNode(Regions region)
        {
            if (!Node.isValidRegion(region))
                return;

            m_Nodes.TryRemove(region, out _);
        }

        /// <summary>
        ///     Locates a Node based on it's ID.
        /// </summary>
        /// <param name="ID">ID to query the GameObject.</param>
        /// <returns>Discovered Node from the GameObject.</returns>
        public static Node FindNode(Regions region)
        {
            Node n;
            if (!m_Nodes.TryGetValue(region, out n))
                return null;    // Could not find the Node, return null.
            return n;           // Return the found Node.
        }

        /// <summary>
        ///     Validates that an originating connection (from) as a desired connection (to).
        /// </summary>
        /// <param name="from">Originating connection.</param>
        /// <param name="to">Connection to validate if exists.</param>
        /// <returns>True - Connection exists. False - Connection is faulty.</returns>
        private static bool isConnectedLocation(Regions from, Regions to)
        {
            if (!Node.isValidRegion(from) || !Node.isValidRegion(to))
                return false;   // One of them are not valid locations.

            // Get our originating location
            Node fromN = FindNode(from);
            if (fromN == null)
                return false;   // Not a valid originating location.

            return fromN.HasConnection(to);
        }
        #endregion

        #region Spawns / Spawnning
        public static bool Spawn(Mobile mobile, Regions region)
        {
            return Spawn(mobile, region, Guid.Empty);
        }

        /// <summary>
        ///     Spawns a new Mobile in the world.
        /// </summary>
        /// <param name="mobile">Mobile to add to a location.</param>
        /// <param name="region">Location to be modified.</param>
        /// <returns></returns>
        public static bool Spawn(Mobile mobile, Regions region, Guid spawner)
        {
            if (mobile == null)
                return false;
            else if (!Node.isValidRegion(region))
            {
                RemoveMobile(mobile);
                return false;   // If an invalid location is passed, return false.
            }

            if (mobile.Type == MobileTypes.Creature && spawner != Guid.Empty)
                (mobile as BaseCreature).OwningSpawner = spawner;

            mobile.Region = region; // Updates the location of the local mobile.

            Utility.ConsoleNotify($"Spawned {mobile.Name} in {mobile.Region} @{mobile.Location}.");
            return true;
        }

        public static int SpawnersCount(Regions region, Guid spawner)
        {
            if (spawner == Guid.Empty)
                return -1;

            HashSet<BaseCreature> bc = FindSpawned(region, spawner);
            if (bc == null)
                return -1;

            return bc.Count;
        }

        private static HashSet<BaseCreature> FindSpawned(Regions region, Guid spawner)
        {
            HashSet<BaseCreature> mobiles = new HashSet<BaseCreature>();
            foreach (Mobile m in m_Mobiles.Values)
            {
                if (m.Region == region && m.Type == MobileTypes.Creature)
                {
                    BaseCreature bc = m as BaseCreature;
                    if (bc == null)
                        continue;

                    if (bc.OwningSpawner == spawner)
                        mobiles.Add(bc);
                }
            }

            if (mobiles.Count > 0)
                return mobiles;

            return null;    // Nothing was found, return null.
        }
        #endregion
    }
}
