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
            newState.Location = GameObject.GetStartingZone();               // Assign the Starting Zone Node to the GameState.
            newState.moved = true;                                          // Player "moved" to the Starting Zone.
            GameObject.UpdateGameStates(ref newState);                      // Updates the GameObject with the new state that is being tracked.

            player.Location = GameObject.GetStartingZone().GetLocation();   // Assign the Starting Zone Location to the player.
            GameObject.UpdateMobiles(player);                               // Update our current Players with new Player.

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
                    MobileActionHandler(ref ma);
                    socketHandler.ToClient(ma.ToByte());
                    break;
                default:
                    Miscellaneous.ConsoleNotify("Bad Request received.");
                    break;
            }
        }

        /// <summary>
        ///     Handles actions that a mobile wants to perform. (recieved from client)
        /// </summary>
        private static void MobileActionHandler(ref MobileAction mobileAction)
        {
            Player initator = GameObject.FindPlayer(mobileAction.GetInitator()) as Player;
            if (initator == null)
            {
                Miscellaneous.ConsoleNotify("Bad MobileAcition recieved. No initiator provided.");
                // TODO: Pass mobileAction by reference? Return an error to the client?
                return;
            }

            if (mobileAction.Type == ActionType.Attack)
            {
                MobileActionHandlerAttack(ref initator, ref mobileAction);
            }
        }

        private static void MobileActionHandlerAttack(ref Player initator, ref MobileAction mobileAction)
        {
            HashSet<MobileModifier> updates = new HashSet<MobileModifier>();        // Will contain updates to be passed back to client.
            MobileModifier mm_initiator = new MobileModifier(initator);

            List<Tuple<MobileType, UInt64>> targets = mobileAction.GetTargets();    // List containing <Type, Serial>
            if (targets.Count == 0)
            {
                Miscellaneous.ConsoleNotify("Bad MobileAction recieved. No targets supplied.");
                // TODO: Pass mobileAction by reference? Return an error to the client?
                return;
            }

            // Iterate each of the affected.
            foreach (Tuple<MobileType, UInt64> t in targets)
            {   // Lookup the affected mobile.
                Mobile affectee = null;
                if (t.Item1 == MobileType.NPC)
                    affectee = GameObject.FindNPC(t.Item2) as Mobile;
                else if (t.Item1 == MobileType.Player)
                    affectee = GameObject.FindPlayer(t.Item2) as Mobile;

                if (affectee == null)
                {   // Double checks were processing on a correct object.
                    Miscellaneous.ConsoleNotify($"Bad MobileAction 'affectee': {t.Item1.ToString()}: {t.Item2}.");
                    continue;
                }

                MobileModifier mm_affectee = new MobileModifier(affectee);

                // Combat the two objects.
                initator.Combat(ref mm_initiator, ref affectee, ref mm_affectee);

                // Update our initiator.
                if (initator.IsDead())
                    GameObject.Kill(initator);
                else
                    GameObject.UpdateMobiles(initator);

                // Update the affectee.
                if (affectee.IsDead())
                    GameObject.Kill(affectee);
                else
                    GameObject.UpdateMobiles(affectee);

                // Update to pass to the client regarding the affectee.
                mobileAction.AddUpdate(mm_affectee);

                mobileAction.Result += $"{initator.m_Name} attacked {affectee.m_Name}. ";    // TODO: Move this to MobileModifier.
            }

            // Add our updated information for the initiator. After processing all changes.
            mobileAction.AddUpdate(mm_initiator);
        }
    }
}
