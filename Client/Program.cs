﻿using System;
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
        ///     Begins the exchange of information, authentication, and further processing of all input to and from the server.
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
                var console = new InteractiveConsole(new SocketHandler(toServer, SocketHandler.Types.Server, _debug));

                // Send the authentication to the server.
                console.ToServer(new AccountAuthenticatePacket(id, username));

                // Handles the client's connection to the server with packet parsing.
                ServerHandler(console);
            }
            catch (InvalidSocketHandlerException she)
            {
                Utility.ConsoleNotify(she.Message);
            }
            catch (InvalidPacketException ipe)
            {
                Utility.ConsoleNotify($"Received a bad packet: {ipe.Message}");
            }
            catch (ArgumentNullException ane)
            {
                Utility.ConsoleNotify($"Invalid argument passed: {ane.Message}");
            }
            catch (Exception e)
            {
                Utility.ConsoleNotify($"Unknown error occurred: \n{e}");
            }
        }

        /// <summary>
        ///     The loop the sends and receives information to the remote connection until closed..
        /// </summary>
        private static void ServerHandler(InteractiveConsole console)
        {
            // While we are receiving information from the server, continue to decipher and process it.
            for (Packet serverPacket; (serverPacket = console.FromServer()) != null;)
            {
                try
                {
                    // Attempt to parse the received packet.
                    if (console.PacketParser(serverPacket, out var clientRequest))
                    {
                        // If the packet was modified and requires to be resend, do so.
                        console.ToServer(clientRequest);
                        continue;
                    }

                    // Activates the interactive console to grab the next action desired to be performed.
                    clientRequest = console.Core();
                    if (clientRequest == null)
                        continue;

                    // Sends the information from the players action to the remote connection.
                    console.ToServer(clientRequest);

                    // If we are closing the connection, exit.
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