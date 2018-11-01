using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SUS.Shared.Utility;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared;

namespace SUSClient
{
    class InteractiveConsole
    {
        private enum ConsoleActions { none, move, look, lastloc, players, npcs, attack, actions, exit }
        private enum RequestStatus { none, pending, closed }    // Request status, tells if the client is waiting for information.

        private static GameState gs = null;
        public SocketKill socketKill = null;
        public Request clientRequest = null;    // Temporary storage for a request sent by the client.

        private RequestStatus status = RequestStatus.none;          // Determines if the client is in the process of requesting information.
        private ConsoleActions lastAction = ConsoleActions.none;
        public bool sendGameState = false;

        public InteractiveConsole(GameState gamestate) { gs = gamestate; }
        private static ulong rounds = 0;        // Amount of turns the client has performed.


        /// <summary>
        ///     Prompts and processes user input.
        /// </summary>
        /// <returns>Updated GameState object.</returns>
        public GameState Core()
        {   // If we requested a location of Players or NPCs, process it first.
            if (lastAction != ConsoleActions.none)
            {
                responseHandler();      // Processes requested information from the server.
                if (this.clientRequest != null && this.status != RequestStatus.none)    // Requesting information from the server,
                    return gs;                                                          //  Returning early to fulfill the initiated action by the user.
            }

            this.Reset();       // Reset our bools and make everything default.
            rounds++;           // Increment our counter for amount of turns we've taken as a client.

            Dictionary<string, ConsoleActions> ValidActions = new Dictionary<string, ConsoleActions>();     // Generate our Valid Actions.

            Console.WriteLine("\n//--------------------------------------//");
            Console.Write("Valid Actions: ");
            foreach (ConsoleActions action in Enum.GetValues(typeof(ConsoleActions)))
            {   // Lists all currently accessible actions.
                string name = Enum.GetName(typeof(ConsoleActions), action);
                ValidActions.Add(name, action);             // **IMPORTANT**, adds the action to a list of Valid Actions.
                Console.Write($"[{name.ToLower()}]  ");
            }

            while (this.socketKill == null && sendGameState == false && clientRequest == null)
            {   // Get our action from the user.
                ConsoleActions consoleAction = ConsoleActions.none;
                string act = string.Empty;

                while (true)
                {
                    Console.Write($"\n > [Round: {rounds}] Choose an action: ");
                    act = Console.ReadLine().ToLower();

                    // Validate the requested action is acceptable to be processed on.
                    if (ValidActions.TryGetValue(act, out consoleAction))
                    {   // If it was a good action, break the while loop early.
                        this.lastAction = consoleAction;
                        break;
                    }
                }

                Console.WriteLine();    // Blank line to seperate output from input.

                switch (consoleAction)
                {   // Process the action by calling the appropriate functions.
                    case ConsoleActions.move:
                        move();
                        break;
                    case ConsoleActions.look:
                        look();
                        break;
                    case ConsoleActions.lastloc:
                        lastloc();
                        break;
                    case ConsoleActions.players:
                        getPlayers();
                        break;
                    case ConsoleActions.npcs:
                        getNPCs();
                        break;
                    case ConsoleActions.actions:
                        printActions();
                        break;
                    case ConsoleActions.attack:
                        attack();
                        break;
                    case ConsoleActions.exit:
                        exit();
                        break;
                    default:
                        Console.WriteLine("Something occured with processing this action.");
                        break;
                }
            }

            return gs;
        }

