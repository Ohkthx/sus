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
        public MobileAction myAction = null;

        public bool sendGameState = false;
        public bool sendRequest = false;
        public bool locationRequest = false;
        public bool actionRequest = false;

        private enum ConsoleActions { move, look, lastloc, players, attack, actions, exit }

        public InteractiveConsole(GameState gamestate) { gs = gamestate; }

        public GameState Core()
        {
            // If we requested a location of players, process it first.
            if (this.locationRequest)
                this.players();
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

            while (this.socketKill == null && sendGameState == false && sendRequest == false && actionRequest == false)
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
                        players();
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
            this.sendRequest = false;
            this.locationRequest = false;
            this.actionRequest = false;
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

        private void players()
        {
            // Send our request if we haven't.
            if (!this.locationRequest)
            {
                this.sendRequest = true;
                this.locationRequest = true;
                return;
            }

            Console.WriteLine($" Local Players:");

            int pos = 0;
            if (gs.Location.Players.Count() > 0)
            {
                foreach (Player p in gs.Location.Players)
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

        private void attack()
        {
            Console.WriteLine("Performing an attack on self!");
            this.myAction = new MobileAction(gs.Account.m_ID);
            this.actionRequest = true;
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
