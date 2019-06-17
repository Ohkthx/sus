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
        /// <param name="args"></param>
        private static void StartUp(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            foreach (var arg in args)
            {
                if (arg.ToLower() != "-debug") continue;

                Console.WriteLine("Found debug..");
                _debug = true;
            }

            AsynchronousClient.StartClient(); // Starts the Client.
            Console.Read();
        }

        /// <summary>
        ///     Begins the exchange of information, authentication, and further processing of
        ///     all input to and from the server.
        /// </summary>
        /// <param name="server">Socket to communicate to the Server.</param>
        public static void ServerConnect(ref Socket server)
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

            // The Socket to communicate over to the server.
            var socketHandler = new SocketHandler(server, SocketHandler.Types.Server, _debug);

            // Send the authentication to the server.
            socketHandler.ToServer(new AccountAuthenticatePacket(id, username).ToByte());

            ServerHandler(socketHandler);
        }

        private static void ServerHandler(SocketHandler socketHandler)
        {
            ClientState clientState = null; // Gamestate of this client.
            InteractiveConsole ia = null; // Interactive console tracks user actions and sends data.

            // While we are receiving information from the server, continue to decipher and process it.
            for (object obj; (obj = socketHandler.FromClient()) != null;)
            {
                Packet clientRequest = null; // Client REQuest. Used by functions not called in interactive console. 
                if (!(obj is Packet req))
                    continue;

                switch (req.Type)
                {
                    case PacketTypes.Ok:
                        if (req is OkPacket ok && ok.Message != string.Empty)
                            Utility.ConsoleNotify(ok.Message);
                        ia?.Reset();
                        break;
                    case PacketTypes.Error:
                        if (req is ErrorPacket err && err.Message != string.Empty)
                            Utility.ConsoleNotify(err.Message);
                        ia?.Reset();
                        break;
                    case PacketTypes.ClientState:
                        ia = new InteractiveConsole(((AccountClientPacket) req).ClientState);
                        break;
                    case PacketTypes.SocketKill:
                        Utility.ConsoleNotify("Socket Kill sent by server.");
                        if (req is SocketKillPacket skp && skp.Message != string.Empty)
                            Utility.ConsoleNotify("Reason: " + skp.Message);
                        socketHandler.Kill();
                        break;


                    case PacketTypes.GetLocalMobiles:
                        if (clientState != null)
                            clientState.LocalMobiles = ((GetMobilesPacket) req).Mobiles;
                        break;
                    case PacketTypes.GetMobile:
                        clientState?.ParseGetMobilePacket(req as GetMobilePacket);
                        ia?.Reset();
                        break;
                    case PacketTypes.GetNode:
                        if (ia != null)
                        {
                            clientState.CurrentRegion = ((GetNodePacket) req).NewRegion;
                            ia.Reset();
                        }

                        break;
                    case PacketTypes.MobileCombat:
                        clientState?.MobileActionHandler(req as CombatMobilePacket);
                        ia?.Reset();
                        break;
                    case PacketTypes.MobileMove:
                        if (clientState != null)
                            if (req is MoveMobilePacket mmp)
                            {
                                clientState.CurrentRegion = mmp.NewRegion; // Reassign our region.
                                if (mmp.DiscoveredRegion != Regions.None)
                                {
                                    // If the client discovered a new location, add it to our potential locations.
                                    clientState.AddUnlockedRegion(mmp.NewRegion.Connections);
                                    Console.WriteLine($"Discovered: {mmp.DiscoveredRegion}!");
                                }
                            }

                        ia?.Reset();
                        break;
                    case PacketTypes.MobileResurrect:
                        clientRequest =
                            clientState?.Resurrect(req as ResurrectMobilePacket); // If we require a new current node,
                        ia?.Reset(); //  the request will be made and sent early.
                        break;
                    case PacketTypes.UseItem:
                        ClientState.UseItemResponse(req as UseItemPacket);
                        ia?.Reset();
                        break;
                    case PacketTypes.UseVendor:
                        clientRequest = clientState?.UseVendorProcessor(req as UseVendorPacket);
                        ia?.Reset();
                        break;
                    case PacketTypes.Authenticate:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (clientRequest != null)
                {
                    // If clientRequest was assigned early, send it and loop again.
                    socketHandler.ToServer(clientRequest.ToByte());
                    continue;
                }

                if (ia == null) continue; // Get an action to perform and send it to the server.

                clientState =
                    ia.Core(); // Activates the interactive console to grab the next action desired to be performed.

                if (ia.Request != null)
                    clientRequest = ia.Request;

                // Check clientRequest again for material to send to the server.
                if (clientRequest == null) continue;

                socketHandler.ToServer(clientRequest.ToByte());
                if (clientRequest.Type == PacketTypes.SocketKill)
                    Environment.Exit(0); // Kill the application after informing the server.
            }
        }
    }
}