using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.Objects.Mobiles.Spawns;
using SUS.Shared.Objects.Nodes;
using SUS.Shared.Utilities;

namespace SUS
{
    [Serializable]
    public static class GameObject
    {
        #region Dictionaries, Hashsets, and Lists.
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

            // Spawn a test Orc in Britain.
            Orc orc = new Orc();
            Cyclops cyclops = new Cyclops();
            Skeleton skeleton = new Skeleton();
            if (Spawn(orc, Locations.Britain))
                Utility.ConsoleNotify($"Spawned {orc.Name} in {Locations.Britain.ToString()}.");
            if (Spawn(cyclops, Locations.Britain))
                Utility.ConsoleNotify($"Spawned {cyclops.Name} in {Locations.Britain.ToString()}.");
            if (Spawn(skeleton, Locations.Graveyard))
                Utility.ConsoleNotify($"Spawned {skeleton.Name} in {Locations.Graveyard.ToString()}.");
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

        #region Updating
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

        /// <summary>
        ///     Updates a Node with a mobile.
        /// </summary>
        /// <param name="toLocation">Node to move the mobile to.</param>
        /// <param name="mobile">Mobile to update.</param>
        /// <param name="forceMove">Overrides requirements if an admin is performing the action.</param>
        public static bool MoveMobile(Locations toLocation, MobileTag mobile, bool forceMove = false, bool ressurrection = false)
        {
            Mobile m = FindMobile(mobile.Guid);
            if (m == null)
            {   // Our mobile does not appear to exist.
                Console.WriteLine($" [ ERR ] Mobile missing: '{mobile.ID}::{mobile.Name}::Player:{mobile.IsPlayer}");
                return false;
            }

            if (ressurrection)
                m.Ressurrect(); // Ressurrect if requested.
            else if (toLocation == m.Location)
                return false;   // Trying to move within the same location.

            if (!forceMove)                                         // If it is not an admin move...
                if (!isConnectedLocation(m.Location, toLocation))   //  And it is not a connected location...
                    return false;                                   //   Return failure.
            

            m.Location = toLocation;    // Update the local mobile to the new location.
            UpdateMobiles(m);           // Update the mobile to our tracked mobile
            return true;
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

        #region Finding
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
        public static Mobile FindMobile(MobileType type, Serial serial)
        {   // Iterate our hashset of mobiles.
            foreach (KeyValuePair<Guid, Mobile> m in m_Mobiles)
                if (((m.Value.Type == type) && (m.Value.ID == serial)) || type == MobileType.Mobile)
                        return m.Value;   // If the type and serial match, return it. If it is type of 'Any', return it.

            return null;    // Nothing was found, return null.
        }

        /// <summary>
        ///     Gets all of the Mobile Tags for a specific location of a specific type.
        /// </summary>
        /// <param name="loc">Location to search.</param>
        /// <param name="type">Type of mobile."</param>
        /// <returns>List of Mobile Tags fitting the criteria.</returns>
        public static HashSet<MobileTag> FindMobiles(Locations loc, MobileType type)
        {
            HashSet<MobileTag> tags = new HashSet<MobileTag>();
            foreach (KeyValuePair<Guid, Mobile> m in m_Mobiles)
            {
                if (((m.Value.Type == type) && (m.Value.Location == loc)) || (type == MobileType.Mobile && m.Value.Location == loc))
                {
                    tags.Add(m.Value.getTag());
                }
            }

            if (tags.Count > 0)
                return tags;

            return null;    // Nothing was found, return null.
        }

        /// <summary>
        ///     Finds a player type mobile.
        /// </summary>
        /// <param name="serial">Serial to find.</param>
        /// <returns></returns>
        public static Mobile FindPlayer(Serial serial)
        {
            return FindMobile(MobileType.Player, serial);
        }

        /// <summary>
        ///     Finds a NPC type mobile.
        /// </summary>
        /// <param name="serial">Serial to find.</param>
        /// <returns></returns>
        public static Mobile FindNPC(Serial serial)
        {
            return FindMobile(MobileType.NPC, serial);
        }

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

        /// <summary>
        ///     Spawns a new Mobile in the world.
        /// </summary>
        /// <param name="mobile">Mobile to add to a location.</param>
        /// <param name="location">Location to be modified.</param>
        /// <returns></returns>
        public static bool Spawn(Mobile mobile, Locations location)
        {
            if (!Node.isValidLocation(location))
                return false;   // If an invalid location is passed, return false.

            mobile.Location = location; // Updates the location of the local mobile.
            UpdateMobiles(mobile);      // Update the mobile in the GameObject.

            return true;                // Success, return true.
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
    }
}
