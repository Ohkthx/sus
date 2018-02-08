using SUS.Shared.Utility;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.SQLite;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUS.Server
{
    public partial class ServerInstance
    {
        public static void Authenticate(SocketHandler socketHandler, Authenticate auth)
        {
            // Client initated Authenticate, look up and verify information.
            // if not information found, prompt to create a new gamestate.
            var go = GameObject.Instance;
            GameState gamestate = go.GetGameState(auth.ID);

            // Send our response if no player is found, else send the client their GameState.
            if (gamestate == null)
                socketHandler.ToClient(auth.ToByte());
            else
                socketHandler.ToClient(gamestate.ToByte());
        }

        public static void Player(SocketHandler socketHandler, Player player)
        {
            // Client has sent a player, create a proper gamestate and send it to the client.
            GameState newState = new GameState(player);
            newState.Location = GameObject.Instance.GetStartingZone();
            newState.moved = true;
            GameObject.Instance.UpdateGameStates(ref newState);

            socketHandler.ToClient(newState.ToByte());
        }

        public static void GameState(SocketHandler socketHandler, GameState gameState)
        {
            // TODO: Update user locally and reflect any other updates back to the client.
            //  Like player locations, enemy locations, etc.
            GameObject.Instance.UpdateGameStates(ref gameState);

            socketHandler.ToClient(gameState.Location.ToByte());
        }

        public static void Node(SocketHandler socketHandler, Node node) { }

        public static void Request(SocketHandler socketHandler, Request req)
        {
            switch (req.Type)
            {
                case RequestTypes.location:
                    Node n = (Node)req.Value;
                    socketHandler.ToClient(GameObject.Instance.GetNode(n.ID).ToByte());
                    break;
                default:
                    Console.WriteLine(" [ERR] Bad Request recieved.");
                    break;
            }
        }
    }
}
