using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SUS.Server
{
    public static class ServerInstance
    {
        private const int ConsolePort = 8410;
        private const int PlayerPort = 8411;

        // Thread signal.  
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);

        public static void StartListening()
        {
            // Establish the local endpoint for the socket.  
            var ipHostInfo = Dns.GetHostEntry("localhost");
            var ipAddress = ipHostInfo.AddressList[0];

            var localConsole = new IPEndPoint(ipAddress, ConsolePort);
            var localEndPoint = new IPEndPoint(ipAddress, PlayerPort);

            // Create a TCP/IP socket.  
            var playerListener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            var consoleListener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                consoleListener.Bind(localConsole);
                consoleListener.Listen(100);

                playerListener.Bind(localEndPoint);
                playerListener.Listen(100);

                Console.WriteLine("[ Server Started ]");

                while (true)
                {
                    // Set the event to non-signaled state.  
                    AllDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    consoleListener.BeginAccept(
                        AcceptCallback,
                        consoleListener);

                    playerListener.BeginAccept(
                        AcceptCallback,
                        playerListener);


                    // Wait until a connection is made before continuing.  
                    AllDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            AllDone.Set();
            var listener = (Socket) ar.AsyncState;
            var socket = listener.EndAccept(ar);

            try
            {
                var handler = ((IPEndPoint) listener.LocalEndPoint).Port == PlayerPort
                    ? (Handler) new ClientHandler(socket)
                    : new ConsoleHandler(socket);

                handler.Core();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something happened within a client. {e.Message}");
            }
        }
    }
}