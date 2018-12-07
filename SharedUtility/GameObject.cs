using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.Objects.Nodes;
using SUS.Shared.Utilities;

namespace SUS
{
    [Serializable]
    public static class GameObject
    {
        #region Dictionaries and Variables.
        // Player ID => GameState
        private static ConcurrentDictionary<ulong, GameState> m_Gamestates = new ConcurrentDictionary<ulong, GameState>();

        // Location.Type => Node
        private static ConcurrentDictionary<Locations, Node> m_Nodes = new ConcurrentDictionary<Locations, Node>();

        // Contains all currently acitve/alive mobiles.
        private static ConcurrentDictionary<Guid, Mobile> m_Mobiles = new ConcurrentDictionary<Guid, Mobile>();

        // Player ID => Client Socket
        private static ConcurrentDictionary<ulong, SocketHandler> m_Clients = new ConcurrentDictionary<ulong, SocketHandler>();
        #endregion

        #region Getters / Setters
        public static Locations StartingZone
        {
            get { return GetStartingZone().Location; }
        }
        #endregion

        /// <summary>
        ///     Starts our serials and creates our map. To be called first to initiate things.
        /// </summary>
        public static void Initiate()
        {
            // Start our serials.
            Serial s = new Serial(0);

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

            // Add pathing / connections to each.
            Britain.AddConnection(Locations.Sewers | Locations.Graveyard | Locations.Wilderness);
            Sewer.AddConnection(Locations.Britain);
            Wilderness.AddConnection(Locations.Britain | Locations.Graveyard);
            Graveyard.AddConnection(Locations.Britain | Locations.Wilderness);
            

            // Add the nodes to be held by the GameObject.
            UpdateNodes(Britain);
            UpdateNodes(Sewer);
            UpdateNodes(Wilderness);
            UpdateNodes(Graveyard);
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
            return m_Nodes[Locations.Britain];
        }
        #endregion

        #region Gamestate Actions
        /// <summary>
        ///     GameState to locate in GameObject.
        /// </summary>
        /// <param name="ID">ID of the GameState.</param>
        /// <returns>GameState provided by GameObject.</returns>
        public static GameState FindGameState(ulong ID)
        {
            GameState gs;   // Blank Gamestate.
            if (!m_Gamestates.TryGetValue(ID, out gs))
                return null;    // Could not find the GameState, return null.
            return gs;          // Return the found GameState.
        }

        /// <summary>
        ///     Updates GameObject's tracked gamestates.
        /// </summary>
        /// <param name="gamestate">Gamestate to add or remove.</param>
        /// <param name="remove">Determines if the gamestate should be permanently removed.</param>
        public static void UpdateGameStates(GameState gamestate, bool remove = false)
        {

            if (remove)
                // Removes if the player DOES exist.
                m_Gamestates.TryRemove(gamestate.ID, out _);
            else
                // This will add or update (override current).
                m_Gamestates[gamestate.ID] = gamestate;
        }
        #endregion

