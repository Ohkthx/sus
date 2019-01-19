using SUS.Shared;
using SUS.Shared.Packets;
using System;
using System.Net.Sockets;

namespace SUS.Server
{
    internal static class Program
    {
        private static void Main()
        {
            StartServer();
        }

        // Initiates the server and its networking.
        private static void StartServer()
        {
            ServerInstance.StartListening();
        }

        public static void ClientHandler(ref Socket client)
        {
            var socketKill = new SocketKillPacket(0, false);
            var ch = new ClientHandler(new SocketHandler(client, SocketHandler.Types.Client, debug: true));

            var requests = 0;
            const int requestCap = 15;
            var timer = new Timer(5, Timer.Formats.Seconds);
            timer.Start();


            while (socketKill != null && socketKill.Kill == false)
            {
                var obj = ch.Handler.FromClient();

                #region Check Timeout
                if (timer.Completed)
                {
                    timer.Restart();
                    requests = 0;
                }
                else if (!timer.Completed && requests >= requestCap)
                {
                    ch.Handler.ToClient(new ErrorPacket($"Server: You have exceeded {requestCap} requests in {timer.Limit / 1000} seconds and now on cooldown.").ToByte());
                    System.Threading.Thread.Sleep(timer.Limit * 3);
                    timer.Restart();
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
                    if (obj is Packet req)
                    {
                        ch.Parser(req); // If it is not a SocketKill, process it first.
                        if (req.Type == PacketTypes.SocketKill)
                        {
                            socketKill = req as SocketKillPacket;   // This will lead to termination.
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Caught an exception: {e.Message}");
                    var skp = new SocketKillPacket(0);  // Create a new packet.
                    ch.Handler.ToClient(skp.ToByte());                           // Send it to our client for a clean connection.
                    socketKill = skp;                                               // Assign our local to break the loop.
                }
            }
        }
    }
}
