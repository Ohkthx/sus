using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SUS.Shared.Utilities;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.Packets;
using SUSClient.Client;

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
            {
                if (arg.ToLower() == "-debug")
                {
                    Console.WriteLine("Found debug..");
                    DEBUG = true;
                }
            }

            Console.WriteLine($"Version: {GameState.Version}");

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

            // Send the authentication to the server.
            socketHandler.ToServer(new AccountAuthenticatePacket(id, username).ToByte());

            ServerHandler(ref socketHandler, id, username);
        }

        private static void ServerHandler(ref SocketHandler socketHandler, ulong id, string username)
        {
            GameState gamestate = null;     // Gamestate of this client.
            InteractiveConsole ia = null;   // Interactive console tracks user actions and sends data.
            Packet creq = null;            // Client REQuest. Used by functions not called in interactive console. 

            // While we are recieving information from the server, continue to decipher and process it.
            for (object obj = null; (obj = socketHandler.FromClient()) != null;)
            {
                creq = null;
                Packet req = obj as Packet;

                switch (req.Type)
                {
                    case PacketTypes.OK:
                        ia.Reset();
                        break;  // Server sent back empty information.
                    case PacketTypes.Error:
                        Utility.ConsoleNotify((req as ErrorPacket).Message);
                        ia.Reset();
                        break;
                    case PacketTypes.Authenticate:
                        ia = new InteractiveConsole(new GameState((req as AccountAuthenticatePacket).Player));
                        break;
                    case PacketTypes.GameState:
                        ia = new InteractiveConsole((req as AccountGameStatePacket).GameState);
                        break;
                    case PacketTypes.SocketKill:
                        Console.WriteLine("Socket Kill sent by server.");
                        socketHandler.Kill();
                        break;


                    case PacketTypes.GetLocalMobiles:
                        gamestate.Mobiles = (req as GetMobilesPacket).Mobiles;
                        break;
                    case PacketTypes.GetMobile:
                        gamestate.ParseGetMobilePacket(req);
                        ia.Reset();
                        break;
                    case PacketTypes.GetNode:
                        ia.LocationUpdater((req as GetNodePacket).NewLocation);
                        ia.Reset();
                        break;


                    case PacketTypes.MobileCombat:
                        gamestate.MobileActionHandler(req as CombatMobilePacket);
                        ia.Reset();
                        break;
                    case PacketTypes.MobileMove:
                        ia.LocationUpdater((req as MoveMobilePacket).NewLocation);
                        ia.Reset();
                        break;
                    case PacketTypes.MobileResurrect:
                        creq = gamestate.Ressurrect(req as RessurrectMobilePacket);   // If we require a new current node,
                        ia.Reset();                                             //  the request will be made and sent early.
                        break;
                    case PacketTypes.UseItem:
                        gamestate.UseItemResponse(req as UseItemPacket);
                        ia.Reset();
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

                    if (ia.clientRequest != null)
                        creq = ia.clientRequest;

                    // Check creq again for material to send to the server.
                    if (creq != null)
                    {
                        socketHandler.ToServer(creq.ToByte());
                        if (creq.Type == PacketTypes.SocketKill)
                            Environment.Exit(0); // Kill the application after informing the server.
                    }
                }
            }
        }
    }
}
