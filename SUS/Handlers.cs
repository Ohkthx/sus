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
            GameState gamestate = GameObject.FindGameState(auth.ID);

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
            newState.Location = GameObject.GetStartingZone();
            newState.moved = true;
            GameObject.UpdateGameStates(ref newState);

            // Add this player to our mobiles.
            GameObject.UpdateMobiles(player);

            socketHandler.ToClient(newState.ToByte());
        }

        public static void GameState(SocketHandler socketHandler, GameState gameState)
        {
            // TODO: Update user locally and reflect any other updates back to the client.
            //  Like player locations, enemy locations, etc.
            GameObject.UpdateGameStates(ref gameState);

            socketHandler.ToClient(gameState.Location.ToByte());
        }

        public static void Node(SocketHandler socketHandler, Node node) { }

        /// <summary>
        ///     Processes and handles requests made by the client for server/gamestate information.
        ///     Responsible for also gathering and returning requested information to the client.
        /// </summary>
        /// <param name="socketHandler">Socket Handler to act upon.</param>
        /// <param name="req">Request Object (includes types.)</param>
        public static void Request(SocketHandler socketHandler, Request req)
        {
            switch (req.Type)
            {
                case RequestTypes.Location:
                    Node n = (Node)req.Value;
                    socketHandler.ToClient(GameObject.FindNode(n.ID).ToByte());
                    break;
                case RequestTypes.MobileAction:
                    MobileAction ma = (MobileAction)req.Value;
                    MobileActionHandler(ma);
                    ma.Fulfilled = true;
                    socketHandler.ToClient(ma.ToByte());
                    break;
                default:
                    Console.WriteLine(" [ERR] Bad Request recieved.");
                    break;
            }
        }

        /// <summary>
        ///     Handles actions that a mobile wants to perform. (recieved from client)
        /// </summary>
        private static void MobileActionHandler(MobileAction mobileAction)
        {
            Player initator = GameObject.FindMobile(mobileAction.GetInitator()) as Player;
            if (initator == null)
            {
                Console.WriteLine(" [ERR] Bad MobileAcition recieved. No initiator provided.");
                // TODO: Pass mobileAction by reference? Return an error to the client?
                return;
            }

            if (mobileAction.Type == ActionType.Attack)
            {
                List<UInt64> targets = mobileAction.GetTargets();
                if (targets.Count == 0)
                {
                    Console.WriteLine(" [ERR] Bad MobileAction recieved. No targets supplied.");
                    // TODO: Pass mobileAction by reference? Return an error to the client?
                    return;
                }

                NPC firstTarget = GameObject.FindMobile(targets.ElementAt(0)) as NPC;
                Console.WriteLine(" [DEBUG] Attack type, Initiator: {0}, Target {1}", 
                    initator.m_Name, firstTarget.m_Name);
            }
        }
    }
}
