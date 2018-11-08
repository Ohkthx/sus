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
        /// <summary>
        ///     Processes and handles requests made by the client for server/gamestate information.
        ///     Responsible for also gathering and returning requested information to the client.
        /// </summary>
        /// <param name="socketHandler">Socket Handler to act upon.</param>
        /// <param name="req">Request Object (includes types.)</param>
        public static void Request(SocketHandler socketHandler, Request req)
        {
            Request clientInfo;

            switch (req.Type)
            {
                case RequestTypes.Authenticate:
                    clientInfo = Authenticate(req.Value as Authenticate);
                    break;
                case RequestTypes.GameState:
                    clientInfo = GameState(req.Value as GameState);
                    break;
                case RequestTypes.Mobile:
                    clientInfo = Mobile(req.Value as Mobile);
                    break;
                case RequestTypes.MobileAction:
                    clientInfo = MobileActionHandler(req.Value as MobileAction);
                    break;
                case RequestTypes.Node:
                    clientInfo = Node(req.Value as Node);
                    break;
                case RequestTypes.Player:
                    clientInfo = Player(req.Value as Player);
                    break;
                case RequestTypes.Resurrection:
                    clientInfo = Ressurrect(req.Value as Mobile);
                    break;
                default:
                    // Perhaps use "error" RequestType here.
                    clientInfo = null;
                    Miscellaneous.ConsoleNotify("Bad Request received.");
                    break;
            }

            if (clientInfo != null)
                socketHandler.ToClient(clientInfo.ToByte());
        }

        private static Request Authenticate(Authenticate auth)
        {
            // Client initated Authenticate, look up and verify information.
            // if not information found, prompt to create a new gamestate.
            GameState gamestate = GameObject.FindGameState(auth.ID);

            // Send our response if no player is found, else send the client their GameState.
            if (gamestate == null)
                return new Request(RequestTypes.Authenticate, auth);
            else
                return new Request(RequestTypes.GameState, gamestate);
        }

        private static Request GameState(GameState gameState)
        {
            // TODO: Update user locally and reflect any other updates back to the client.
            //  Like player locations, enemy locations, etc.
            GameObject.UpdateGameStates(ref gameState);

            return new Request(RequestTypes.Node, gameState.Location);
        }

        private static Request Mobile(Mobile mobile) { return null; }

        /// <summary>
        ///     Handles actions that a mobile wants to perform. (recieved from client)
        /// </summary>
        private static Request MobileActionHandler(MobileAction mobileAction)
        {
            Player initator = GameObject.FindPlayer(mobileAction.GetInitator()) as Player;
            if (initator == null)
            {
                Miscellaneous.ConsoleNotify("Bad MobileAcition recieved. No initiator provided.");
                // TODO: Pass mobileAction by reference? Return an error to the client?
                return null;
            }

            if (mobileAction.Type == ActionType.Attack)
            {
                MobileActionHandlerAttack(initator, ref mobileAction);
                return new Request(RequestTypes.MobileAction, mobileAction);
            }
            return null;
        }

        private static void MobileActionHandlerAttack(Player initiator, ref MobileAction mobileAction)
        {
            HashSet<MobileModifier> updates = new HashSet<MobileModifier>();        // Will contain updates to be passed back to client.
            MobileModifier mm_initiator = new MobileModifier(initiator);

            List<Tuple<MobileType, UInt64>> targets = mobileAction.GetTargets();    // List containing <Type, Serial>
            if (targets.Count == 0)
            {
                Miscellaneous.ConsoleNotify("Bad MobileAction recieved. No targets supplied.");
                // TODO: Pass mobileAction by reference? Return an error to the client?
                return;
            }

            // Iterate each of the affected.
            foreach (Tuple<MobileType, UInt64> t in targets)
            {
                if (initiator.IsDead())
                    break;  // Break early because the initiator is dead.

                // Lookup the affected mobile.
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
                initiator.Combat(ref mm_initiator, ref affectee, ref mm_affectee);

                // Update our initiator.
                if (initiator.IsDead())
                    GameObject.Kill(initiator);
                else
                    GameObject.UpdateMobiles(initiator);

                // Update the affectee.
                if (affectee.IsDead())
                    GameObject.Kill(affectee);
                else
                    GameObject.UpdateMobiles(affectee);

                // Update to pass to the client regarding the affectee.
                mobileAction.AddUpdate(mm_affectee);

                mobileAction.Result += $"{initiator.m_Name} attacked {affectee.m_Name}. ";    // TODO: Move this to MobileModifier.
            }

            // Add our updated information for the initiator. After processing all changes.
            mobileAction.AddUpdate(mm_initiator);
        }

        private static Request Node(Node node)
        {
            Node n = GameObject.FindNode(node.ID);  // Fetch a new or updated node.
            return new Request(RequestTypes.Node, n);
        }

        private static Request Player(Player player)
        {
            // Client has sent a player, create a proper gamestate and send it to the client.
            GameState newState = new GameState(player);
            newState.Location = GameObject.GetStartingZone();               // Assign the Starting Zone Node to the GameState.
            newState.moved = true;                                          // Player "moved" to the Starting Zone.
            GameObject.UpdateGameStates(ref newState);                      // Updates the GameObject with the new state that is being tracked.

            player.Location = GameObject.GetStartingZone().GetLocation();   // Assign the Starting Zone Location to the player.
            GameObject.UpdateMobiles(player);                               // Update our current Players with new Player.

            return new Request(RequestTypes.GameState, newState);
        }

        private static Request Ressurrect(Mobile mobile)
        {
            if (mobile == null)
            {
                Miscellaneous.ConsoleNotify("Ressurrection received a null mobile.");
                return null;
            }

            mobile.Ressurrect();
            mobile.Location = GameObject.GetStartingZone().GetLocation();

            Ressurrect r = new Ressurrect(GameObject.GetStartingZone(), mobile);
            return new Request(RequestTypes.Resurrection, r);
        }
    }
}
