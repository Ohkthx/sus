using System;
using System.Net.Sockets;
using SUS.Shared;
using SUS.Shared.Packets;
using SUSClient.Client;

namespace SUSClient
{
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
                switch (arg.ToLower())
                {
                    case "-debug":
                        Console.WriteLine("Found debug..");
                        DEBUG = true;
                        break;
                }
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

            // Send the authentication to the server.
            socketHandler.ToServer(new AccountAuthenticatePacket(id, username).ToByte());

            ServerHandler(socketHandler, id, username);
        }

        private static void ServerHandler(SocketHandler socketHandler, ulong id, string username)
        {
            ClientState clientstate = null;     // Gamestate of this client.
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
                    case PacketTypes.ClientState:
                        ia = new InteractiveConsole((req as AccountClientPacket).ClientState);
                        break;
                    case PacketTypes.SocketKill:
                        Console.WriteLine("Socket Kill sent by server.");
                        socketHandler.Kill();
                        break;


                    case PacketTypes.GetLocalMobiles:
                        clientstate.Mobiles = (req as GetMobilesPacket).Mobiles;
                        break;
                    case PacketTypes.GetMobile:
                        clientstate.ParseGetMobilePacket(req as GetMobilePacket);
                        ia.Reset();
                        break;
                    case PacketTypes.GetNode:
                        ia.LocationUpdater((req as GetNodePacket).NewRegion);
                        ia.Reset();
                        break;


                    case PacketTypes.MobileCombat:
                        clientstate.MobileActionHandler(req as CombatMobilePacket);
                        ia.Reset();
                        break;
                    case PacketTypes.MobileMove:
                        ia.LocationUpdater((req as MoveMobilePacket).NewRegion);
                        ia.Reset();
                        break;
                    case PacketTypes.MobileResurrect:
                        creq = clientstate.Ressurrect(req as RessurrectMobilePacket);   // If we require a new current node,
                        ia.Reset();                                             //  the request will be made and sent early.
                        break;
                    case PacketTypes.UseItem:
                        clientstate.UseItemResponse(req as UseItemPacket);
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
                    clientstate = ia.Core();   // Activates the interactive console to grab the next action desired to be performed.

                    if (ia.Request != null)
                        creq = ia.Request;

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
