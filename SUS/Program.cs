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

            int timeout = 5000;
            int requests = 0;
            int requestCap = 15;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();


            while (socketKill.Kill == false)
            {
                Object obj = socketHandler.FromClient();

                #region Check Timeout
                if (stopwatch.ElapsedMilliseconds >= timeout)
                {
                    stopwatch.Restart();
                    requests = 0;
                }
                else if (stopwatch.ElapsedMilliseconds < timeout && requests >= requestCap)
                {
                    socketHandler.ToClient(new ErrorPacket($"Server: You have exceeded {requestCap} requests in {timeout / 1000} seconds and now on cooldown.").ToByte());
                    System.Threading.Thread.Sleep(timeout * 3);
                    stopwatch.Restart();
                    requests = 0;
                    continue;
                }
                else
                {
                    ++requests;
                }
                #endregion

                try
                {
                    if (obj is Packet)
                    {
                        Packet req = obj as Packet;

                        ServerInstance.Request(socketHandler, req); // If it is not a SocketKill, process it first.
                        if (req.Type == PacketTypes.SocketKill)
                            socketKill = req as SocketKillPacket;   // This will lead to termination.
                    }
                } catch (Exception e)
                {
                    Console.WriteLine($"Caught an exception: {e.Message}");
                    SocketKillPacket skp = new SocketKillPacket(null, kill: true);  // Create a new packet.
                    socketHandler.ToClient(skp.ToByte());                           // Send it to our client for a clean connection.
                    socketKill = skp;                                               // Assign our local to break the loop.
                }
            }
        }
    }
}
