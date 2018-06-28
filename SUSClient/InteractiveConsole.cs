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
        private static GameState gs = null;
        public SocketKill socketKill = null;
        public Request clientRequest = null;    // Temporary storage for a request sent by the client.

        private int getEntityType = 0;
        public bool sendGameState = false;

        private enum ConsoleActions { move, look, lastloc, players, npcs, attack, actions, exit }

        public InteractiveConsole(GameState gamestate) { gs = gamestate; }

        public GameState Core()
        {
            // If we requested a location of players, process it first.
            if (this.clientRequest != null && this.clientRequest.Type == RequestTypes.Location)
            {
                this.getMobiles(getEntityType);
            }
            this.Reset();       // Reset our bools.


            List<string> ValidActions = new List<string>();
            string act = string.Empty;

            Console.Write("Valid Actions: ");
            foreach (string action in Enum.GetNames(typeof(ConsoleActions)))
            {
                ValidActions.Add(action.ToLower());
                Console.Write($"[{action.ToLower()}]  ");
            }
            Console.WriteLine();

            while (this.socketKill == null && sendGameState == false && clientRequest == null)
            {
                while (true)
                {
                    Console.Write("\nChoose an action: ");
                    act = Console.ReadLine().ToLower();

                    if (ValidActions.Contains(act))
                        break;
                }

                switch (act)
                {
                    case "move":
                        move();
                        break;
                    case "look":
                        look();
                        break;
                    case "lastloc":
                        lastloc();
                        break;
                    case "players":
                        getMobiles(0);
                        break;
                    case "npcs":
                        getMobiles(1);
                        break;
                    case "actions":
                        actions();
                        break;
                    case "attack":
                        attack();
                        break;
                    case "exit":
                        exit();
                        break;
                    default:
                        Console.WriteLine("Something occured with processing this action.");
                        break;
                }
            }

            return gs;
        }

        public void Reset()
        {
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
            this.getEntityType = type;
            if (this.clientRequest == null)
            {
                this.clientRequest = new Request(RequestTypes.Location, gs.Location);
                return;
            }

            int pos, amount;
            pos = amount = 0;

            if (type == 0)
            {
                Console.WriteLine($" Local Players:");
                amount = gs.Location.Players.Count();
            }
            else
            {
                Console.WriteLine($" Local NPCs:");
                amount = gs.Location.NPCs.Count();
            }

            if (amount > 0)
            {
                if (type == 0)
                {
                    foreach (Player p in gs.Location.Players)
                        if (p.m_ID != gs.ID())
                        {
                            pos++;
                            Console.WriteLine($"  [Pos: {pos}] {p.m_Name},  ID: {p.m_ID}");
                        }
                }
                else
                {
                    foreach (NPC p in gs.Location.NPCs)
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

        private void attack()
        {
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
