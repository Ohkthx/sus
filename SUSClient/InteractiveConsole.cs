using System;
using System.Collections.Generic;
using System.Linq;
using SUS.Shared.Utilities;
using SUS.Shared.Objects;
using SUS.Shared.Packets;
using SUSClient.MenuItems;

namespace SUSClient
{
    class InteractiveConsole
    {
        private enum ConsoleActions { none, move, look, lastloc, players, npcs, mobiles, attack, actions, use, update, paperdoll, exit }

        private static GameState gs = null;
        public Packet clientRequest = null;    // Temporary storage for a request sent by the client.

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
            if (gs.IsDead)
            {
                Utility.ConsoleNotify("Sending ressurrection request.");

                // Request to be sent to the server.
                this.clientRequest = new RessurrectMobilePacket(gs.NodeCurrent.Location, gs.Account);
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
                string[] actions = null;

                while (true)
                {
                    Console.Write($"\n > [Round: {rounds}] Choose an action: ");
                    actions = Console.ReadLine().ToLower().Split(' ');

                    if (actions.Count() == 0)
                        continue;

                    // Validate the requested action is acceptable to be processed on.
                    if (ValidActions.TryGetValue(actions[0], out consoleAction))
                    {   // If it was a good action, break the while loop early.
                        this.lastAction = consoleAction;
                        break;
                    }
                }

                Console.WriteLine();    // Blank line to seperate output from input.

                switch (consoleAction)
                {   // Process the action by calling the appropriate functions.
                    case ConsoleActions.move:
                        if (actions.Count() > 1)
                            move(actions[1]);
                        else
                            move();
                        break;
                    case ConsoleActions.look:
                        look();
                        break;
                    case ConsoleActions.lastloc:
                        lastloc();
                        break;
                    case ConsoleActions.players:
                        listMobiles(Mobile.Types.Player);
                        break;
                    case ConsoleActions.npcs:
                        listMobiles(Mobile.Types.NPC | Mobile.Types.Creature);
                        break;
                    case ConsoleActions.mobiles:
                        listMobiles(Mobile.Types.Mobile);
                        break;
                    case ConsoleActions.actions:
                        printActions();
                        break;
                    case ConsoleActions.attack:
                        if (actions.Count() > 1)
                            attack(actions[1]);
                        else
                            attack();
                        break;
                    case ConsoleActions.paperdoll:
                        Paperdoll pd = new Paperdoll(gs.Account);
                        clientRequest = pd.Display();
                        break;
                    case ConsoleActions.use:
                        clientRequest = gs.UseItems();
                        break;
                    case ConsoleActions.update:
                        Console.WriteLine("Getting updated Equipment and Items.");
                        clientRequest = new GetMobilePacket(gs.Account, GetMobilePacket.RequestReason.Equipment | GetMobilePacket.RequestReason.Items);
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
                    listMobiles(Mobile.Types.Player);
                    break;
                case ConsoleActions.npcs:
                    listMobiles(Mobile.Types.NPC | Mobile.Types.Creature);
                    break;
                case ConsoleActions.mobiles:
                    listMobiles(Mobile.Types.Mobile);
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
        public void LocationUpdater(BasicNode location)
        {
            if (location == null)
                return;
            gs.NodeCurrent = location;
        }

        /// <summary>
        ///     Moves a player to a new location, setting flag to send updated gamestate to server.
        /// </summary>
        private void move()
        {
            look();

            Mobile.Directions newDir = Mobile.Directions.None;
            Locations newLoc = Locations.None;
            while(true)
            {
                Console.Write("Select location: ");
                string input = Console.ReadLine();

                if (gs.NodeCurrent.CanTraverse)
                {
                    if ((newDir = gs.StringToDirection(input)) != Mobile.Directions.None)
                    {
                        Console.WriteLine($"Selected: {newDir.ToString()}");
                        clientRequest = new MoveMobilePacket(gs.NodeCurrent.Location, gs.Account, newDir);
                        return;
                    }
                }

                if ((newLoc = gs.StringToLocation(input)) != Locations.None)
                {
                    Console.WriteLine($"Selected: {newLoc.ToString()}");
                    clientRequest = new MoveMobilePacket(newLoc, gs.Account);
                    return;
                }
            }
        }

        private void move(string position)
        {
            Mobile.Directions newDir = Mobile.Directions.None;
            Locations newLoc = Locations.None;

            if (gs.NodeCurrent.CanTraverse)
            {
                if ((newDir = gs.StringToDirection(position)) != Mobile.Directions.None)
                {
                    Console.WriteLine($"Selected: {newDir.ToString()}");
                    clientRequest = new MoveMobilePacket(gs.NodeCurrent.Location, gs.Account, newDir);
                    return;
                }
            }

            if ((newLoc = gs.StringToLocation(position)) != Locations.None)
            {
                Console.WriteLine($"Selected: {newLoc.ToString()}");
                clientRequest = new MoveMobilePacket(newLoc, gs.Account);
                return;
            }

            move();
        }

        /// <summary>
        ///     Checks nearby locations.
        /// </summary>
        private void look()
        {
            if (gs.NodeCurrent.CanTraverse)
            {   // Print our directions since we can move within this map.
                Console.WriteLine(" Directions to travel locally:");
                foreach (Mobile.Directions dir in Enum.GetValues(typeof(Mobile.Directions)))
                {
                    if (dir == Mobile.Directions.None)
                        continue;

                    Console.WriteLine($"  {Enum.GetName(typeof(Mobile.Directions), dir)}");
                }
                Console.WriteLine();
            }

            int pos = 0;
            Console.WriteLine(" Nearby Locations:");
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
                last = gs.NodeLast.ToString();

            Console.WriteLine($" {last}");
        }

        /// <summary>
        ///     Makes a request to the Server for an updated list of players.
        ///     This function is called again on the list is returned by the server.
        /// </summary>
        /// <returns>List of Players from the server.</returns>
        private List<BasicMobile> getMobiles()
        {
            if (this.clientRequest == null)
            {   // Create a request for the server to respond to.
                this.clientRequest = new GetMobilesPacket(gs.NodeCurrent.Location, gs.Account);
                return null;
            }

            // Return a list of players the server has provided.
            return gs.Mobiles.ToList();
        }

        /// <summary>
        ///     Parent caller for retrieving either local Players or NPCs.
        /// </summary>
        /// <param name="type"></param>
        private void listMobiles(Mobile.Types type)
        {
            List<BasicMobile> mobiles = getMobiles();    // Get a fresh list of mobiles from the server.
            if (mobiles == null && this.clientRequest != null)
            {
                return; // Return early to process a client request.
            }

            Console.WriteLine($" Local {type.ToString()}s:");

            int pos = 0;
            if (mobiles.Count > 0)
            {   // Iterate our list of Players.
                foreach (BasicMobile m in mobiles)
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

        public BasicMobile SelectMobile(List<BasicMobile> mobiles)
        {
            listMobiles(Mobile.Types.Mobile);    // Retreives our mobiles.

            int input = 0;
            do
            {
                Console.Write(" Select a target you wish to attack: ");
                if (int.TryParse(Console.ReadLine(), out input))
                {
                    if (input - 1 >= 0 && input - 1 < mobiles.Count)
                    {
                        return mobiles[input - 1];
                    }
                    Utility.ConsoleNotify("Bad option, please try again.");
                }
            } while (input - 1 < 0 || input - 1 >= mobiles.Count);

            Utility.ConsoleNotify($"Something happened? Bad number: {input}.");
            return null;
        }

        private void attack(string target)
        {
            if ((target.ToLower() == "last" || target.ToLower() == "l") && gs.MobileLast != null)
            {   // Target the last mobile.
                CombatMobilePacket attackAction = new CombatMobilePacket(gs.Account);
                attackAction.AddTarget(gs.MobileLast);
                clientRequest = attackAction;
                return;
            }

            attack();
        }

        /// <summary>
        ///     Initiates an attack on a mobile.
        /// </summary>
        private void attack()
        {
            List<BasicMobile> mobiles = getMobiles();
            if (mobiles == null)
            {   // Get a fresh batch of local NPCs.
                return;                                 // Haven't made the request, making it now by returning early.
            }

            if (mobiles.Count == 0)
            {
                Utility.ConsoleNotify("No mobs to attack.");
                Reset();
                return;
            }

            gs.MobileLast = SelectMobile(mobiles);

            // Our newly created action to perform.
            CombatMobilePacket attackAction = new CombatMobilePacket(gs.Account);
            attackAction.AddTarget(gs.MobileLast);

            // Request to be sent to the server.
            this.clientRequest = attackAction;
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
            clientRequest = new SocketKillPacket(gs.Account, kill: true);
        }
    }
}
