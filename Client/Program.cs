using System;
using System.Net.Sockets;
using SUS.Shared;
using SUS.Shared.Packets;

namespace SUS.Client
{
    internal static class Program
    {
        private static bool _debug;

        public static void Main(string[] args)
        {
            StartUp(args);
        }

        /// <summary>
        ///     Launches the client, parses all arguments passed at launch.
        /// </summary>
        /// <param name="args"></param>
        private static void StartUp(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            foreach (var arg in args)
            {
                if (arg.ToLower() != "-debug")
                    continue;

                Console.WriteLine("Found debug..");
                _debug = true;
            }

            try
            {
                var toServer = AsynchronousClient.StartClient();
                if (toServer == null)
                    throw new InvalidSocketHandlerException("Unable to initiate the connection to the server.");

                // Connect and loop until error or exit the session.
                ServerConnect(toServer);

                Utility.ConsoleNotify("Ending session with server.");

                // Release the socket.  
                toServer.Shutdown(SocketShutdown.Both);
                toServer.Close();
            }
            catch (InvalidSocketHandlerException she)
            {
                Utility.ConsoleNotify(she.Message);
            }
            catch (Exception e)
            {
                Utility.ConsoleNotify($"An unknown error occurred: \n{e}");
            }

            Utility.ConsoleNotify("The session has ended.");
            Console.Read();
        }

        /// <summary>
        ///     Begins the exchange of information, authentication, and further processing of
        ///     all input to and from the server.
        /// </summary>
        /// <param name="toServer">Socket to communicate to the Server.</param>
        public static void ServerConnect(Socket toServer)
        {
            ulong id;
            do
            {
                // Get our User ID, ensure it is valid.
                Console.Write("Select an ID: ");
            } while (!ulong.TryParse(Console.ReadLine(), out id));

            // Get our Username.
            Console.Write("Select a Username: ");
            var username = Console.ReadLine();

            try
            {
                // The Socket to communicate over to the server.
                InteractiveConsole.SetHandler(new SocketHandler(toServer, SocketHandler.Types.Server, _debug));

                // Send the authentication to the server.
                InteractiveConsole.ToServer(new AccountAuthenticatePacket(id, username));

                // Handles the client's connection to the server with packet parsing.
                ServerHandler();
            }
            catch (InvalidSocketHandlerException she)
            {
                Utility.ConsoleNotify(she.Message);
            }
            catch (InvalidPacketException ipe)
            {
                Utility.ConsoleNotify($"Received a bad packet: {ipe.Message}");
            }
            catch (Exception e)
            {
                Utility.ConsoleNotify($"Unknown error occurred: \n{e}");
            }
        }

        private static void ServerHandler()
        {
            // While we are receiving information from the server, continue to decipher and process it.
            for (Packet serverPacket; (serverPacket = InteractiveConsole.FromServer()) != null;)
            {
                try
                {
                    var clientRequest = InteractiveConsole.PacketParser(serverPacket);
                    if (clientRequest != null)
                    {
                        InteractiveConsole.ToServer(clientRequest);
                        continue;
                    }

                    // Activates the interactive console to grab the next action desired to be performed.
                    clientRequest = InteractiveConsole.Core();
                    if (clientRequest == null)
                        continue;

                    InteractiveConsole.ToServer(clientRequest);
                    if (clientRequest is SocketKillPacket)
                        Environment.Exit(0); // Kill the application after informing the server.
                }
                catch (Exception e)
                {
                    Utility.ConsoleNotify("[Client Error] " + e.Message);
                }
            }
        }
    }
}