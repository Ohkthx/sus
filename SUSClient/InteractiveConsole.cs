using System;
using System.Collections.Generic;
using System.Linq;
using SUS.Shared;
using SUS.Shared.Packets;
using SUSClient.MenuItems;

namespace SUSClient
{
    class InteractiveConsole
    {
        private enum ConsoleActions { none, move, look, lastloc, players, npcs, mobiles, attack, actions, use, update, paperdoll, exit }

        private static ClientState m_Client;
        public Packet m_Request;            // Temporary storage for a request sent by the client.
        private static ulong m_Rounds = 0;  // Amount of turns the client has performed.
        private ConsoleActions LastAction;

        #region Constructors
        public InteractiveConsole(ClientState clientstate)
        {
            m_Client = clientstate;
        }
        #endregion

        #region Getters / Setters
        public ClientState Client { get { return m_Client; } }

        public Packet Request
        {
            get { return m_Request; }
            set
            {
                m_Request = value;
            }
        }

        public ulong Rounds { get { return m_Rounds; } }
        #endregion

        /// <summary>
        ///     Prompts and processes user input.
        /// </summary>
        /// <returns>Updated GameState object.</returns>
        public ClientState Core()
        {   // If our last console action required a response, process it.
            if (LastAction != ConsoleActions.none)
            {
                responseHandler();      // Processes requested information from the server.
                if (Request != null)    // Requesting information from the server,
                    return Client;      //  Returning early to fulfill the initiated action by the user.
            }

            Reset();       // Reset our bools and make everything default.
            m_Rounds++;           // Increment our counter for amount of turns we've taken as a client.

            Dictionary<string, ConsoleActions> ValidActions = new Dictionary<string, ConsoleActions>();     // Generate our Valid Actions.

            // If Player is dead, we should send a ressurrection requestion.
            if (Client.IsAlive)
            {
                Utility.ConsoleNotify("Sending ressurrection request.");

                // Request to be sent to the server.
                Request = new RessurrectMobilePacket(Client.PlayerID);
                return Client;
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

            while (Request == null)
            {   // Get our action from the user.
                ConsoleActions consoleAction = ConsoleActions.none;
                string[] actions = null;

                while (true)
                {
                    Console.Write($"\n > [Round: {Rounds}] Choose an action: ");
                    actions = Console.ReadLine().ToLower().Split(' ');

                    if (actions.Count() == 0)
                        continue;

                    // Validate the requested action is acceptable to be processed on.
                    if (ValidActions.TryGetValue(actions[0], out consoleAction))
                    {   // If it was a good action, break the while loop early.
                        LastAction = consoleAction;
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
                        listMobiles(MobileTypes.Player);
                        break;
                    case ConsoleActions.npcs:
                        listMobiles(MobileTypes.NPC | MobileTypes.Creature);
                        break;
                    case ConsoleActions.mobiles:
                        listMobiles(MobileTypes.Mobile);
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
                        Paperdoll pd = new Paperdoll(Client.PlayerID, Client.Account);
                        Request = pd.Display();
                        break;
                    case ConsoleActions.use:
                        Request = Client.UseItems();
                        break;
                    case ConsoleActions.update:
                        Console.WriteLine("Getting updated Equipment and Items.");
                        Request = new GetMobilePacket(GetMobilePacket.RequestReason.Equipment | GetMobilePacket.RequestReason.Items, Client.PlayerID);
                        break;
                    case ConsoleActions.exit:
                        exit();
                        break;
                    default:
                        Console.WriteLine("Something occured with processing this action.");
                        break;
                }
            }

            return Client;
        }

        /// <summary>
        ///     If the last action requested by the client is required
        ///     information from the server- act and recall the function
        ///     with the new data provided by the server.
        /// </summary>
        private void responseHandler()
        {
            switch (LastAction)
            {
                case ConsoleActions.players:
                    listMobiles(MobileTypes.Player);
                    break;
                case ConsoleActions.npcs:
                    listMobiles(MobileTypes.NPC | MobileTypes.Creature);
                    break;
                case ConsoleActions.mobiles:
                    listMobiles(MobileTypes.Mobile);
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
            LastAction = ConsoleActions.none;
            Request = null;
        }

        /// <summary>
        ///     Changes the location of the current gamestate to that which is provided.
        /// </summary>
        /// <param name="location">New location.</param>
        public void LocationUpdater(BaseRegion location)
        {
            if (location == null)
                return;
            Client.Region = location;
        }

        /// <summary>
        ///     Moves a player to a new location, setting flag to send updated gamestate to server.
        /// </summary>
        private void move()
        {
            look();

            MobileDirections newDir = MobileDirections.None;
            Regions newLoc = Regions.None;
            while(true)
            {
                Console.Write("Select location: ");
                string input = Console.ReadLine();

                if (Client.Region.CanTraverse)
                {
                    if ((newDir = Client.StringToDirection(input)) != MobileDirections.None)
                    {
                        Console.WriteLine($"Selected: {newDir.ToString()}");
                        Request = new MoveMobilePacket(Client.Region.Location, Client.PlayerID, newDir);
                        return;
                    }
                }

                if ((newLoc = Client.StringToLocation(input)) != Regions.None)
                {
                    Console.WriteLine($"Selected: {newLoc.ToString()}");
                    Request = new MoveMobilePacket(newLoc, Client.PlayerID);
                    return;
                }
            }
        }

        private void move(string position)
        {
            MobileDirections newDir = MobileDirections.None;
            Regions newLoc = Regions.None;

            if (Client.Region.CanTraverse)
            {
                if ((newDir = Client.StringToDirection(position)) != MobileDirections.None)
                {
                    Console.WriteLine($"Selected: {newDir.ToString()}");
                    Request = new MoveMobilePacket(Client.Region.Location, Client.PlayerID, newDir);
                    return;
                }
            }

            if ((newLoc = Client.StringToLocation(position)) != Regions.None)
            {
                Console.WriteLine($"Selected: {newLoc.ToString()}");
                Request = new MoveMobilePacket(newLoc, Client.PlayerID);
                return;
            }

            move();
        }

        /// <summary>
        ///     Checks nearby locations.
        /// </summary>
        private void look()
        {
            if (Client.Region.CanTraverse)
            {   // Print our directions since we can move within this map.
                Console.WriteLine(" Directions to travel locally:");
                foreach (MobileDirections dir in Enum.GetValues(typeof(MobileDirections)))
                {
                    if (dir == MobileDirections.None)
                        continue;

                    Console.WriteLine($"  {Enum.GetName(typeof(MobileDirections), dir)}");
                }
                Console.WriteLine();
            }

            int pos = 0;
            Console.WriteLine(" Nearby Locations:");
            foreach (Regions n in Client.Region.ConnectionsToList())
            {
                ++pos;
                Console.WriteLine($"  [Pos: {pos}] {Enum.GetName(typeof(Regions), n)}");
            }
            Console.WriteLine();
        }

        /// <summary>
        ///     Displays the last location the user visited.
        /// </summary>
        private void lastloc()
        {
            string last = string.Empty;
            if (Client.LastRegion == null)
                last = "None";
            else
                last = Client.LastRegion.ToString();

            Console.WriteLine($" {last}");
        }

        /// <summary>
        ///     Makes a request to the Server for an updated list of players.
        ///     This function is called again on the list is returned by the server.
        /// </summary>
        /// <returns>List of Players from the server.</returns>
        private List<BaseMobile> getMobiles()
        {
            if (Request == null)
            {   // Create a request for the server to respond to.
                Request = new GetMobilesPacket(Client.Region.Location, Client.PlayerID);
                return null;
            }

            // Return a list of players the server has provided.
            return Client.Mobiles.ToList();
        }

        /// <summary>
        ///     Parent caller for retrieving either local Players or NPCs.
        /// </summary>
        /// <param name="type"></param>
        private void listMobiles(MobileTypes type)
        {
            List<BaseMobile> mobiles = getMobiles();    // Get a fresh list of mobiles from the server.
            if (mobiles == null && Request != null)
            {
                return; // Return early to process a client request.
            }

            Console.WriteLine($" Local {type.ToString()}s:");

            int pos = 0;
            if (mobiles.Count > 0)
            {   // Iterate our list of Players.
                foreach (BaseMobile m in mobiles)
                {
                    if ((type & m.Type) == m.Type)
                    {
                        pos++;
                        Console.WriteLine($"  [Pos: {pos}] {m.Name},  ID: {m.Serial}");
                    }
                }
            }

            // If not Players found, print 'None'.
            if (pos == 0)
                Console.WriteLine("    => None.");

            this.Reset();
        }

        public BaseMobile SelectMobile(List<BaseMobile> mobiles)
        {
            listMobiles(MobileTypes.Mobile);    // Retreives our mobiles.

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

            Utility.ConsoleNotify("We shouldn't have gotten here...");
            return Client.Account;
        }

        private void attack(string target)
        {
            if ((target.ToLower() == "last" || target.ToLower() == "l") && Client.LastTarget != null)
            {   // Target the last mobile.
                CombatMobilePacket attackAction = new CombatMobilePacket(Client.PlayerID);
                attackAction.AddTarget(Client.LastTarget);
                Request = attackAction;
                return;
            }

            attack();
        }

        /// <summary>
        ///     Initiates an attack on a mobile.
        /// </summary>
        private void attack()
        {
            List<BaseMobile> mobiles = getMobiles();
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

            Client.LastTarget = SelectMobile(mobiles);

            // Our newly created action to perform.
            CombatMobilePacket attackAction = new CombatMobilePacket(Client.PlayerID);
            attackAction.AddTarget(Client.LastTarget);

            // Request to be sent to the server.
            Request = attackAction;
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
            Request = new SocketKillPacket(Client.PlayerID, kill: true);
        }
    }
}
