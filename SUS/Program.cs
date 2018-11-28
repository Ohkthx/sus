using System;
using System.Net.Sockets;
using SUS.Server;
using SUS.Shared.Utilities;
using SUS.Shared.Packets;
using System.Timers;

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

            int timeout = 10000;
            int requests = 0;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();


            while (socketKill.Kill == false)
            {
                Object obj = socketHandler.FromClient();
                ++requests;

                if (stopwatch.ElapsedMilliseconds < timeout && requests > 5)
                {
                    socketHandler.ToClient(new ErrorPacket($"Server: You have exceeded 5 requests in {timeout / 1000} seconds and now on cooldown.").ToByte());
                    System.Threading.Thread.Sleep(timeout);
                    stopwatch.Reset();
                    requests = 0;
                }
                else if (stopwatch.ElapsedMilliseconds >= timeout)
                {
                    stopwatch.Reset();
                    requests = 0;
                }

                if (obj is Packet)
                {
                    Packet req = obj as Packet;

                    ServerInstance.Request(socketHandler, req); // If it is not a SocketKill, process it first.
                    if (req.Type == PacketTypes.SocketKill)
                        socketKill = req as SocketKillPacket;   // This will lead to termination.
                }
            }
        }
    }
}
