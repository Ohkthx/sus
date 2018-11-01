using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.Utility;

namespace SUS
{
    [Serializable]
    public static class GameObject
    {
        #region Locks
        private static readonly object padlock = new object();
        private static Mutex gamestatesMutex = new Mutex();
        private static Mutex mobilesMutex = new Mutex();
        private static Mutex playersMutex = new Mutex();
        private static Mutex locationsMutex = new Mutex();
        private static Mutex clientsMutex = new Mutex();
        #endregion

        #region Dictionaries, Hashsets, and Lists.
        // Player ID => GameState
        private static Dictionary<ulong, GameState> m_Gamestates = new Dictionary<ulong, GameState>();

        // Location.Type => Node
        private static Dictionary<int, Node> m_Nodes = new Dictionary<int, Node>();

        // Contains all currently acitve/alive mobiles.
        private static HashSet<Mobile> m_Mobiles = new HashSet<Mobile>();

        // Player ID => Player 
        private static Dictionary<ulong, Player> m_Players = new Dictionary<ulong, Player>();

        // Player ID => Client Socket
        private static Dictionary<ulong, SocketHandler> m_Clients = new Dictionary<ulong, SocketHandler>();
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
            Node Britain = new Node(Types.Town, Locations.Britain, "Britain Bank!");
            Node Sewers = new Node(Types.Dungeon | Types.PvP, Locations.Sewers, "EW! Sticky!");
            Node Wilderness = new Node(Types.OpenWorld | Types.PvP, Locations.Wilderness, "Vast open world...");
            Node Graveyard = new Node(Types.OpenWorld | Types.PvP, Locations.Graveyard, "Spooky Skelematinns.");

            // Add pathing / connections to each.
            Britain.AddConnection(ref Sewers);
            Britain.AddConnection(ref Wilderness);
            Britain.AddConnection(ref Graveyard);

            // Add the nodes to be held by the GameObject.
            UpdateNodes(Britain);
            UpdateNodes(Sewers);
            UpdateNodes(Wilderness);
            UpdateNodes(Graveyard);

            // Spawn a test Orc in Britain.
            NPC npc = new NPC("Orc", 10, 10, 8, 2);
            if (Spawn(npc, Locations.Britain))
            {
                ConsoleColor cc = Console.ForegroundColor;          // Save the console's color.
                Console.ForegroundColor = ConsoleColor.DarkRed;     // Set the color to Dark Red.
                Console.WriteLine($" !! Spawned an {npc.m_Name} in {Locations.Britain.ToString()}.");
                Console.ForegroundColor = cc;                       // Reset the color to the default.
            }
        }

        /// <summary>
        ///     Gets the current starting zone for new players.
        /// </summary>
        /// <returns>A Node of the starting zone.</returns>
        public static Node GetStartingZone()
        {   // If the nodes have not been added, create the map first.
            if (m_Nodes.Count() == 0)
                CreateMap();

            // Returns the designated starting Node.
            return m_Nodes[(int)Locations.Britain];
        }
        #endregion

        #region Updating
        /// <summary>
        ///     Updates GameObject's tracked gamestates.
        /// </summary>
        /// <param name="gamestate">Gamestate to add or remove.</param>
        /// <param name="remove">Determines if the gamestate should be permanently removed.</param>
        public static void UpdateGameStates(ref GameState gamestate, bool remove = false)
        {
            // Update Location if the gamestate shows that the character moved from last location.
            if (gamestate.moved)
            {
                if (gamestate.LocationLast != null)
                { 
                    // Remove from old location.
                    UpdateLocationMobile(gamestate.LocationLast.ID, gamestate.Account, remove: true);
                    gamestate.LocationLast = m_Nodes[(int)gamestate.LocationLast.ID];
                }

                // Add to new location.
                gamestate.Account.Location = gamestate.Location.GetLocation();  // Updates the player's location.
                UpdateLocationMobile(gamestate.Location.ID, gamestate.Account); // Update the node reflecting the new player.

                // Reflect the updated locations back to the user.
                gamestate.Location = m_Nodes[(int)gamestate.Location.ID];       // Set the Gamestate's location to the new node.
                gamestate.moved = false;                                        // Untoggle the move flag.
            }

            gamestatesMutex.WaitOne();
            if (remove)
                // Removes if the player DOES exist.
                m_Gamestates.Remove(gamestate.ID());
            else
                // This will add or update (override current).
                m_Gamestates[gamestate.ID()] = gamestate;

            UpdatePlayers(gamestate.GetPlayer(), remove);
            gamestatesMutex.ReleaseMutex();
        }

