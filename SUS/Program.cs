using System;
using System.Net.Sockets;
using SUS.Server;
using SUS.Shared.Utilities;

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
            SocketKill socketKill = new SocketKill(null, false);
            SocketHandler socketHandler = new SocketHandler(client, SocketHandler.Types.Client, debug: true);

            while (socketKill.killme == false)
            {
                Object obj = socketHandler.FromClient();

                if (obj is Request)
                {
                    Request req = obj as Request;

                    ServerInstance.Request(socketHandler, req); // If it is not a SocketKill, process it.
                    if (req.Type == RequestTypes.SocketKill)
                        socketKill = req.Value as SocketKill;       // This will lead to termination.
                }
            }
        }
    }
}