        #region Mobile Actions
        public static Mobile FindMobile(BasicMobile mobile) { return FindMobile(mobile.Guid); }
        public static Mobile FindMobile(Mobile mobile) { return FindMobile(mobile.Guid); }
        public static Mobile FindMobile(Guid guid)
        {
            if (m_Mobiles.ContainsKey(guid))
            {
                Mobile m = null;
                if (m_Mobiles.TryGetValue(guid, out m))
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
        public static Mobile FindMobile(Mobile.Types type, Serial serial)
        {   // Iterate our hashset of mobiles.
            foreach (KeyValuePair<Guid, Mobile> m in m_Mobiles)
                if (((m.Value.Type == type) && (m.Value.ID == serial))
                    || type == Mobile.Types.Mobile)
                {
                    return m.Value;   // If the type and serial match, return it. If it is type of 'Any', return it.
                }

            return null;    // Nothing was found, return null.
        }

        /// <summary>
        ///     Gets all of the Mobile Tags for a specific location of a specific type.
        /// </summary>
        /// <param name="loc">Location to search.</param>
        /// <param name="type">Type of mobile."</param>
        /// <returns>List of Mobile Tags fitting the criteria.</returns>
        public static HashSet<BasicMobile> FindMobiles(Locations loc, Mobile.Types type)
        {
            HashSet<BasicMobile> tags = new HashSet<BasicMobile>();
            foreach (KeyValuePair<Guid, Mobile> m in m_Mobiles)
            {
                if (((m.Value.Type == type) && (m.Value.Location == loc)) || (type == Mobile.Types.Mobile && m.Value.Location == loc))
                {
                    tags.Add(m.Value.Basic());
                }
            }

            if (tags.Count > 0)
                return tags;

            return null;    // Nothing was found, return null.
        }

        public static HashSet<BasicMobile> FindNearbyMobiles(Locations loc, Mobile baseMobile, int range)
        {
            HashSet<BasicMobile> lm = new HashSet<BasicMobile>();
            foreach (KeyValuePair<Guid, Mobile> m in m_Mobiles)
            {
                if (m.Value.Location != loc || (m.Value.IsPlayer && m.Value.ID == baseMobile.ID))
                    continue;

                // Calculate the distance between the two coordinates.
                int distance = baseMobile.Coordinate.Distance(m.Value.Coordinate);

                if (distance <= range)
                    lm.Add(m.Value.Basic());
            }
            return lm;
        }

        public static Mobile FindNearestMobile(ref Mobile mobile)
        {
            int v = mobile.Vision;
            HashSet<BasicMobile> localmobiles = new HashSet<BasicMobile>();
            int i = 0;
            while (localmobiles.Count == 0 && ++i * v < 120)
            {
                localmobiles = FindNearbyMobiles(mobile.Location, mobile, v * i);
                if (localmobiles == null)
                    localmobiles = new HashSet<BasicMobile>();
            }

            if (localmobiles.Count == 0)
                return null;   // Just return null since we found no nearby.

            Mobile closestMobile = null;
            foreach (BasicMobile m in localmobiles)
            {
                Mobile mm = FindMobile(m.Guid);
                if (mm == null)
                    continue;

                if (closestMobile == null)
                    closestMobile = mm;
                else if (mobile.Coordinate.Distance(mm.Coordinate) < mobile.Coordinate.Distance(closestMobile.Coordinate))
                    closestMobile = mm;
            }

            return closestMobile;
        }

        /// <summary>
        ///     Finds a player type mobile.
        /// </summary>
        /// <param name="serial">Serial to find.</param>
        /// <returns></returns>
        public static Mobile FindPlayer(Serial serial)
        {
            return FindMobile(Mobile.Types.Player, serial);
        }

        /// <summary>
        ///     Finds a NPC type mobile.
        /// </summary>
        /// <param name="serial">Serial to find.</param>
        /// <returns></returns>
        public static Mobile FindNPC(Serial serial)
        {
            return FindMobile(Mobile.Types.NPC, serial);
        }

        /// <summary>
        ///     Updates a Node with a mobile.
        /// </summary>
        /// <param name="toLocation">Node to move the mobile to.</param>
        /// <param name="mobile">Mobile to update.</param>
        /// <param name="forceMove">Overrides requirements if an admin is performing the action.</param>
        public static Node MoveMobile(Locations toLocation, BasicMobile mobile, Mobile.Directions direction = Mobile.Directions.None, bool forceMove = false)
        {
            Mobile m = FindMobile(mobile.Guid);
            if (m == null)
            {   // Our mobile does not appear to exist.
                Console.WriteLine($" [ ERR ] Mobile missing: '{mobile.ID}::{mobile.Name}::Player:{mobile.IsPlayer}");
                return null;
            }

            if (!forceMove)                                         // If it is not an admin move...
                if (!isConnectedLocation(m.Location, toLocation)
                    && toLocation != m.Location)                    //  And it is not a connected location...
                    return null;                                    //   Return failure.

            Node n = FindNode(toLocation);
            if (n.isSpawnable)
            {
                Spawnable s = n as Spawnable;
                if (m.Coordinate == null || toLocation != m.Location)
                    m.Coordinate = s.StartingCoordinate();  // Resets or assigns the new coordinate.

                if (toLocation == m.Location)
                {   // Move our mobile into the targeted direction.
                    if (direction == Mobile.Directions.Nearby)
                    {   // Move to nearby NPC.
                        Mobile newM = FindNearestMobile(ref m);
                        if (newM != null)
                        {   // Moves the mobile to the nearest, 1 pace at a time until in vision.
                            while (m.Coordinate.Distance(newM.Coordinate) > m.Vision)
                            {
                                m.Coordinate.MoveTowards(newM.Coordinate, m.Speed);
                            }
                        }
                    }
                    else
                    {   // Move in a specific direction.
                        m.MoveInDirection(direction, s.MaxX, s.MaxY);
                    }
                }
            }
            else
                m.Coordinate = null;

            m.Location = toLocation;    // Update the local mobile to the new location.
            if (UpdateMobiles(m))       // Update the mobile to our tracked mobile
            {
                return FindNode(toLocation);
            }

            return null;
        }

        /// <summary>
        ///     Update the GameObject's mobiles.
        /// </summary>
        /// <param name="mobile">Mobile to be modified.</param>
        /// <param name="remove">Determines if the mobile should be removed or not.</param>
        public static bool UpdateMobiles(Mobile mobile, bool remove = false)
        {   // If the key does not exist and we are not removing, try and add and return.
            if (!m_Mobiles.ContainsKey(mobile.Guid) && !remove)
                return m_Mobiles.TryAdd(mobile.Guid, mobile);

            // Attempt to remove the key, returning and ignoring the 'out' requirement.
            if (remove)
                return m_Mobiles.TryRemove(mobile.Guid, out _);

            // Lastly, if the key existed and we are not removing... simply update it.
            m_Mobiles[mobile.Guid] = mobile;
            return true;
        }

        public static bool SwapMobileEquipment(BasicMobile mobile, Guid item)
        {
            if (mobile == null || item == null || item == Guid.Empty)
                return false;

            Mobile m = FindMobile(mobile);
            if (m == null)
                return false;

            Item i = m.FindItem(item);
            if (i == null || !i.IsEquippable)
                return false;

            m.Equip(i as Equippable);

            return UpdateMobiles(m);
        }

        public static bool Ressurrect(Locations loc, BasicMobile mobile)
        {   // Validate we're not working with a null value.
            if (mobile == null)
                return false;

            MoveMobile(loc, mobile, forceMove: true);

            Mobile m = FindMobile(mobile.Guid);
            if (m == null)
                return false;

            m.Ressurrect();    // Perform the ressurrection.
            return UpdateMobiles(m);
        }

        /// <summary>
        ///     Kills off a mobile from the GameObject.
        /// </summary>
        /// <param name="mobile">Mobile to kill.</param>
        public static void Kill(Mobile mobile)
        {
            mobile.Kill();  // Kill the mobile.

            if (mobile.IsPlayer)
                UpdateMobiles(mobile);                  // Update the players health, but do not remove.
            else
                UpdateMobiles(mobile, remove: true);    // Remove the mobile from the list of mobiles.
        }
        #endregion

        #region Nodes / Locations Actions
        /// <summary>
        ///     Locates a Node based on it's ID.
        /// </summary>
        /// <param name="ID">ID to query the GameObject.</param>
        /// <returns>Discovered Node from the GameObject.</returns>
        public static Node FindNode(Locations loc)
        {
            Node n;
            if (!m_Nodes.TryGetValue(loc, out n))
                return null;    // Could not find the Node, return null.
            return n;           // Return the found Node.
        }

        /// <summary>
        ///     Update nodes, remove if requested.
        /// </summary>
        /// <param name="node">Node to be added or removed.</param>
        /// <param name="remove">Determines if we need to remove the node.</param>
        public static void UpdateNodes(Node node, bool remove = false)
        {
            if (remove)
                m_Nodes.TryRemove(node.Location, out _);    // Removes the node.
            else
                m_Nodes[node.Location] = node;    // Reassigns the node.
        }

        /// <summary>
        ///     Validates that an originating connection (from) as a desired connection (to).
        /// </summary>
        /// <param name="from">Originating connection.</param>
        /// <param name="to">Connection to validate if exists.</param>
        /// <returns>True - Connection exists. False - Connection is faulty.</returns>
        private static bool isConnectedLocation(Locations from, Locations to)
        {
            if (!Node.isValidLocation(from) || !Node.isValidLocation(to))
                return false;   // One of them are not valid locations.

            // Get our originating location
            Node fromN = FindNode(from);
            if (fromN == null)
                return false;   // Not a valid originating location.

            return fromN.HasConnection(to);
        }
        #endregion

        #region Spawns / Spawnning
        public static bool Spawn(Mobile mobile, Locations location)
        {
            return Spawn(mobile, location, Guid.Empty);
        }

        /// <summary>
        ///     Spawns a new Mobile in the world.
        /// </summary>
        /// <param name="mobile">Mobile to add to a location.</param>
        /// <param name="location">Location to be modified.</param>
        /// <returns></returns>
        public static bool Spawn(Mobile mobile, Locations location, Guid spawner)
        {
            if (mobile == null)
                return false;
            else if (!Node.isValidLocation(location))
                return false;   // If an invalid location is passed, return false.

            if (mobile.Type == Mobile.Types.Creature && spawner != Guid.Empty)
                (mobile as BaseCreature).OwningSpawner = spawner;

            mobile.Location = location; // Updates the location of the local mobile.
            if (UpdateMobiles(mobile))  // Update the mobile in the GameObject.
            {
                Utility.ConsoleNotify($"Spawned {mobile.Name} in {mobile.Location.ToString()} @({mobile.Coordinate.X}, {mobile.Coordinate.Y}).");
                return true;
            }

            return false;
        }

        public static int SpawnersCount(Locations loc, Guid spawner)
        {
            if (spawner == Guid.Empty)
                return -1;

            HashSet<BaseCreature> bc = FindSpawned(loc, spawner);
            if (bc == null)
                return -1;

            return bc.Count;
        }

        private static HashSet<BaseCreature> FindSpawned(Locations loc, Guid spawner)
        {
            HashSet<BaseCreature> mobiles = new HashSet<BaseCreature>();
            foreach (KeyValuePair<Guid, Mobile> m in m_Mobiles)
            {
                if (m.Value.Location == loc && m.Value.Type == Mobile.Types.Creature)
                {
                    BaseCreature bc = m.Value as BaseCreature;
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
