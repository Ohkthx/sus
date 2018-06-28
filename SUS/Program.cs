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

        // Initiates the server and it's networking.
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

                if (obj is Authenticate)
                    ServerInstance.Authenticate(socketHandler, (Authenticate)obj);
                else if (obj is Player)
                    ServerInstance.Player(socketHandler, (Player)obj);
                else if (obj is GameState)
                    ServerInstance.GameState(socketHandler, (GameState)obj);
                else if (obj is Node)
                    ServerInstance.Node(socketHandler, (Node)obj);
                else if (obj is Request)
                    ServerInstance.Request(socketHandler, (Request)obj);
                else if (obj is SocketKill)
                    socketKill = (SocketKill)obj;
            }
        }
    }
}
