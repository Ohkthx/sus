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
    public sealed class GameObject
    {
        #region Locks
        private static GameObject instance = null;
        private static readonly object padlock = new object();
        private static Mutex gamestatesMutex = new Mutex();
        private static Mutex playersMutex = new Mutex();
        private static Mutex locationsMutex = new Mutex();
        private static Mutex clientsMutex = new Mutex();
        #endregion

        #region Dictionaries
        // Player ID => GameState
        private static Dictionary<ulong, GameState> GameStates = new Dictionary<ulong, GameState>();

        // Location.Type => Node
        private static Dictionary<int, Node> Nodes = new Dictionary<int, Node>();

        // Player ID => Player 
        private static Dictionary<ulong, Player> Players = new Dictionary<ulong, Player>();

        // Player ID => Client Socket
        private static Dictionary<ulong, SocketHandler> Clients = new Dictionary<ulong, SocketHandler>();
        #endregion

        #region Events
        private delegate void locationRaiser();
        private event locationRaiser locationUpdate;
        #endregion

        #region Constructors
        GameObject()
        {
        }

        public static GameObject Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new GameObject();

                    }
                    return instance;
                }
            }
        }
        #endregion

        #region Map Data
        public void CreateMap()
        {
            if (Nodes.Count() != 0)
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
        }

        public Node GetStartingZone()
        {
            if (Nodes.Count() == 0)
                CreateMap();

            return Nodes[(int)Locations.Britain];
        }
        #endregion

        #region Updating
        public void UpdateGameStates(ref GameState gamestate, bool remove = false)
        {
            // Update Location if the gamestate shows that the character moved from last location.
            if (gamestate.moved)
            {
                if (gamestate.LocationLast != null)
                { 
                    // Remove from old location.
                    this.UpdateLocationPlayer(gamestate.LocationLast.ID, gamestate.Account, true);
                    gamestate.LocationLast = Nodes[(int)gamestate.LocationLast.ID];
                }

                // Add to new location.
                this.UpdateLocationPlayer(gamestate.Location.ID, gamestate.Account);

                // Reflect the updated locations back to the user.
                gamestate.Location = Nodes[(int)gamestate.Location.ID];
                gamestate.moved = false;
            }

            gamestatesMutex.WaitOne();
            if (remove)
                // Removes if the player DOES exist.
                GameStates.Remove(gamestate.ID());
            else
                // This will add or update (override current).
                GameStates[gamestate.ID()] = gamestate;

            UpdatePlayers(gamestate.GetPlayer(), remove);
            gamestatesMutex.ReleaseMutex();
        }

        private void UpdatePlayers(Player player, bool remove = false)
        {
            playersMutex.WaitOne();
            if (remove)
                Players.Remove(player.m_ID);
            else
                Players[player.m_ID] = player;
            playersMutex.ReleaseMutex();
        }

        // Add or Remove a player from a Node.
        private void UpdateLocationPlayer(int nodeKey, Player account, bool remove = false)
        {
            Node n;
            if (!Nodes.TryGetValue((int)nodeKey, out n))
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

            this.UpdateNodes(n);
        }

        public void UpdateNodes(Node node, bool remove = false)
        {
            locationsMutex.WaitOne();
            if (remove)
                Nodes.Remove(node.ID);
            else
            {
                // TODO: Combine Location data here (players?, add and remove.
                Nodes[node.ID] = node;

                // Event to update players.
                this.locationUpdate?.Invoke();
            }
            locationsMutex.ReleaseMutex();
        }
        #endregion

        public GameState GetGameState(ulong ID)
        {
            GameState gs;
            if (!GameStates.TryGetValue(ID, out gs))
                return null;
            return gs;
        }
            
        public Node GetNode(int ID)
        {
            Node n;
            if (!Nodes.TryGetValue(ID, out n))
                return null;
            return n;
        }
    }
}
