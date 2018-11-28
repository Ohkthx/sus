using System;
using System.Collections.Generic;
using SUS.Shared.Utilities;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Mobiles;
using SUS.Shared.Packets;

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
        public static void Request(SocketHandler socketHandler, Packet req)
        {
            Packet clientInfo;

            switch (req.Type)
            {
                case PacketTypes.Authenticate:
                    clientInfo = Authenticate(req as AccountAuthenticatePacket);
                    break;
                case PacketTypes.GameState:
                    clientInfo = GameState(req as AccountGameStatePacket);
                    break;
                case PacketTypes.SocketKill:
                    Logout(req as SocketKillPacket);
                    clientInfo = null;
                    break;


                case PacketTypes.GetLocalMobiles:
                    clientInfo = LocalMobiles(req as GetMobilesPacket);
                    break;
                case PacketTypes.GetMobile:
                    clientInfo = GetMobile(req as GetMobilePacket);
                    break;
                case PacketTypes.GetNode:
                    clientInfo = GetNode(req as GetNodePacket);
                    break;


                case PacketTypes.MobileCombat:
                    clientInfo = MobileActionHandler(req as CombatMobilePacket);
                    break;
                case PacketTypes.MobileMove:
                    clientInfo = MobileMove(req as MoveMobilePacket);
                    break;
                case PacketTypes.MobileResurrect:
                    clientInfo = MobileRessurrect(req as RessurrectMobilePacket);
                    break;


                default:
                    // Perhaps use "error" RequestType here.
                    clientInfo = new ErrorPacket("Server: Bad request received.");
                    break;
            }

            if (clientInfo != null)
                socketHandler.ToClient(clientInfo.ToByte());
        }

        #region Account Actions
        private static Packet Authenticate(AccountAuthenticatePacket auth)
        {
            // Client initated Authenticate, look up and verify information.
            // if not information found, prompt to create a new gamestate.
            GameState gamestate = GameObject.FindGameState(auth.Author.ID);

            // Send our response if no player is found, else send the client their GameState.
            if (gamestate == null)
            {
                return Player(new Player(auth.Author.ID, auth.Author.Name, 50, 35, 20));
            }

            AccountGameStatePacket gsp = new AccountGameStatePacket(auth.Author);
            gsp.GameState = gamestate;
            return gsp;
        }

        /// <summary>
        ///     Receives a Player type from a client for authentication.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>Packaged GameState.</returns>
        private static Packet Player(Player player)
        {
            Locations szLoc = GameObject.StartingZone;

            player.Location = szLoc;            // Assign the Starting Zone Location to the player.
            player.Login();                     // Log the player in.

            // Client has sent a player, create a proper gamestate and send it to the client.
            GameState newState = new GameState(player);

            Node loc = GameObject.FindNode(szLoc); // Assign the Starting Zone Node to the GameState.
            if (loc == null)
                return new ErrorPacket("Server: Invalid location to move to.");

            newState.NodeCurrent = loc.GetTag();

            GameObject.UpdateGameStates(newState);          // Updates the GameObject with the new state that is being tracked.
            GameObject.UpdateMobiles(player);               // Update our tracked Mobiles with the new Player.

            AccountGameStatePacket gsp = new AccountGameStatePacket(player.getTag());
            gsp.GameState = newState;
            return gsp;
        }


        private static Packet GameState(AccountGameStatePacket gsp)
        {
            GameObject.UpdateGameStates(gsp.GameState);

            return new OKPacket();
        }

        /// <summary>
        ///     Looks up the Mobile and Node, removes the Mobile, updates our lists.
        /// </summary>
        /// <param name="sk">Socket Kill containing the User ID.</param>
        private static void Logout(SocketKillPacket sk)
        {
            if (sk.Author == null)
                return;             // No User ID? Just return.

            GameState gs = GameObject.FindGameState(sk.Author.ID);
            if (gs == null)
                return;             // No mobile by that User ID? Just return.

            gs.Account.Logout();

            GameObject.UpdateMobiles(gs.Account);
            GameObject.UpdateGameStates(gs, remove: true);
        }
        #endregion

        #region Information Requests 
        private static Packet LocalMobiles(GetMobilesPacket gmp)
        {
            Mobile relativeMobile = GameObject.FindMobile(gmp.Author.Guid);
            if (relativeMobile == null || relativeMobile.Coordinate == null)
                return new ErrorPacket("Server: You are not in a location to get nearby objects.");

            HashSet<MobileTag> lm = GameObject.FindNearbyMobiles(gmp.Location, relativeMobile);
            gmp.Mobiles = lm;
            //gmp.Mobiles = GameObject.FindMobiles(gmp.Location, MobileType.Mobile);
            return gmp;
        }

        /// <summary>
        ///     Requests a mobile from the server, if it is found then it is returned.
        /// </summary>
        /// <param name="mobile">Type and ID of the mobile in the form of a MobileTag.</param>
        /// <returns>Either an Error Request or a Request containing the mobile.</returns>
        private static Packet GetMobile(GetMobilePacket gmp)
        {
            if (gmp.Tag == null)
                return new ErrorPacket("Server: Bad mobile requested.");

            gmp.Mobile = GameObject.FindMobile(gmp.Tag.Guid);
            if (gmp.Mobile == null)
                return new ErrorPacket("Server: There is no such mobile anymore.");

            return gmp;
        }

        /// <summary>
        ///     Returns a Node to the client.
        /// </summary>
        /// <param name="node">ID of the Node to retrieve.</param>
        /// <returns>Packaged Node.</returns>
        private static Packet GetNode(GetNodePacket gnp)
        {
            Node newLocation = GameObject.FindNode(gnp.Location);  // Fetch a new or updated node.
            if (newLocation == null)
                return new ErrorPacket("Server: Bad node requested.");

            gnp.NewLocation = newLocation.GetTag();
            return gnp;
        }
        #endregion

        #region Mobile Actions
        /// <summary>
        ///     Handles actions that a mobile wants to perform. (recieved from client)
        /// </summary>
        private static Packet MobileActionHandler(CombatMobilePacket mobileAction)
        {
            Player initator = GameObject.FindPlayer(mobileAction.Author.ID) as Player;
            if (initator == null)
                return new ErrorPacket("Server: Bad initiator provided for action.");

            Packet req = MobileActionHandlerAttack(initator, ref mobileAction);
            if (req != null)    // If Request is not null, that means an error occured in MobileActionHandlerAttack.
                return req;     // Return the error to the client.

            return mobileAction;    // Create a new Request with updated information.
        }

        /// <summary>
        ///     Performs the lookup and combating of mobiles.
        /// </summary>
        /// <param name="initiator">Initiating Mobile.</param>
        /// <param name="mobileAction">Reference to the MobileAction to send back.</param>
        /// <returns>Packaged Error if occurred, otherwise should normally return null.</returns>
        private static Packet MobileActionHandlerAttack(Player initiator, ref CombatMobilePacket mobileAction)
        {
            if (initiator.IsDead)
                return new ErrorPacket("Server: You are dead and need to ressurrect.");

            HashSet<MobileModifier> updates = new HashSet<MobileModifier>();        // Will contain updates to be passed back to client.
            MobileModifier mm_initiator = new MobileModifier(initiator);

            List<Mobile> mobiles = new List<Mobile>();                              // This will hold all good mobiles.
            List<MobileTag> targets = mobileAction.GetTargets();    // List containing <Type, Serial>
            if (targets.Count == 0)
                return new ErrorPacket("Server: No targets provided for attacking.");

            // Iterate each of the affected, adding it to our list of Mobiles.
            foreach (MobileTag mt in targets)
            {
                // Lookup the affected mobile.
                Mobile affectee = null;
                    affectee = GameObject.FindMobile(mt.Guid);

                if (affectee == null)
                    return new ErrorPacket("Server: That target has moved or died recently.");
                else if (initiator.Location != affectee.Location)
                    return new ErrorPacket("Server: That target is no longer in the area.");
                else if (affectee.IsPlayer)
                {
                    Player p = affectee as Player;  
                    if (!p.isLoggedIn)
                        return new ErrorPacket("Server: That target has logged out recently.");
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

                // Update the affectee.
                if (target.IsDead)
                {
                    initiator.AddKill();
                    GameObject.Kill(target);
                }
                else
                {
                    GameObject.UpdateMobiles(target);
                }

                // Update our initiator.
                if (initiator.IsDead)
                {
                    GameObject.Kill(initiator);
                }
                else
                {
                    GameObject.UpdateMobiles(initiator);
                }

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
        private static Packet MobileMove(MoveMobilePacket mm)
        {
            Node loc = GameObject.MoveMobile(mm.Location, mm.Author, direction : mm.Direction);
            if (loc == null)
                return new ErrorPacket("Server: Invalid location to move to.");

            mm.NewLocation = loc.GetTag();
            return mm;
        }

        /// <summary>
        ///     Brings a Mobile back to life and returns it to the client.
        /// </summary>
        /// <param name="mobile">Mobile to return to life.</param>
        /// <returns>Packaged Mobile.</returns>
        private static Packet MobileRessurrect(RessurrectMobilePacket res)
        {
            if (res.Author == null)
                return new ErrorPacket("Server: Ressurrection target not provided.");

            // Sends the mobile to the StartingZone, ressurrects, and processes it as if an admin performed the action.
            GameObject.Ressurrect(GameObject.StartingZone, res.Author);
            return new RessurrectMobilePacket(GameObject.StartingZone, res.Author, success: true);
        }
    }
    #endregion
}
