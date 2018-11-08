using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SUS.Shared.Utility;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;
using SUSClient.MenuItems;
using SUS.Shared;

namespace SUSClient
{
    class InteractiveConsole
    {
        private enum ConsoleActions { none, move, look, lastloc, players, npcs, mobiles, attack, paperdoll, actions, exit }
        private enum RequestStatus { none, pending, closed }    // Request status, tells if the client is waiting for information.

        private static GameState gs = null;
        public SocketKill socketKill = null;
        public Request clientRequest = null;    // Temporary storage for a request sent by the client.

        private RequestStatus status = RequestStatus.none;          // Determines if the client is in the process of requesting information.
        private ConsoleActions lastAction = ConsoleActions.none;

        public InteractiveConsole(GameState gamestate) { gs = gamestate; }
        private static ulong rounds = 0;        // Amount of turns the client has performed.


        /// <summary>
        ///     Prompts and processes user input.
        /// </summary>
        /// <returns>Updated GameState object.</returns>
        public GameState Core()
        {   // If our last console action required a response, process it.
            if (lastAction != ConsoleActions.none)
            {
                responseHandler();      // Processes requested information from the server.
                if (this.clientRequest != null && this.status != RequestStatus.none)    // Requesting information from the server,
                    return gs;                                                          //  Returning early to fulfill the initiated action by the user.
            }

            this.Reset();       // Reset our bools and make everything default.
            rounds++;           // Increment our counter for amount of turns we've taken as a client.

            Dictionary<string, ConsoleActions> ValidActions = new Dictionary<string, ConsoleActions>();     // Generate our Valid Actions.

            // If Player is dead, we should send a ressurrection requestion.
            if (gs.Account.IsDead())
            {
                Miscellaneous.ConsoleNotify("Sending ressurrection request.");

                // Request to be sent to the server.
                this.clientRequest = new Request(RequestTypes.Resurrection, gs.Account);
                return gs;
            }

            Console.WriteLine("\n//--------------------------------------//");
            Console.Write("Valid Actions: ");
            foreach (ConsoleActions action in Enum.GetValues(typeof(ConsoleActions)))
            {   // Lists all currently accessible actions.
                string name = Enum.GetName(typeof(ConsoleActions), action);
                ValidActions.Add(name, action);             // **IMPORTANT**, adds the action to a list of Valid Actions.
                Console.Write($"[{name.ToLower()}]  ");
            }

            while (this.socketKill == null && clientRequest == null)
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
                        listMobiles(MobileType.Player);
                        break;
                    case ConsoleActions.npcs:
                        listMobiles(MobileType.NPC);
                        break;
                    case ConsoleActions.mobiles:
                        listMobiles(MobileType.Mobile);
                        break;
                    case ConsoleActions.actions:
                        printActions();
                        break;
                    case ConsoleActions.attack:
                        attack();
                        break;
                    case ConsoleActions.paperdoll:
                        Paperdoll pd = new Paperdoll(gs.GetPlayer());
                        pd.Print();
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
                    listMobiles(MobileType.Player);
                    break;
                case ConsoleActions.npcs:
                    listMobiles(MobileType.NPC);
                    break;
                case ConsoleActions.mobiles:
                    listMobiles(MobileType.Mobile);
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

            MobileMove mm = new MobileMove(gs.Location.GetLocation(), gs.Account);
            this.clientRequest = new Request(RequestTypes.MobileMove, mm);
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
        ///     Makes a request to the Server for an updated list of players.
        ///     This function is called again on the list is returned by the server.
        /// </summary>
        /// <returns>List of Players from the server.</returns>
        private List<Mobile> getMobiles()
        {
            if (this.clientRequest == null)
            {   // Create a request for the server to respond to.
                this.clientRequest = new Request(RequestTypes.Node, gs.Location);
                return null;
            }

            // Return a list of players the server has provided.
            return gs.Location.Mobiles.ToList();
        }

        /// <summary>
        ///     Parent caller for retrieving either local Players or NPCs.
        /// </summary>
        /// <param name="type"></param>
        private void listMobiles(MobileType type)
        {
            List<Mobile> mobiles = getMobiles();                // Get a fresh list of mobiles from the server.
            if (mobiles == null && this.clientRequest != null)
                return; // Return early to process a client request.

            Console.WriteLine($" Local {type.ToString()}s:");

            int pos = 0;
            if (mobiles.Count > 0)
            {   // Iterate our list of Players.
                foreach (Mobile m in mobiles)
                {
                    if ((type & m.m_Type) == m.m_Type)
                    {
                        pos++;
                        Console.WriteLine($"  [Pos: {pos}] {m.m_Name},  ID: {m.m_ID}");
                    }
                }
            }

            // If not Players found, print 'None'.
            if (pos == 0)
                Console.WriteLine("    => None.");
        }

        public Mobile SelectMobile(List<Mobile> mobiles)
        {
            listMobiles(MobileType.Mobile);    // Retreives our mobiles.

            int input = 0;
            do
            {
                Console.Write(" Select a target you wish to attack: ");
                if (int.TryParse(Console.ReadLine(), out input))
                {
                    if (input - 1 >= 0 && input - 1 < mobiles.Count)
                    {
                        Miscellaneous.ConsoleNotify($"Mobile Selected: {mobiles[input - 1].m_Name}.");
                        return mobiles[input - 1];
                    }
                    Miscellaneous.ConsoleNotify("Bad option, please try again.");
                }
            } while (input - 1 < 0 || input - 1 >= mobiles.Count);

            Miscellaneous.ConsoleNotify($"Something happened? Bad number: {input}.");
            return null;
        }

        /// <summary>
        ///     Initiates an attack on a mobile.
        /// </summary>
        private void attack()
        {
            List<Mobile> mobiles = getMobiles();
            if (mobiles == null)
            {   // Get a fresh batch of local NPCs.
                this.status = RequestStatus.pending;    // We require a response from the server for updated information.
                return;                                 // Haven't made the request, making it now by returning early.
            }

            this.status = RequestStatus.closed;        // We got our response and now processing it.

            if (mobiles.Count == 0)
            {
                Miscellaneous.ConsoleNotify("No mobs to attack.");
                return;
            }

            Mobile targetMobile = SelectMobile(mobiles);

            Console.WriteLine(" Performing an attack on {0}.", targetMobile.m_Name);

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
