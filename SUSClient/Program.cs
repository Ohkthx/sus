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

        static void StartUp(string []args)
        {
            foreach (string arg in args)
                if (arg.ToLower() == "-debug")
                {
                    Console.WriteLine("Found debug..");
                    DEBUG = true;
                }


            AsynchronousClient.StartClient();
            Console.Read();
        }

        public static void ServerHandler(ref Socket server)
        {
            ulong id;
            do {
                Console.Write("Select an ID: ");
            } while (!ulong.TryParse(Console.ReadLine(), out id));

            Console.Write("Select a Username: ");
            var username = Console.ReadLine();

            SocketHandler socketHandler = new SocketHandler(server, SocketHandler.Types.Server, debug: DEBUG);

            // Authorizing with host.
            Authenticate auth = new Authenticate(id);
            if (DEBUG)
                Console.WriteLine("\n <= Sending Authentication.");
            socketHandler.ToServer(auth.ToByte());

            GameState myGS = null;
            InteractiveConsole ia = null;

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

                    Console.WriteLine($" [ Player: {myGS.Account.m_Name}, Location: {myGS.Location.Name} ]\n");

                    ia = new InteractiveConsole(myGS);
                }
                else if (obj is Request)
                {
                    if (DEBUG)
                        Console.WriteLine(" => Recieved request.");
                }
                else if (obj is Node && ia != null)
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
                else if (obj is SocketKill)
                {
                    if (DEBUG)
                        Console.WriteLine(" => Received SocketKill");

                    socketHandler.Kill();
                    break;
                }

                if (ia != null)
                {
                    myGS = ia.Core();

                    if (ia.sendGameState)
                    {
                        if (DEBUG)
                            Console.WriteLine(" <= Sending Updated Gamestate.");
                        socketHandler.ToServer(myGS.ToByte());
                    }
                    else if (ia.sendRequest)
                    {
                        if (DEBUG)
                            Console.WriteLine(" <= Sending request for current location.");
                        Request req = new Request(RequestTypes.location, myGS.Location);
                        socketHandler.ToServer(req.ToByte());
                    }
                    else
                    {
                        if (DEBUG)
                            Console.WriteLine(" <= Sending SocketKill.");
                        socketHandler.ToServer(ia.socketKill.ToByte());
                    }
                }
            }
        }
    }
}