        /// <summary>
        ///     Update tracked players with new one provided, remove if requested.
        /// </summary>
        /// <param name="player">Player to add/modify.</param>
        /// <param name="remove">Determines if it should be removed or not.</param>
        private static void UpdatePlayers(Player player, bool remove = false)
        {
            playersMutex.WaitOne();                 // (Wait / Lock) 
            UInt64 pUint64 = player.m_ID.ToInt();   // Get the interger.
            if (remove)
                m_Players.Remove(pUint64);          // Remove the player.
            else
                m_Players[pUint64] = player;        // Replace the player.
            playersMutex.ReleaseMutex();            // Release to allow modification.
        }

        /// <summary>
        ///     Updates a Node with a mobile.
        /// </summary>
        /// <param name="nodeKey">Node to update(key).</param>
        /// <param name="mobile">Mobile to add or remove.</param>
        /// <param name="remove">Determines if the mobile is to be removed or not.</param>
        private static void UpdateLocationMobile(int nodeKey, Mobile mobile, bool remove = false)
        {
            Node n = null;
            if (!m_Nodes.TryGetValue((int)nodeKey, out n))
            {   // Location doesn't exist?!
                Console.WriteLine($" [ ERR ] Location missing: {Enum.GetName(typeof(Locations), nodeKey)}");
                return;
            }

            if (remove)
                n.RemoveMobile(mobile); // Remove the mobile from the Node.
            else
                n.AddMobile(mobile);    // Add the mobile to the node.

            UpdateNodes(n);             // Update the GameObject's nodes list with changes.
        }

        /// <summary>
        ///     Update the GameObject's mobiles.
        /// </summary>
        /// <param name="mobile">Mobile to be modified.</param>
        /// <param name="remove">Determines if the mobile should be removed or not.</param>
        public static void UpdateMobiles(Mobile mobile, bool remove = false)
        {   // Check if the mobile is already in our GameObject's hashset.
            if (!m_Mobiles.Contains(mobile))
            {   // Mobile is not in the hashset.
                mobilesMutex.WaitOne();         // Lock the mutex.
                m_Mobiles.Add(mobile);          // Add the mobile to the hashset.
                mobilesMutex.ReleaseMutex();    // Release the mutex so the hashset can be modified.
                return;                         // Return early.
            }

            mobilesMutex.WaitOne();         // Lock the mutex.
            m_Mobiles.Remove(mobile);       // Remove the mobile.
            if (!remove)
                m_Mobiles.Add(mobile);      // If we are not strictly removing... readd it.

            mobilesMutex.ReleaseMutex();    // Allow for modification again.
        }

        /// <summary>
        ///     Update nodes, remove if requested.
        /// </summary>
        /// <param name="node">Node to be added or removed.</param>
        /// <param name="remove">Determines if we need to remove the node.</param>
        public static void UpdateNodes(Node node, bool remove = false)
        {
            locationsMutex.WaitOne();       // Lock our mutex.
            if (remove)
                m_Nodes.Remove(node.ID);    // Removes the node.
            else
                m_Nodes[node.ID] = node;    // Reassigns the node.
            locationsMutex.ReleaseMutex();  // Allows for remodification.
        }

        /// <summary>
        ///     Kills off a mobile from the GameObject.
        /// </summary>
        /// <param name="mobile">Mobile to kill.</param>
        public static void Kill(Mobile mobile)
        {
            mobile.Kill();  // Kill the mobile.
            UpdateLocationMobile((int)mobile.Location, mobile, remove: true);   // Remove the mobile from the Node.

            if (mobile is NPC)
                UpdateMobiles(mobile, remove: true);    // Remove the mobile from the list of mobiles.
        }
        #endregion

        #region Finding
        /// <summary>
        ///     Find a mobile based on it's type.
        /// </summary>
        /// <param name="type">Type of a mobile.</param>
        /// <param name="serial">Serial of the mobile to find.</param>
        /// <returns></returns>
        public static Mobile FindMobile(MobileType type, Serial serial)
        {   // Iterate our hashset of mobiles.
            foreach (Mobile m in m_Mobiles)
                if ((m.m_Type == type && m.m_ID == serial) || type == MobileType.Any)
                        return m;   // If the type and serial match, return it. If it is type of 'Any', return it.

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
        public static Node FindNode(int ID)
        {
            Node n;
            if (!m_Nodes.TryGetValue(ID, out n))
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
            Node n = FindNode((int)location);   // Attempts to find a node based on ID.
            if (n == null)
                return false;               // Bad location was provided.

            if (n.AddMobile(mobile))
            {   // If adding the mobile to the location succeeded, update the Nodes.
                mobile.Location = location; // Updates the location of the local mobile.
                UpdateMobiles(mobile);      // Update the mobile in the GameObject.
                UpdateNodes(n);             // Update the Node in the GameObject.
                return true;                // Success, return true.
            }

            return false;                   // Something occured.
        }
    }
}
