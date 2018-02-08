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
        static void Main(string[] args) 
            => StartUp();

        static void StartUp()
        {
            AsynchronousClient.StartClient();
            Console.Read();
        }

        public static void ServerHandler(ref Socket server)
        {
            for (int x = 0; x < 30; x++)
                Console.WriteLine($"{RandomImpl.NextDouble(), 2}");

            ulong id;
            do {
                Console.Write("Select an ID: ");
            } while (!ulong.TryParse(Console.ReadLine(), out id));

            Console.Write("Select a Username: ");
            var username = Console.ReadLine();

            SocketHandler socketHandler = new SocketHandler(server, SocketHandler.Types.Server);

            // Authorizing with host.
            Authenticate auth = new Authenticate(id);
            Console.WriteLine("\n <= Sending Authentication.");
            socketHandler.ToServer(auth.ToByte());

            GameState myGS = null;
            InteractiveConsole ia = null;

            for (object obj = null; (obj = socketHandler.FromClient()) != null; )
            {
                if (obj is Authenticate)
                {
                    Console.WriteLine(" => Received Authenticate.");
                    Player player = new Player(id, username, 100);
                    Console.WriteLine("\n <= Sending New Player.");
                    socketHandler.ToServer(player.ToByte());
                }
                else if (obj is GameState)
                {
                    myGS = (GameState)obj;
                    Console.WriteLine(" => Received GameState of Player.\n");
                    Console.WriteLine($" [ Player: {myGS.Account.m_Name}, Location: {myGS.Location.Name} ]\n");

                    ia = new InteractiveConsole(myGS);
                }
                else if (obj is Request)
                    Console.WriteLine(" => Recieved request.");
                else if (obj is Node && ia != null)
                {
                    Console.WriteLine(" => Received node.");
                    ia.LocationUpdater((Node)obj);
                }
                else if (obj is Player)
                    Console.WriteLine(" => Received Player.");
                else if (obj is SocketKill)
                {
                    Console.WriteLine(" => Received SocketKill");
                    socketHandler.Kill();
                    break;
                }

                if (ia != null)
                {
                    myGS = ia.Core();

                    if (ia.sendGameState)
                    {
                        Console.WriteLine(" <= Sending Updated Gamestate.");
                        socketHandler.ToServer(myGS.ToByte());
                    }
                    else if (ia.sendRequest)
                    {
                        Console.WriteLine(" <= Sending request for current location.");
                        Request req = new Request(RequestTypes.location, myGS.Location);
                        socketHandler.ToServer(req.ToByte());
                    }
                    else
                    {
                        Console.WriteLine(" <= Sending SocketKill.");
                        socketHandler.ToServer(ia.socketKill.ToByte());
                    }
                }
            }
        }
    }
}
