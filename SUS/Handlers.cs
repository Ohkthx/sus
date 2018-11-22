using System;
using System.Collections.Generic;
using SUS.Shared.Utilities;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;

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
                case RequestTypes.LocalMobiles:
                    clientInfo = LocalMobiles((Locations)req.Value);
                    break;
                case RequestTypes.Mobile:
                    clientInfo = Mobile(req.Value as MobileTag);
                    break;
                case RequestTypes.MobileAction:
                    clientInfo = MobileActionHandler(req.Value as MobileAction);
                    break;
                case RequestTypes.MobileMove:
                    clientInfo = MobileMove(req.Value as MobileMove);
                    break;
                case RequestTypes.Node:
                    clientInfo = Node((Locations)req.Value);
                    break;
                case RequestTypes.Player:
                    clientInfo = Player(req.Value as Player);
                    break;
                case RequestTypes.Resurrection:
                    clientInfo = Ressurrect(req.Value as MobileTag);
                    break;
                case RequestTypes.SocketKill:
                    Logout(req.Value as SocketKill);
                    clientInfo = null;
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
            GameObject.UpdateGameStates(gameState);

            return new Request(RequestTypes.Node, gameState.NodeCurrent);
        }

        private static Request LocalMobiles(Locations loc)
        {
            HashSet<MobileTag> tags = GameObject.FindMobiles(loc, MobileType.Mobile);
            return new Request(RequestTypes.LocalMobiles, tags);
        }

        /// <summary>
        ///     Looks up the Mobile and Node, removes the Mobile, updates our lists.
        /// </summary>
        /// <param name="sk">Socket Kill containing the User ID.</param>
        private static void Logout(SocketKill sk)
        {
            if (sk.UserID == null)
                return;             // No User ID? Just return.

            GameState gs = GameObject.FindGameState(sk.UserID);
            if (gs == null)
                return;             // No mobile by that User ID? Just return.

            gs.Account.Logout();

            GameObject.UpdateMobiles(gs.Account);
            GameObject.UpdateGameStates(gs, remove: true);
        }

        /// <summary>
        ///     Requests a mobile from the server, if it is found then it is returned.
        /// </summary>
        /// <param name="mobile">Type and ID of the mobile in the form of a MobileTag.</param>
        /// <returns>Either an Error Request or a Request containing the mobile.</returns>
        private static Request Mobile(MobileTag mobile)
        {
            if (mobile == null)
                return new Request(RequestTypes.Error, "Server: Bad mobile requested.");

            Mobile reqMobile = GameObject.FindMobile(mobile.Type, mobile.ID);
            if (reqMobile == null)
                return new Request(RequestTypes.Error, "Server: There is no such mobile anymore.");

            return new Request(RequestTypes.Mobile, reqMobile);
        }

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
            if (initiator.IsDead)
                return new Request(RequestTypes.Error, "Server: You are dead and need to ressurrect.");

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
                    return new Request(RequestTypes.Error, "Server: That target has moved or died recently.");
                else if (initiator.Location != affectee.Location)
                    return new Request(RequestTypes.Error, "Server: That target is no longer in the area.");
                else if (affectee.IsPlayer)
                {
                    Player p = affectee as Player;  
                    if (!p.isLoggedIn)
                        return new Request(RequestTypes.Error, "Server: That target has logged out recently.");
                }

                mobiles.Add(affectee);  // Add it to our list of known "good" mobiles.
            }

            // Iterate our mobiles and perform combat.
            foreach (Mobile m in mobiles)
            {
                if (initiator.IsDead)
                    break;

                Mobile target = m;

                MobileModifier mm_affectee = new MobileModifier(target);

                // Combat the two objects.
                initiator.Combat(ref mm_initiator, ref target, ref mm_affectee);

                // Update our initiator.
                if (initiator.IsDead)
                    GameObject.Kill(initiator);
                else
                    GameObject.UpdateMobiles(initiator);

                // Update the affectee.
                if (target.IsDead)
                    GameObject.Kill(m);
                else
                    GameObject.UpdateMobiles(target);

                // Update to pass to the client regarding the affectee.
                mobileAction.AddUpdate(mm_affectee);

                mobileAction.Result += $"{initiator.Name} attacked {target.Name}. ";    // TODO: Move this to MobileModifier.
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
            if (GameObject.MoveMobile(mm.Node, mm.Mobile))
                return new Request(RequestTypes.Node, GameObject.FindNode(mm.Node));

            return new Request(RequestTypes.Error, "Server: Did not move to the desired location.");
        }

        /// <summary>
        ///     Returns a Node to the client.
        /// </summary>
        /// <param name="node">ID of the Node to retrieve.</param>
        /// <returns>Packaged Node.</returns>
        private static Request Node(Locations loc)
        {
            Node n = GameObject.FindNode(loc);  // Fetch a new or updated node.
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
            Locations szLoc = GameObject.StartingZone;

            player.Location = szLoc; // Assign the Starting Zone Location to the player.
            player.Login();                     // Log the player in.

            // Client has sent a player, create a proper gamestate and send it to the client.
            GameState newState = new GameState(player);
            newState.NodeCurrent = GameObject.FindNode(szLoc); // Assign the Starting Zone Node to the GameState.

            GameObject.UpdateGameStates(newState);          // Updates the GameObject with the new state that is being tracked.
            GameObject.UpdateMobiles(player);               // Update our tracked Mobiles with the new Player.

            return new Request(RequestTypes.GameState, newState);
        }

        /// <summary>
        ///     Brings a Mobile back to life and returns it to the client.
        /// </summary>
        /// <param name="mobile">Mobile to return to life.</param>
        /// <returns>Packaged Mobile.</returns>
        private static Request Ressurrect(MobileTag mobile)
        {
            if (mobile == null)
                return new Request(RequestTypes.Error, "Server: Ressurrection target not provided.");

            // Sends the mobile to the StartingZone, ressurrects, and processes it as if an admin performed the action.
            bool ressurrected = GameObject.MoveMobile(GameObject.StartingZone, mobile, forceMove: true, ressurrection: true);
            Ressurrect r = new Ressurrect(GameObject.StartingZone, mobile, success: ressurrected);

            return new Request(RequestTypes.Resurrection, r);
        }
    }
}
