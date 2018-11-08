using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;
using SUS.Server;
using SUS.Shared.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace SUS
{
    class Program
    {
        static void Main(string[] args)
            => StartServer();

        // Initiates the server and its networking.
        static void StartServer()
        {
            Server.ServerInstance.StartListening();
        }

        public static void ClientHandler(ref Socket client)
        {
            SocketKill socketKill = new SocketKill(false);
            SocketHandler socketHandler = new SocketHandler(client, SocketHandler.Types.Client, debug: true);

            while (socketKill.killme == false)
            {
                Object obj = socketHandler.FromClient();

                if (obj is Request)
                {
                    Request req = obj as Request;

                    if (req.Type != RequestTypes.SocketKill)
                        ServerInstance.Request(socketHandler, req); // If it is not a SocketKill, process it.
                    else
                        socketKill = req.Value as SocketKill;       // This will lead to termination.
                }
            }
        }
    }
}
