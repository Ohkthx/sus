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
        public static void ServerHandler(ref Socket server)
        {
            UInt64 id;
            do {    // Get our User ID, ensure it is valid.
                Console.Write("Select an ID: ");
            } while (!UInt64.TryParse(Console.ReadLine(), out id));

            // Get our Username.
            Console.Write("Select a Username: ");
            var username = Console.ReadLine();

            // The Socket to communicate over to the server.
            SocketHandler socketHandler = new SocketHandler(server, SocketHandler.Types.Server, debug: DEBUG);

            // Authorizing with host.
            Authenticate auth = new Authenticate(id);

            if (DEBUG)
                Console.WriteLine("\n <= Sending Authentication.");

            // Send the authentication to the server.
            socketHandler.ToServer(auth.ToByte());

            GameState myGS = null;
            InteractiveConsole ia = null;

            // While we are recieving information from the server, continue to decipher and process it.
            for (object obj = null; (obj = socketHandler.FromClient()) != null; )
            {
                if (obj is Authenticate)
                {
                    if (DEBUG)
                        Console.WriteLine(" => Received Authenticate.");
                    Player player = new Player(id, username, 100);

                    if (DEBUG)
                        Console.WriteLine("\n <= Sending New Player.");
                    socketHandler.ToServer(player.ToByte());
                }
                else if (obj is GameState)
                {
                    myGS = (GameState)obj;

                    if (DEBUG)
                        Console.WriteLine(" => Received GameState of Player.\n");

                    Console.WriteLine($"\nInformation:\n [ Player: {myGS.Account.m_Name}, Location: {myGS.Location.Name} ]");

                    ia = new InteractiveConsole(myGS);
                }
                else if (obj is MobileAction)
                {
                    if (DEBUG)
                        Console.WriteLine(" => Recieved Action!");

                    MobileAction ma = obj as MobileAction;
                    Console.WriteLine($"\n Server Reponse: {ma.Result}");

                    foreach (Mobile m in ma.GetUpdates())
                    {   // Attempt to update the gamestate's account if it is in the list of updates.
                        if (!myGS.Refresh(m))
                        {   // Update failed.
                            if (m is NPC)
                            {
                                NPC npc = m as NPC;
                                Console.WriteLine($" Damage to {npc.m_Name}. Health: {npc.GetHealth()}.");
                            }
                        }
                    }
                    ia.Reset();
                }
                else if (obj is Request)
                {
                    if (DEBUG)
                        Console.WriteLine(" => Recieved Request.");
                }
                else if (obj is Node) 
                {
                    if (DEBUG)
                        Console.WriteLine(" => Received node.");
                    ia.LocationUpdater((Node)obj);
                }
                else if (obj is Player)
                {
                    if (DEBUG)
                        Console.WriteLine(" => Received Player.");
                }
                else if (obj is NPC)
                {
                    if (DEBUG)
                        Console.WriteLine(" => Received NPC.");
                }
                else if (obj is SocketKill)
                {
                    if (DEBUG)
                        Console.WriteLine(" => Received SocketKill.");
                    socketHandler.Kill();
                    break;
                }

                if (ia != null)
                {   // Get an action to perform and send it to the server.
                    myGS = ia.Core();   // Activates the interactive console to grab the next action desired to be performed.

                    if (ia.sendGameState)
                    {   // Send an updated Gamestate to the server.
                        if (DEBUG)
                            Console.WriteLine(" <= Sending Updated Gamestate.");
                        socketHandler.ToServer(myGS.ToByte());
                    }
                    else if (ia.clientRequest != null)
                    {   // Send an updated Request for information to the server.
                        if (DEBUG)
                            Console.WriteLine(" <= Sending request for current location.");

                        socketHandler.ToServer(ia.clientRequest.ToByte());
                    }
                    else
                    {   // Send a SocketKill to the server to close the socket down peacefully.
                        // TODO: Add in catches for signal interrupts and run this.
                        if (DEBUG)
                            Console.WriteLine(" <= Sending SocketKill.");
                        socketHandler.ToServer(ia.socketKill.ToByte());
                    }
                }
            }
        }
    }
}
