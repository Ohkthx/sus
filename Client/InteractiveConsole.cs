using System;
using System.Collections.Generic;
using System.Linq;
using SUS.Client.Menus;
using SUS.Shared;
using SUS.Shared.Packets;

namespace SUS.Client
{
    public class InteractiveConsole
    {
        private ConsoleActions _lastAction;

        #region Constructors

        public InteractiveConsole(ClientState state)
        {
            Client = state;
        }

        #endregion

        /// <summary>
        ///     Prompts and processes user input.
        /// </summary>
        /// <returns>Updated GameState object.</returns>
        public ClientState Core()
        {
            // If our last console action required a response, process it.
            if (_lastAction != ConsoleActions.None)
            {
                responseHandler(); // Processes requested information from the server.
                if (Request != null) // Requesting information from the server,
                    return Client; //  Returning early to fulfill the initiated action by the user.
            }

            Reset(); // Reset our bool and make everything default.
            Rounds++; // Increment our counter for amount of turns we've taken as a client.

            var validActions = new Dictionary<string, ConsoleActions>(); // Generate our Valid Actions.

            // If Player is dead, we should send a resurrection request.
            if (Client.IsAlive)
            {
                Utility.ConsoleNotify("Sending resurrection request.");

                // Request to be sent to the server.
                Request = new ResurrectMobilePacket(Client.PlayerId);
                return Client;
            }

            Console.WriteLine("\nValid Actions: ");
            var c = 0;
            foreach (ConsoleActions action in Enum.GetValues(typeof(ConsoleActions)))
            {
                // Lists all currently accessible actions.
                var name = Enum.GetName(typeof(ConsoleActions), action)?.ToLower();
                validActions.Add(name ?? throw new InvalidOperationException(),
                    action); // **IMPORTANT**, adds the action to a list of Valid Actions.

                if (c != 0 && c % 6 == 0) Console.Write("\n");

                Console.Write($"[{name.ToLower()}]  ");
                c++;
            }

            Console.WriteLine();

            while (Request == null)
            {
                // Get our action from the user.
                string[] actions;

                ConsoleActions consoleAction;
                while (true)
                {
                    Console.Write($"\n > [Round: {Rounds}] Choose an action: ");
                    actions = Console.ReadLine()?.ToLower().Split(' ');

                    if (!(actions ?? throw new InvalidOperationException()).Any()) continue;

                    // Validate the requested action is acceptable to be processed on.
                    if (!validActions.TryGetValue(actions[0], out consoleAction))
                        continue; // If it was a good action, break the while loop early.

                    _lastAction = consoleAction;
                    break;
                }

                Console.WriteLine(); // Blank line to separate output from input.

                switch (consoleAction)
                {
                    case ConsoleActions.Move when actions.Length > 1:
                        Move(actions[1]);
                        break;
                    case ConsoleActions.Move:
                        Move();
                        break;
                    case ConsoleActions.Look:
                        Look();
                        break;
                    case ConsoleActions.LastLoc:
                        LastLocation();
                        break;
                    case ConsoleActions.Players:
                        ListMobiles(MobileTypes.Player);
                        break;
                    case ConsoleActions.Npcs:
                        ListMobiles(MobileTypes.Npc | MobileTypes.Creature);
                        break;
                    case ConsoleActions.Mobiles:
                        ListMobiles(MobileTypes.Mobile);
                        break;
                    case ConsoleActions.Actions:
                        printActions();
                        break;
                    case ConsoleActions.Attack when actions.Length > 1:
                        Attack(actions[1]);
                        break;
                    case ConsoleActions.Attack:
                        Attack();
                        break;
                    case ConsoleActions.Paperdoll:
                        var pd = new Paperdoll(Client.PlayerId, Client.Account);
                        Request = pd.Display();
                        break;
                    case ConsoleActions.Use:
                        Request = Client.UseItems();
                        break;
                    case ConsoleActions.Update:
                        Console.WriteLine("Getting updated Equipment and Items.");
                        Request = new GetMobilePacket(
                            GetMobilePacket.RequestReason.Equipment | GetMobilePacket.RequestReason.Items,
                            Client.PlayerId);
                        break;
                    case ConsoleActions.Exit:
                        Exit();
                        break;
                    case ConsoleActions.None:
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
            switch (_lastAction)
            {
                case ConsoleActions.Players:
                    ListMobiles(MobileTypes.Player);
                    break;
                case ConsoleActions.Npcs:
                    ListMobiles(MobileTypes.Npc | MobileTypes.Creature);
                    break;
                case ConsoleActions.Mobiles:
                    ListMobiles(MobileTypes.Mobile);
                    break;
                case ConsoleActions.Attack:
                    Attack();
                    break;
            }
        }

        /// <summary>
        ///     Sets all of our states, objects, and bools to their default value for a clean reprocessing.
        /// </summary>
        public void Reset()
        {
            _lastAction = ConsoleActions.None;
            Request = null;
        }

        /// <summary>
        ///     Moves a player to a new location, setting flag to send updated gamestate to server.
        /// </summary>
        private void Move()
        {
            Look();

            while (true)
            {
                Console.Write("Select location: ");
                var input = Console.ReadLine();

                if (Client.CurrentRegion.Navigable)
                {
                    MobileDirections newDir;
                    if ((newDir = Client.StringToDirection(input)) != MobileDirections.None)
                    {
                        Console.WriteLine($"Selected: {newDir.ToString()}");
                        Request = new MoveMobilePacket(Client.CurrentRegion.Location, Client.PlayerId, newDir);
                        return;
                    }
                }

                Regions newLoc;
                if ((newLoc = Client.StringToLocation(input)) == Regions.None) continue;

                Console.WriteLine($"Selected: {newLoc.ToString()}");
                Request = new MoveMobilePacket(newLoc, Client.PlayerId);
                return;
            }
        }

        private void Move(string position)
        {
            MobileDirections newDir;
            Regions newLoc;

            if (Client.CurrentRegion.Navigable)
                if ((newDir = Client.StringToDirection(position)) != MobileDirections.None)
                {
                    Console.WriteLine($"Selected: {newDir.ToString()}");
                    Request = new MoveMobilePacket(Client.CurrentRegion.Location, Client.PlayerId, newDir);
                    return;
                }

            if ((newLoc = Client.StringToLocation(position)) != Regions.None)
            {
                Console.WriteLine($"Selected: {newLoc.ToString()}");
                Request = new MoveMobilePacket(newLoc, Client.PlayerId);
                return;
            }

            Move();
        }

        /// <summary>
        ///     Checks nearby locations.
        /// </summary>
        private static void Look()
        {
            if (Client.CurrentRegion.Navigable)
            {
                // Print our directions since we can move within this map.
                Console.WriteLine(" Directions to travel locally:");
                foreach (MobileDirections dir in Enum.GetValues(typeof(MobileDirections)))
                {
                    if (dir == MobileDirections.None) continue;

                    Console.WriteLine($"  {Enum.GetName(typeof(MobileDirections), dir)}");
                }

                Console.WriteLine();
            }

            var pos = 0;
            Console.WriteLine(" Nearby Locations:");
            foreach (var n in Utility.EnumToIEnumerable<Regions>(Client.CurrentRegion.Connections, true))
            {
                ++pos;
                Console.WriteLine($"  [Pos: {pos}] {Enum.GetName(typeof(Regions), n)}");
            }

            Console.WriteLine();
        }

        /// <summary>
        ///     Displays the last location the user visited.
        /// </summary>
        private static void LastLocation()
        {
            var last = Client.LastRegion.ToString();

            Console.WriteLine($" {last}");
        }

        /// <summary>
        ///     Makes a request to the Server for an updated list of players.
        ///     This function is called again on the list is returned by the server.
        /// </summary>
        /// <returns>List of Players from the server.</returns>
        private List<BaseMobile> GetMobiles()
        {
            if (Request != null) return Client.LocalMobiles.ToList();

            // Create a request for the server to respond to.
            Request = new GetMobilesPacket(Client.CurrentRegion.Location, Client.PlayerId);
            return null;
        }

        /// <summary>
        ///     Parent caller for retrieving either local Players or NPCs.
        /// </summary>
        /// <param name="type"></param>
        private void ListMobiles(MobileTypes type)
        {
            var mobiles = GetMobiles(); // Get a fresh list of mobiles from the server.
            if (mobiles == null && Request != null) return; // Return early to process a client request.

            Console.WriteLine($" Local {type.ToString()}s:");

            var pos = 0;
            if (mobiles != null && mobiles.Count > 0)
                foreach (var m in mobiles)
                {
                    if ((type & m.Type) != m.Type) continue;

                    pos++;
                    Console.WriteLine($"  [Pos: {pos}] {m.Name},  ID: {m.Serial}");
                }

            // If not Players found, print 'None'.
            if (pos == 0) Console.WriteLine("    => None.");

            Reset();
        }

        private BaseMobile SelectMobile(IReadOnlyList<BaseMobile> mobiles)
        {
            ListMobiles(MobileTypes.Mobile); // Retrieves our mobiles.

            int input;
            do
            {
                Console.Write(" Select a target you wish to attack: ");
                if (int.TryParse(Console.ReadLine(), out input))
                {
                    if (input - 1 >= 0 && input - 1 < mobiles.Count) return mobiles[input - 1];

                    Utility.ConsoleNotify("Bad option, please try again.");
                }
            } while (input - 1 < 0 || input - 1 >= mobiles.Count);

            Utility.ConsoleNotify("We shouldn't have gotten here...");
            return Client.Account;
        }

        private void Attack(string target)
        {
            if (target.ToLower() == "last" || target.ToLower() == "l")
            {
                // Target the last mobile.
                var attackAction = new CombatMobilePacket(Client.PlayerId);
                attackAction.AddTarget(Client.LastTarget);
                Request = attackAction;
                return;
            }

            Attack();
        }

        /// <summary>
        ///     Initiates an attack on a mobile.
        /// </summary>
        private void Attack()
        {
            var mobiles = GetMobiles();
            if (mobiles == null) return; // Haven't made the request, making it now by returning early.

            if (mobiles.Count == 0)
            {
                Utility.ConsoleNotify("No mobs to attack.");
                Reset();
                return;
            }

            Client.LastTarget = SelectMobile(mobiles);

            // Our newly created action to perform.
            var attackAction = new CombatMobilePacket(Client.PlayerId);
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
            var c = 0;
            foreach (var action in Enum.GetNames(typeof(ConsoleActions)))
            {
                if (c != 0 && c % 6 == 0) Console.Write("\n");

                Console.Write($"[{action.ToLower()}]  ");
                c++;
            }

            Console.WriteLine();
        }

        /// <summary>
        ///     Exits the client, sends the server a request to kill the socket.
        /// </summary>
        private void Exit()
        {
            Request = new SocketKillPacket(Client.PlayerId);
        }

        private enum ConsoleActions
        {
            None,
            Move,
            Look,
            LastLoc,
            Players,
            Npcs,
            Mobiles,
            Attack,
            Actions,
            Use,
            Update,
            Paperdoll,
            Exit
        }

        #region Getters / Setters

        private static ClientState Client { get; set; }

        public Packet Request { get; private set; }

        private static ulong Rounds { get; set; }

        #endregion
    }
}