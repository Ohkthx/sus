using System;
using System.Collections.Generic;
using System.Linq;
using SUS.Shared.Utilities;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;
using SUSClient.MenuItems;

namespace SUSClient
{
    class InteractiveConsole
    {
        private enum ConsoleActions { none, move, look, lastloc, players, npcs, mobiles, attack, paperdoll, actions, exit }

        private static GameState gs = null;
        public Request clientRequest = null;    // Temporary storage for a request sent by the client.

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
                if (this.clientRequest != null)    // Requesting information from the server,
                    return gs;                                                          //  Returning early to fulfill the initiated action by the user.
            }

            this.Reset();       // Reset our bools and make everything default.
            rounds++;           // Increment our counter for amount of turns we've taken as a client.

            Dictionary<string, ConsoleActions> ValidActions = new Dictionary<string, ConsoleActions>();     // Generate our Valid Actions.

            // If Player is dead, we should send a ressurrection requestion.
            if (gs.Account.IsDead)
            {
                Utility.ConsoleNotify("Sending ressurrection request.");

                // Request to be sent to the server.
                this.clientRequest = new Request(RequestTypes.Resurrection, gs.Account.getTag());
                return gs;
            }

            Console.WriteLine("\nValid Actions: ");
            int c = 0;
            foreach (ConsoleActions action in Enum.GetValues(typeof(ConsoleActions)))
            {   // Lists all currently accessible actions.
                string name = Enum.GetName(typeof(ConsoleActions), action);
                ValidActions.Add(name, action);             // **IMPORTANT**, adds the action to a list of Valid Actions.
                if (c != 0 && (c % 6) == 0)
                    Console.Write("\n");
                Console.Write($"[{name.ToLower()}]  ");
                c++;
            }

            Console.WriteLine();

            while (clientRequest == null)
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
                        Paperdoll pd = new Paperdoll(gs.Account);
                        pd.Display();
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
            this.lastAction = ConsoleActions.none;
            this.clientRequest = null;
        }

        /// <summary>
        ///     Changes the location of the current gamestate to that which is provided.
        /// </summary>
        /// <param name="location">New location.</param>
        public void LocationUpdater(Node location)
        {
            gs.NodeCurrent = location;
            gs.Account.Location = location.Location;
        }

        /// <summary>
        ///     Moves a player to a new location, setting flag to send updated gamestate to server.
        /// </summary>
        private void move()
        {
            look();

            Locations newLoc = Locations.None;
            do
                Console.Write("Select location: ");
            while ((newLoc = gs.StringToLocation(Console.ReadLine())) == Locations.None);

            Console.WriteLine($"Selected: {newLoc.ToString()}");

            MobileMove mm = new MobileMove(newLoc, gs.Account);
            this.clientRequest = new Request(RequestTypes.MobileMove, mm);
        }

        /// <summary>
        ///     Checks nearby locations.
        /// </summary>
        private void look()
        {
            Console.WriteLine($" Nearby Locations:");
            int pos = 0;
            foreach (Locations n in gs.NodeCurrent.ConnectionsToList())
            {
                ++pos;
                Console.WriteLine($"  [Pos: {pos}] {Enum.GetName(typeof(Locations), n)}");
            }
            Console.WriteLine();
        }

        /// <summary>
        ///     Displays the last location the user visited.
        /// </summary>
        private void lastloc()
        {
            string last = string.Empty;
            if (gs.NodeLast == null)
                last = "None";
            else
                last = gs.NodeLast.Name;

            Console.WriteLine($" {last}");
        }

        /// <summary>
        ///     Makes a request to the Server for an updated list of players.
        ///     This function is called again on the list is returned by the server.
        /// </summary>
        /// <returns>List of Players from the server.</returns>
        private List<MobileTag> getMobiles()
        {
            if (this.clientRequest == null)
            {   // Create a request for the server to respond to.
                this.clientRequest = new Request(RequestTypes.LocalMobiles, gs.NodeCurrent.Location);
                return null;
            }

            // Return a list of players the server has provided.
            return gs.Mobiles.ToList();
        }

        /// <summary>
        ///     Parent caller for retrieving either local Players or NPCs.
        /// </summary>
        /// <param name="type"></param>
        private void listMobiles(MobileType type)
        {
            List<MobileTag> mobiles = getMobiles();    // Get a fresh list of mobiles from the server.
            if (mobiles == null && this.clientRequest != null)
            {
                return; // Return early to process a client request.
            }

            Console.WriteLine($" Local {type.ToString()}s:");

            int pos = 0;
            if (mobiles.Count > 0)
            {   // Iterate our list of Players.
                foreach (MobileTag m in mobiles)
                {
                    if ((type & m.Type) == m.Type)
                    {
                        pos++;
                        Console.WriteLine($"  [Pos: {pos}] {m.Name},  ID: {m.ID}");
                    }
                }
            }

            // If not Players found, print 'None'.
            if (pos == 0)
                Console.WriteLine("    => None.");

            this.Reset();
        }

        public MobileTag SelectMobile(List<MobileTag> mobiles)
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
                        Utility.ConsoleNotify($"Mobile Selected: {mobiles[input - 1].Name}.");
                        return mobiles[input - 1];
                    }
                    Utility.ConsoleNotify("Bad option, please try again.");
                }
            } while (input - 1 < 0 || input - 1 >= mobiles.Count);

            Utility.ConsoleNotify($"Something happened? Bad number: {input}.");
            return null;
        }

        /// <summary>
        ///     Initiates an attack on a mobile.
        /// </summary>
        private void attack()
        {
            List<MobileTag> mobiles = getMobiles();
            if (mobiles == null)
            {   // Get a fresh batch of local NPCs.
                return;                                 // Haven't made the request, making it now by returning early.
            }

            if (mobiles.Count == 0)
            {
                Utility.ConsoleNotify("No mobs to attack.");
                return;
            }

            MobileTag targetMobile = SelectMobile(mobiles);

            Console.WriteLine(" Performing an attack on {0}.", targetMobile.Name);

            // Our newly created action to perform.
            MobileAction attackAction = new MobileAction(gs.Account.ID);
            attackAction.Type = ActionType.Attack;
            attackAction.AddTarget(targetMobile);

            // Request to be sent to the server.
            this.clientRequest = new Request(RequestTypes.MobileAction, attackAction);
        }

        /// <summary>
        ///     List valid actions that can be performed by the Interactive Console.
        /// </summary>
        private void printActions()
        {
            Console.WriteLine("\nValid Actions:");
            int c = 0;
            foreach (string action in Enum.GetNames(typeof(ConsoleActions)))
            {
                if (c != 0 && (c % 6) == 0)
                    Console.Write("\n");
                Console.Write($"[{action.ToLower()}]  ");
                c++;
            }
            Console.WriteLine();
        }

        /// <summary>
        ///     Exits the client, sends the server a request to kill the socket.
        /// </summary>
        private void exit()
        {
            SocketKill sk = new SocketKill(gs.Account.ID, true);
            clientRequest = new Request(RequestTypes.SocketKill, sk);
        }
    }
}
