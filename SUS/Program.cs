using System;
using System.Net.Sockets;
using SUS.Server;
using SUS.Shared.Utilities;
using SUS.Shared.Packets;

namespace SUS
{
    class Program
    {
        static void Main(string[] args)
            => StartServer();

        // Initiates the server and its networking.
        static void StartServer()
        {
            ServerInstance.StartListening();
        }

        public static void ClientHandler(ref Socket client)
        {
            SocketKillPacket socketKill = new SocketKillPacket(null, false);
            SocketHandler socketHandler = new SocketHandler(client, SocketHandler.Types.Client, debug: true);

            while (socketKill.Kill == false)
            {
                Object obj = socketHandler.FromClient();

                if (obj is Packet)
                {
                    Packet req = obj as Packet;

                    ServerInstance.Request(socketHandler, req); // If it is not a SocketKill, process it.
                    if (req.Type == PacketTypes.SocketKill)
                        socketKill = req as SocketKillPacket;       // This will lead to termination.
                }
            }
        }
    }
}
