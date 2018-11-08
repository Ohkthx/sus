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
                case RequestTypes.MobileMove:
                    clientInfo = MobileMove(req.Value as MobileMove);
                    break;
                case RequestTypes.Node:
                    clientInfo = Node((int)req.Value);
                    break;
                case RequestTypes.Player:
                    clientInfo = Player(req.Value as Player);
                    break;
                case RequestTypes.Resurrection:
                    clientInfo = Ressurrect(req.Value as Mobile);
                    break;
                default:
                    // Perhaps use "error" RequestType here.
                    clientInfo = new Request(RequestTypes.Error, "Server: Bad request received.");
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
                return new Request(RequestTypes.Error, "Server: Bad initiator provided for action.");

            if (mobileAction.Type == ActionType.Attack)
            {
                Request req = MobileActionHandlerAttack(initator, ref mobileAction);
                if (req != null)    // If Request is not null, that means an error occured in MobileActionHandlerAttack.
                    return req;     // Return the error to the client.
                return new Request(RequestTypes.MobileAction, mobileAction);    // Create a new Request with updated information.
            }

            return null;
        }

        /// <summary>
        ///     Performs the lookup and combating of mobiles.
        /// </summary>
        /// <param name="initiator">Initiating Mobile.</param>
        /// <param name="mobileAction">Reference to the MobileAction to send back.</param>
        /// <returns>Packaged Error if occurred, otherwise should normally return null.</returns>
        private static Request MobileActionHandlerAttack(Player initiator, ref MobileAction mobileAction)
        {
            HashSet<MobileModifier> updates = new HashSet<MobileModifier>();        // Will contain updates to be passed back to client.
            MobileModifier mm_initiator = new MobileModifier(initiator);

            List<Mobile> mobiles = new List<Mobile>();                              // This will hold all good mobiles.
            List<Tuple<MobileType, UInt64>> targets = mobileAction.GetTargets();    // List containing <Type, Serial>
            if (targets.Count == 0)
                return new Request(RequestTypes.Error, "Server: No targets provided for attacking.");

            // Iterate each of the affected, adding it to our list of Mobiles.
            foreach (Tuple<MobileType, UInt64> t in targets)
            {
                // Lookup the affected mobile.
                Mobile affectee = null;
                if (t.Item1 == MobileType.NPC)
                    affectee = GameObject.FindNPC(t.Item2) as Mobile;
                else if (t.Item1 == MobileType.Player)
                    affectee = GameObject.FindPlayer(t.Item2) as Mobile;

                if (affectee == null)
                    return new Request(RequestTypes.Error, "Server: That target is already dead.");

                mobiles.Add(affectee);  // Add it to our list of known "good" mobiles.
            }

            // Iterate our mobiles and perform combat.
            foreach (Mobile m in mobiles)
            {
                Mobile target = m;

                MobileModifier mm_affectee = new MobileModifier(target);

                // Combat the two objects.
                initiator.Combat(ref mm_initiator, ref target, ref mm_affectee);

                // Update our initiator.
                if (initiator.IsDead())
                    GameObject.Kill(initiator);
                else
                    GameObject.UpdateMobiles(initiator);

                // Update the affectee.
                if (target.IsDead())
                    GameObject.Kill(m);
                else
                    GameObject.UpdateMobiles(target);

                // Update to pass to the client regarding the affectee.
                mobileAction.AddUpdate(mm_affectee);

                mobileAction.Result += $"{initiator.m_Name} attacked {target.m_Name}. ";    // TODO: Move this to MobileModifier.
            }

            // Add our updated information for the initiator. After processing all changes.
            mobileAction.AddUpdate(mm_initiator);
            mobileAction.CleanClientInfo();         // Remove worthless data so it isn't retransmitted.
            return null;
        }

        /// <summary>
        ///     Moves a Mobile from one Node to another Node.
        /// </summary>
        /// <param name="mm">Node / Mobile pair to move.</param>
        /// <returns>Packed "OK" server response.</returns>
        private static Request MobileMove(MobileMove mm)
        {
            if (mm.Mobile.Location != mm.NodeID)
            {   // Remove the player from the old node.
                Node old = GameObject.FindNode((int)mm.Mobile.Location);
                if (old == null)
                    return new Request(RequestTypes.Error, "Server: Bad node requested.");
                old.RemoveMobile(mm.Mobile);    // Remove the mobile from the node.
                GameObject.UpdateNodes(old);    // Update the node.
            }

            Node node = GameObject.FindNode((int)mm.NodeID);
            if (node == null)
                return new Request(RequestTypes.Error, "Server: Bad node requested.");
            else if (!node.HasMobile(mm.Mobile))
            {   // The Node does not contain the mobile.
                node.AddMobile(mm.Mobile);      // Add the mobile to the Node.
                GameObject.UpdateNodes(node);   // Update the Node in the GameObject.
            }

            // Lastly, update the Mobile.
            GameObject.UpdateMobiles(mm.Mobile);

            return new Request(RequestTypes.OK, null);
        }

        /// <summary>
        ///     Returns a Node to the client.
        /// </summary>
        /// <param name="node">ID of the Node to retrieve.</param>
        /// <returns>Packaged Node.</returns>
        private static Request Node(int node)
        {
            Node n = GameObject.FindNode(node);  // Fetch a new or updated node.
            if (n == null)
                return new Request(RequestTypes.Error, "Server: Bad node requested.");

            return new Request(RequestTypes.Node, n);
        }

        /// <summary>
        ///     Receives a Player type from a client for authentication.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>Packaged GameState.</returns>
        private static Request Player(Player player)
        {
            // Client has sent a player, create a proper gamestate and send it to the client.
            GameState newState = new GameState(player);
            newState.Location = GameObject.GetStartingZone();               // Assign the Starting Zone Node to the GameState.
            newState.moved = true;                                          // Player "moved" to the Starting Zone.
            GameObject.UpdateGameStates(ref newState);                      // Updates the GameObject with the new state that is being tracked.

            player.Location = GameObject.GetStartingZone().GetLocation();   // Assign the Starting Zone Location to the player.
            GameObject.UpdateMobiles(player);                               // Update our tracked Mobiles with the new Player.

            return new Request(RequestTypes.GameState, newState);
        }

        /// <summary>
        ///     Brings a Mobile back to life and returns it to the client.
        /// </summary>
        /// <param name="mobile">Mobile to return to life.</param>
        /// <returns>Packaged Mobile.</returns>
        private static Request Ressurrect(Mobile mobile)
        {
            if (mobile == null)
                return new Request(RequestTypes.Error, "Server: Ressurrection target not provided.");

            Node sz = GameObject.GetStartingZone(); // Gets our starting zone.

            mobile.Ressurrect();
            mobile.Location = sz.GetLocation();

            Node n = GameObject.FindNode(sz.ID);  // Fetch a new or updated node.
            if (n == null)
                return new Request(RequestTypes.Error, "Server: Bad starting zone.");

            n.AddMobile(mobile);
            GameObject.UpdateNodes(n);          // Update our node locally on the server.

            Ressurrect r = new Ressurrect(n, mobile);
            return new Request(RequestTypes.Resurrection, r);
        }
    }
}
