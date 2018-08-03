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

        private static GameState gs = null;
        public SocketKill socketKill = null;
        public Request clientRequest = null;    // Temporary storage for a request sent by the client.

        private ConsoleActions lastAction = ConsoleActions.none;
        public bool sendGameState = false;

        public InteractiveConsole(GameState gamestate) { gs = gamestate; }

        public GameState Core()
        {   // If we requested a location of players, process it first.
            if (lastAction != ConsoleActions.none)
                responseHandler();
            this.Reset();       // Reset our bools.

            Dictionary<string, ConsoleActions> ValidActions = new Dictionary<string, ConsoleActions>();

            Console.Write("Valid Actions: ");
            foreach (ConsoleActions action in Enum.GetValues(typeof(ConsoleActions)))
            {
                string name = Enum.GetName(typeof(ConsoleActions), action);
                ValidActions.Add(name, action);
                Console.Write($"[{name.ToLower()}]  ");
            }

            Console.WriteLine();

            while (this.socketKill == null && sendGameState == false && clientRequest == null)
            {
                ConsoleActions consoleAction = ConsoleActions.none;
                string act = string.Empty;

                while (true)
                {
                    Console.Write("\nChoose an action: ");
                    act = Console.ReadLine().ToLower();

                    if (ValidActions.TryGetValue(act, out consoleAction))
                    {
                        this.lastAction = consoleAction;
                        break;
                    }
                }

                switch (consoleAction)
                {
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
                        actions();
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

        public void Reset()
        {
            this.lastAction = ConsoleActions.none;
            this.sendGameState = false;
            this.clientRequest = null;
        }

        public void LocationUpdater(Node location)
        {
            gs.Location = location;
        }

        private void move()
        {
            look();
            do
                Console.Write("Select location: ");
            while (!gs.MoveTo(Console.ReadLine()));
            Console.WriteLine($" New Location: {gs.Location.Name}");
            this.sendGameState = true;
        }

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

        private void lastloc()
        {
            string last = string.Empty;
            if (gs.LocationLast == null)
                last = "None";
            else
                last = gs.LocationLast.Name;

            Console.WriteLine($" {last}");
        }

        private void getMobiles(int type)
        {   // Send our request if we haven't.
            if (type == 0)
                listPlayers(getPlayers());
            else
                listNPCs(getNPCs());
        }

        private void listPlayers(List<Player> players)
        {
            Console.WriteLine($" Local Players:");

            int pos = 0;
            if (players.Count() > 0)
            {
                foreach (Player p in players)
                {
                    if (p.m_ID != gs.ID())
                    {
                        pos++;
                        Console.WriteLine($"  [Pos: {pos}] {p.m_Name},  ID: {p.m_ID}");
                    }
                }
            }

            if (pos == 0)
                Console.WriteLine("    => None.");

            Console.WriteLine();
        }

        private List<Player> getPlayers()
        {
            if (this.clientRequest == null)
            {
                this.clientRequest = new Request(RequestTypes.Location, gs.Location);
                return null;
            }

            return gs.Location.Players.ToList();
        }

        private void listNPCs(List<NPC> npcs)
        {
            Console.WriteLine($" Local NPcs:");

            int pos = 0;
            if (npcs.Count() > 0)
            {
                foreach (NPC p in npcs)
                {
                    if (p.m_ID != gs.ID())
                    {
                        pos++;
                        Console.WriteLine($"  [Pos: {pos}] {p.m_Name},  ID: {p.m_ID}");
                    }
                }
            }

            if (pos == 0)
                Console.WriteLine("    => None.");

            Console.WriteLine();
        }

        private List<NPC> getNPCs()
        {
            if (this.clientRequest == null)
            {
                this.clientRequest = new Request(RequestTypes.Location, gs.Location);
                return null;
            }

            return gs.Location.NPCs.ToList();
        }

        private void attack()
        {
            List<NPC> npcs = getNPCs();
            if (npcs == null)
                return;         // Haven't made the request, making it now by returning early.

            Console.WriteLine("Performing an attack on self!");
            this.clientRequest = new Request(RequestTypes.MobileAction, new MobileAction(gs.Account.m_ID));
        }

        private void actions()
        {
            Console.Write(" Valid Actions: \n  ");
            foreach (string action in Enum.GetNames(typeof(ConsoleActions)))
                Console.Write($"[{action.ToLower()}]  ");
            Console.WriteLine();
        }

        private void exit()
        {
            socketKill = new SocketKill(true);
            Console.WriteLine(" SocketKill is set.");
        }

    }
}
