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

        #region Dictionaries
        // Player ID => GameState
        private static Dictionary<ulong, GameState> m_Gamestates = new Dictionary<ulong, GameState>();

        // Location.Type => Node
        private static Dictionary<int, Node> m_Nodes = new Dictionary<int, Node>();

        // Serial => Mobile
        private static Dictionary<Serial, Mobile> m_Mobiles = new Dictionary<Serial, Mobile>();

        // Player ID => Player 
        private static Dictionary<ulong, Player> m_Players = new Dictionary<ulong, Player>();

        // Player ID => Client Socket
        private static Dictionary<ulong, SocketHandler> m_Clients = new Dictionary<ulong, SocketHandler>();
        #endregion

        #region Map Data
        public static void CreateMap()
        {
            if (m_Nodes.Count() != 0)
                return;

            Node Britain = new Node(Types.Town, Locations.Britain, "Britain Bank!");
            Node Sewers = new Node(Types.Dungeon | Types.PvP, Locations.Sewers, "EW! Sticky!");
            Node Wilderness = new Node(Types.OpenWorld | Types.PvP, Locations.Wilderness, "Vast open world...");
            Node Graveyard = new Node(Types.OpenWorld | Types.PvP, Locations.Graveyard, "Spooky Skelematinns.");

            // Add pathing here.
            Britain.AddConnection(ref Sewers);
            Britain.AddConnection(ref Wilderness);
            Britain.AddConnection(ref Graveyard);

            UpdateNodes(Britain);
            UpdateNodes(Sewers);
            UpdateNodes(Wilderness);
            UpdateNodes(Graveyard);

            if (Spawn(new NPC("Orc", 10, 10, 8, 2), Locations.Britain))
                Console.WriteLine("Spawned an Orc in Britain.");
        }

        public static Node GetStartingZone()
        {
            if (m_Nodes.Count() == 0)
                CreateMap();

            return m_Nodes[(int)Locations.Britain];
        }
        #endregion

        #region Updating
        public static void UpdateGameStates(ref GameState gamestate, bool remove = false)
        {
            // Update Location if the gamestate shows that the character moved from last location.
            if (gamestate.moved)
            {
                if (gamestate.LocationLast != null)
                { 
                    // Remove from old location.
                    UpdateLocationPlayer(gamestate.LocationLast.ID, gamestate.Account, true);
                    gamestate.LocationLast = m_Nodes[(int)gamestate.LocationLast.ID];
                }

                // Add to new location.
                UpdateLocationPlayer(gamestate.Location.ID, gamestate.Account);

                // Reflect the updated locations back to the user.
                gamestate.Location = m_Nodes[(int)gamestate.Location.ID];
                gamestate.moved = false;
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

        private static void UpdatePlayers(Player player, bool remove = false)
        {
            playersMutex.WaitOne();
            UInt64 pUint64 = player.m_ID.ToInt();
            if (remove)
                m_Players.Remove(pUint64);
            else
                m_Players[pUint64] = player;
            playersMutex.ReleaseMutex();
        }

        // Add or Remove a player from a Node.
        private static void UpdateLocationPlayer(int nodeKey, Player account, bool remove = false)
        {
            Node n;
            if (!m_Nodes.TryGetValue((int)nodeKey, out n))
            {
                // Location doesn't exist?!
                Console.WriteLine($" [ ERR ] Location missing: {Enum.GetName(typeof(Locations), nodeKey)}");
                return;
            }

            // Edit our node with the new player location and reassign.
            if (remove)
                n.RemoveMobile(account);
            else
                n.AddMobile(account);

            UpdateNodes(n);
        }

        public static void UpdateMobiles(Mobile mobile, bool remove = false)
        {
            Mobile m;
            if (!m_Mobiles.TryGetValue((Serial)mobile.m_ID, out m))
            {   // Doesn't exist, add it.
                mobilesMutex.WaitOne();
                m_Mobiles.Add(mobile.m_ID, mobile);
                mobilesMutex.ReleaseMutex();
                return;
            }

            mobilesMutex.WaitOne();
            if (remove)
                m_Mobiles.Remove(mobile.m_ID);
            else
            {
                m_Mobiles[mobile.m_ID] = mobile;
            }
            mobilesMutex.ReleaseMutex();
        }

        public static void UpdateNodes(Node node, bool remove = false)
        {
            locationsMutex.WaitOne();
            if (remove)
                m_Nodes.Remove(node.ID);
            else
            {
                // TODO: Combine Location data here (players?, add and remove.
                m_Nodes[node.ID] = node;

            }
            locationsMutex.ReleaseMutex();
        }
        #endregion

        #region Finding
        public static Mobile FindMobile(Serial serial)
        {
            Mobile m;
            if (!m_Mobiles.TryGetValue(serial, out m))
                return null;
            return m;
        }

        public static GameState FindGameState(ulong ID)
        {
            GameState gs;
            if (!m_Gamestates.TryGetValue(ID, out gs))
                return null;
            return gs;
        }
            
        public static Node FindNode(int ID)
        {
            Node n;
            if (!m_Nodes.TryGetValue(ID, out n))
                return null;
            return n;
        }
        #endregion

        public static void Initiate()
        {
            // Start our serials.
            Serial s = new Serial(0);

            // Create our map.
            if (m_Nodes.Count() == 0)
                CreateMap();
        }

        public static bool Spawn(Mobile mobile, Locations location)
        {
            Node n = FindNode((int)location);
            if (n == null)
                return false;       // Bad location was provided.

            GameObject.UpdateMobiles(mobile);   // Add our mobile to our list.

            if (n.AddMobile(mobile))
            {   // If adding the mobile to the location succeeded, update the Nodes.
                UpdateNodes(n);         // Update the current list of nodes.
                return true;
            }

            return false;
        }
    }
}
