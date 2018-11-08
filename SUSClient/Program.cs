using SUS.Shared.Utility;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;
using SUSClient.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SUSClient
{
    public class Message
    {
        public byte[] Data { get; set; }
    }

    class Program
    {
        private static bool DEBUG = false;

        static void Main(string[] args) 
            => StartUp(args);
        
        /// <summary>
        ///     Launches the client, parses all arguments passed at launch.
        /// </summary>
        /// <param name="args"></param>
        static void StartUp(string []args)
        {
            foreach (string arg in args)
                if (arg.ToLower() == "-debug")
                {
                    Console.WriteLine("Found debug..");
                    DEBUG = true;
                }


            AsynchronousClient.StartClient();   // Starts the Client.
            Console.Read();
        }

        /// <summary>
        ///     Begins the exchange of information, authentication, and further processing of
        ///     all input to and from the server.
        /// </summary>
        /// <param name="server">Socket to communicate to the Server.</param>
        public static void ServerConnect(ref Socket server)
        {
            UInt64 id;
            do
            {    // Get our User ID, ensure it is valid.
                Console.Write("Select an ID: ");
            } while (!UInt64.TryParse(Console.ReadLine(), out id));

            // Get our Username.
            Console.Write("Select a Username: ");
            var username = Console.ReadLine();

            // The Socket to communicate over to the server.
            SocketHandler socketHandler = new SocketHandler(server, SocketHandler.Types.Server, debug: DEBUG);

            // Authorizing with host.
            Authenticate auth = new Authenticate(id);

            // Send the authentication to the server.
            Request creq = new Request(RequestTypes.Authenticate, auth);
            socketHandler.ToServer(creq.ToByte());

            ServerHandler(ref socketHandler, id, username);
        }

        private static void ServerHandler(ref SocketHandler socketHandler, ulong id, string username)
        {
            GameState gamestate = null;     // Gamestate of this client.
            InteractiveConsole ia = null;   // Interactive console tracks user actions and sends data.
            Request creq = null;            // Client REQuest. Used by functions not called in interactive console. 

            // While we are recieving information from the server, continue to decipher and process it.
            for (object obj = null; (obj = socketHandler.FromClient()) != null;)
            {
                creq = null;
                Request req = obj as Request;

                switch (req.Type)
                {
                    case RequestTypes.Authenticate:
                        Player player = new Player(id, username, 100, 105, 35, 10);
                        creq = new Request(RequestTypes.Player, player);
                        break;
                    case RequestTypes.GameState:
                        ia = new InteractiveConsole(req.Value as GameState);
                        break;
                    case RequestTypes.Mobile:
                        gamestate.UpdateMobile(req.Value as Mobile);
                        ia.Reset();
                        break;
                    case RequestTypes.MobileAction:
                        MobileActionHandler(ref gamestate, req.Value as MobileAction);
                        ia.Reset();
                        break;
                    case RequestTypes.Node:
                        ia.LocationUpdater(req.Value as Node);
                        break;
                    case RequestTypes.Resurrection:
                    case RequestTypes.SocketKill:
                        socketHandler.Kill();
                        break;
                }

                if (creq != null)
                {   // If creq was assigned early, send it and reloop.
                    socketHandler.ToServer(creq.ToByte());
                    continue;
                }

                if (ia != null)
                {   // Get an action to perform and send it to the server.
                    gamestate = ia.Core();   // Activates the interactive console to grab the next action desired to be performed.

                    if (ia.sendGameState)
                        creq = new Request(RequestTypes.GameState, gamestate);
                    else if (ia.clientRequest != null)
                        creq = ia.clientRequest;
                    else
                    {   // Send a SocketKill to the server to close the socket down peacefully.
                        // TODO: Add in catches for signal interrupts and run this.
                        creq = new Request(RequestTypes.SocketKill, ia.socketKill);
                    }

                    // Check creq again for material to send to the server.
                    if (creq != null)
                        socketHandler.ToServer(creq.ToByte());
                }
            }
        }

        private static void MobileActionHandler(ref GameState gs, MobileAction ma)
        {
            Console.WriteLine($"\n Server Reponse: {ma.Result}");

            foreach (MobileModifier mm in ma.GetUpdates())
            {   // Attempt to update the gamestate with the modifications to the mobile.
                Mobile mobile;
                if (gs.UpdateMobile(mm, out mobile) && mobile != null)
                {
                    if (mobile.m_Type == MobileType.Player)
                        gs.Refresh(mobile);   // Update our gamestate with the new player information.

                    Console.WriteLine($"  => {mobile.m_Name}'s health was changed by {mm.ModHits}. " +
                        $"\n\tStamina was changed by {mm.ModStamina}." +
                        $"\n\tHealth: {mobile.GetHealth()}." +
                        $"\n\tDead? {mm.IsDead}");
                }
            }
        }
    }
}