        /// <summary>
        ///     If the last action requested by the client is required
        ///     information from the server- act and recall the function
        ///     with the new data provided by the server.
        /// </summary>
        private void responseHandler()
        {
            switch (lastAction)
            {
                case ConsoleActions.players:
                    getMobiles(0);
                    break;
                case ConsoleActions.npcs:
                    getMobiles(1);
                    break;
                case ConsoleActions.attack:
                    attack();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     Sets all of our states, objects, and bools to their default value for a clean reprocessing.
        /// </summary>
        public void Reset()
        {
            this.status = RequestStatus.none;
            this.lastAction = ConsoleActions.none;
            this.sendGameState = false;
            this.clientRequest = null;
        }

        /// <summary>
        ///     Changes the location of the current gamestate to that which is provided.
        /// </summary>
        /// <param name="location">New location.</param>
        public void LocationUpdater(Node location)
        {
            gs.Location = location;
        }

        /// <summary>
        ///     Moves a player to a new location, setting flag to send updated gamestate to server.
        /// </summary>
        private void move()
        {
            look();
            do
                Console.Write("Select location: ");
            while (!gs.MoveTo(Console.ReadLine()));

            Console.WriteLine($" New Location: {gs.Location.Name}");
            this.sendGameState = true;  // Sets the flag to send our gamestate.
        }

        /// <summary>
        ///     Checks nearby locations.
        /// </summary>
        private void look()
        {
            Console.WriteLine($" Nearby Locations:");
            int pos = 1;
            foreach (Node n in gs.Location.Connections)
            {
                Console.WriteLine($"  [Pos: {pos}] {n.Name} :: [{n.ID}] {n.Description}");
                pos++;
            }
            Console.WriteLine();
        }

        /// <summary>
        ///     Displays the last location the user visited.
        /// </summary>
        private void lastloc()
        {
            string last = string.Empty;
            if (gs.LocationLast == null)
                last = "None";
            else
                last = gs.LocationLast.Name;

            Console.WriteLine($" {last}");
        }

        /// <summary>
        ///     Parent caller for retrieving either local Players or NPCs.
        /// </summary>
        /// <param name="type"></param>
        private void getMobiles(int type)
        {   // Send our request if we haven't.
            if (type == 0)
                listPlayers(getPlayers());
            else
                listNPCs(getNPCs());
        }

        /// <summary>
        ///     List the Players that are currently in the area. This is updated by 'getPlayers()'.
        /// </summary>
        /// <param name="players">List of Players to display.</param>
        private void listPlayers(List<Player> players)
        {
            Console.WriteLine($" Local Players:");

            int pos = 0;
            if (players.Count > 0)
            {   // Iterate our list of Players.
                foreach (Player p in players)
                {
                    if (p.m_ID != gs.ID())
                    {
                        pos++;
                        Console.WriteLine($"  [Pos: {pos}] {p.m_Name},  ID: {p.m_ID}");
                    }
                }
            }

            // If not Players found, print 'None'.
            if (pos == 0)
                Console.WriteLine("    => None.");
        }

        /// <summary>
        ///     Makes a request to the Server for an updated list of players.
        ///     This function is called again on the list is returned by the server.
        /// </summary>
        /// <returns>List of Players from the server.</returns>
        private List<Player> getPlayers()
        {
            if (this.clientRequest == null)
            {   // Create a request for the server to respond to.
                this.clientRequest = new Request(RequestTypes.Location, gs.Location);
                return null;
            }

            // Return a list of players the server has provided.
            return gs.Location.Players.ToList();
        }

        /// <summary>
        ///     List the NPCs that are currently in the area. This is updated by 'getNPCs()'.
        /// </summary>
        /// <param name="npcs">List of NPCs to display.</param>
        private void listNPCs(List<NPC> npcs)
        {
            Console.WriteLine($" Local NPCs:");

            int pos = 0;
            if (npcs.Count > 0)
            {   // Iterate our list of NPCs
                foreach (NPC p in npcs)
                {
                    pos++;
                    Console.WriteLine($"  [Pos: {pos}] {p.m_Name},  ID: {p.m_ID.ToInt()}");
                }
            }

            // No NPCs in the list, print 'None' instead.
            if (npcs.Count == 0 && pos == 0)
                Console.WriteLine("    => None.");
        }

        /// <summary>
        ///     Makes a request to the Server for an updated list of NPCs.
        ///     This function is called again on the list is returned by the server.
        /// </summary
        /// <returns>List of NPCs from the server.</returns>
        private List<NPC> getNPCs()
        {
            if (this.clientRequest == null)
            {   // Create a request for the server to respond to.
                this.clientRequest = new Request(RequestTypes.Location, gs.Location);
                return null;
            }

            // Return a list of NPCs the server has provided.
            return gs.Location.NPCs.ToList();
        }

        /// <summary>
        ///     Initiates an attack on a mobile.
        /// </summary>
        private void attack()
        {
            List<NPC> npcs = getNPCs();
            if (npcs == null)
            {   // Get a fresh batch of local NPCs.
                this.status = RequestStatus.pending;    // We require a response from the server for updated information.
                return;                                 // Haven't made the request, making it now by returning early.
            }

            this.status = RequestStatus.closed;        // We got our response and now processing it.
            // TODO: Get the object to attack from the list of NPCs.
            listNPCs(npcs);

            if (npcs.Count == 0)
            {
                Console.WriteLine("No mobs to attack.");
                return;
            }

            NPC targetMobile = npcs.First();
            Console.WriteLine("Performing an attack on {0}.", targetMobile.m_Name);

            // Our newly created action to perform.
            MobileAction attackAction = new MobileAction(gs.Account.m_ID);
            attackAction.Type = ActionType.Attack;
            attackAction.AddTarget(targetMobile.m_Type, targetMobile.m_ID);

            // Request to be sent to the server.
            this.clientRequest = new Request(RequestTypes.MobileAction, attackAction);
        }

        /// <summary>
        ///     List valid actions that can be performed by the Interactive Console.
        /// </summary>
        private void printActions()
        {
            Console.WriteLine("\n//--------------------------------------//");
            Console.Write(" Valid Actions: \n  ");
            foreach (string action in Enum.GetNames(typeof(ConsoleActions)))
                Console.Write($"[{action.ToLower()}]  ");
            Console.WriteLine();
        }

        /// <summary>
        ///     Exits the client, sends the server a request to kill the socket.
        /// </summary>
        private void exit()
        {
            socketKill = new SocketKill(true);
            Console.WriteLine(" SocketKill is set.");
        }

    }
}
