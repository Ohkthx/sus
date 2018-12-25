using System;
using System.Collections.Generic;
using SUS.Shared.Utilities;
using SUS.Shared.Objects;
using SUS.Shared.Objects.Items;
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
                    clientInfo = Gamestate(req as AccountGameStatePacket);
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
                case PacketTypes.UseItem:
                    clientInfo = MobileUseItem(req as UseItemPacket);
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
            Gamestate gamestate = World.FindGamestate(auth.Author.ID);

            // Send our response if no player is found, else send the client their GameState.
            if (gamestate == null)
                return Player(new Player(auth.Author.ID, auth.Author.Name, 50, 35, 20));

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
            Locations szLoc = World.StartingZone;

            player.Location = szLoc;            // Assign the Starting Zone Location to the player.
            player.Login();                     // Log the player in.

            // Client has sent a player, create a proper gamestate and send it to the client.
            Gamestate newState = new Gamestate(player.Basic());

            Node loc = World.FindNode(szLoc); // Assign the Starting Zone Node to the GameState.
            if (loc == null)
                return new ErrorPacket("Server: Invalid location to move to.");

            newState.NodeCurrent = loc.GetTag();

            World.AddGamestate(newState);          // Updates the GameObject with the new state that is being tracked.
            World.AddMobile(player);               // Update our tracked Mobiles with the new Player.

            AccountGameStatePacket gsp = new AccountGameStatePacket(player.Basic());
            gsp.GameState = newState;
            return gsp;
        }

        private static Packet Gamestate(AccountGameStatePacket gsp)
        {
            World.AddGamestate(gsp.GameState);

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

            Gamestate gs = World.FindGamestate(sk.Author.ID);
            if (gs == null)
                return;             // No mobile by that User ID? Just return.

            Player account = World.FindMobile(gs.Account.Guid) as Player;
            if (account == null)
                return;

            account.Logout();
            World.RemoveGamestate(gs);
        }
        #endregion

        #region Information Requests 
        private static Packet LocalMobiles(GetMobilesPacket gmp)
        {
            Mobile m = World.FindMobile(gmp.Author.Guid);
            if (m == null || m.Coordinate == null)
                return new ErrorPacket("Server: You are not in a location to get nearby objects.");

            HashSet<BasicMobile> lm = World.FindNearbyMobiles(gmp.Location, m, m.Vision);
            gmp.Mobiles = lm;
            return gmp;
        }

        /// <summary>
        ///     Requests a mobile from the server, if it is found then it is returned.
        /// </summary>
        /// <param name="mobile">Type and ID of the mobile in the form of a MobileTag.</param>
        /// <returns>Either an Error Request or a Request containing the mobile.</returns>
        private static Packet GetMobile(GetMobilePacket gmp)
        {
            if (gmp.Target == null)
                return new ErrorPacket("Server: Bad mobile requested.");

            Mobile m = World.FindMobile(gmp.Target.Guid);
            if (m == null)
                return new ErrorPacket("Server: There is no such mobile anymore.");

            GetMobilePacket.RequestReason reason = gmp.Reason;

            while (reason != GetMobilePacket.RequestReason.None)
            {
                foreach (GetMobilePacket.RequestReason r in Enum.GetValues(typeof(GetMobilePacket.RequestReason)))
                {
                    if (r == GetMobilePacket.RequestReason.None || (r & (r - 1)) != 0)
                        continue; 

                    switch (gmp.Reason & r)
                    {
                        case GetMobilePacket.RequestReason.Paperdoll:
                            gmp.Paperdoll = m.ToString();
                            break;
                        case GetMobilePacket.RequestReason.Location:
                            gmp.Location = m.Location;
                            break;
                        case GetMobilePacket.RequestReason.IsDead:
                            gmp.IsDead = m.IsDead;
                            break;
                        case GetMobilePacket.RequestReason.Items:
                            gmp.Items = m.Items;
                            break;
                        case GetMobilePacket.RequestReason.Equipment:
                            gmp.Equipment = m.Equipment;
                            break;
                    }

                    reason &= ~(r);
                }
            }

            return gmp;
        }

        /// <summary>
        ///     Returns a Node to the client.
        /// </summary>
        /// <param name="node">ID of the Node to retrieve.</param>
        /// <returns>Packaged Node.</returns>
        private static Packet GetNode(GetNodePacket gnp)
        {
            Node n = World.FindNode(gnp.Location);  // Fetch a new or updated node.
            if (n == null)
                return new ErrorPacket("Server: Bad node requested.");

            gnp.NewLocation = n.GetTag();
            return gnp;
        }
        #endregion

        #region Mobile Actions
        /// <summary>
        ///     Handles actions that a mobile wants to perform. (recieved from client)
        /// </summary>
        private static Packet MobileActionHandler(CombatMobilePacket mobileAction)
        {
            Player initator = World.FindMobile(mobileAction.Author.Guid) as Player;
            if (initator == null)
                return new ErrorPacket("Server: Bad initiator provided for action.");

            Packet req = MobileActionHandlerAttack(initator, mobileAction);
            if (req != null)    // If Request is not null, that means an error occured in MobileActionHandlerAttack.
                return req;     // Return the error to the client.

            return mobileAction;    // Create a new Request with updated information.
        }

        /// <summary>
        ///     Performs the lookup and combating of mobiles.
        /// </summary>
        /// <param name="initiator">Initiating Mobile.</param>
        /// <param name="cmp">Reference to the MobileAction to send back.</param>
        /// <returns>Packaged Error if occurred, otherwise should normally return null.</returns>
        private static Packet MobileActionHandlerAttack(Player initiator, CombatMobilePacket cmp)
        {
            if (initiator.IsDead)
                return new ErrorPacket("Server: You are dead and need to ressurrect.");

            initiator.Target = null;                        // Reset the target.
            List<Mobile> mobiles = new List<Mobile>();      // This will hold all good mobiles.
            List<BasicMobile> targets = cmp.GetTargets();   // List containing basic mobiles representing targets.
            if (targets.Count == 0)
                return new ErrorPacket("Server: No targets provided for attacking.");

            // Iterate each of the affected, adding it to our list of Mobiles.
            foreach (BasicMobile t in targets)
            {
                // Lookup the affected mobile.
                 Mobile target = World.FindMobile(t.Guid);
                if (target == null)
                    return new ErrorPacket("Server: That target has moved or died recently.");
                else if (initiator.Location != target.Location)
                    return new ErrorPacket("Server: That target is no longer in the area.");
                else if (initiator.Coordinate.Distance(target.Coordinate) > initiator.Vision)
                    return new ErrorPacket("Server: You are too far from the target.");
                else if (target.IsPlayer)
                {
                    Player p = target as Player;  
                    if (!p.isLoggedIn)
                        return new ErrorPacket("Server: That target has logged out recently.");
                }

                mobiles.Add(target);  // Add it to our list of known "good" mobiles.
            }

            // Iterate our mobiles and perform combat.
            foreach (Mobile m in mobiles)
            {
                if (initiator.IsDead)
                    break;

                Mobile target = m;
                Mobile init = initiator as Mobile;

                // Combat the two objects.
                cmp.AddUpdate(CombatStage.Combat(init, target));

                // Update the affectee.
                if (target.IsDead)
                {
                    (init as Player).AddKill();
                    World.Kill(target);
                }

                // Update our initiator.
                if (init.IsDead)
                {
                    cmp.IsDead = true;
                    World.Kill(init);
                }

                cmp.Result += $"{init.Name} attacked {target.Name}. ";    // TODO: Move this to MobileModifier.
            }

            // Add our updated information for the initiator. After processing all changes.
            cmp.CleanClientInfo();         // Remove worthless data so it isn't retransmitted.
            return null;
        }

        /// <summary>
        ///     Moves a Mobile from one Node to another Node.
        /// </summary>
        /// <param name="mm">Node / Mobile pair to move.</param>
        /// <returns>Packed "OK" server response.</returns>
        private static Packet MobileMove(MoveMobilePacket mm)
        {
            Node loc = World.MoveMobile(mm.Location, mm.Author, direction: mm.Direction);
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
            World.Ressurrect(World.StartingZone, res.Author);
            return new RessurrectMobilePacket(World.StartingZone, res.Author, success: true);
        }

        private static Packet MobileUseItem(UseItemPacket uip)
        {
            if (uip.Item == null || uip.Item == Guid.Empty)
                return new ErrorPacket("Server: That is an invalid item.");

            Mobile m = World.FindMobile(uip.Author.Guid);
            if (m == null)
                return new ErrorPacket("Server: Invalid object to use that item on.");

            if (m.IsDead)
                return new ErrorPacket("Server: You are dead.");

            if (!m.HasItem(uip.Item))
                return new ErrorPacket("Server: You no longer have that item.");

            Item i = m.FindItem(uip.Item);
            if (i == null)
                return new ErrorPacket("Server: You no longer have that item.");


            switch(i.Type)
            {
                case ItemTypes.Consumable:
                    return UseConsumable(uip, m, i as Consumable);
                case ItemTypes.Equippable:
                case ItemTypes.Armor:
                case ItemTypes.Weapon:
                    return EquipEquippable(uip, m, i as Equippable);
                default:
                    return new ErrorPacket("Server: You no longer have that item.");
            }
        }

        #region Item Use
        private static Packet UseConsumable(UseItemPacket uip, Mobile mobile, Consumable item)
        {
            if (item.Amount <= 0)
                return new ErrorPacket("Server: Do you not have anymore of those.");

            switch (item.ConsumableType)
            {
                case Consumable.Types.Bandages:
                case Consumable.Types.HealthPotion:
                    if (mobile.Hits == mobile.HitsMax)
                        return new ErrorPacket("Server: You are already at full health.");

                    int effect = 0;
                    if (item.ConsumableType == Consumable.Types.Bandages)
                    {   // Uses a bandage.
                        --mobile.Bandages;
                        effect = Bandage.GetEffect(mobile.HitsMax, mobile.Skills[SkillName.Healing].Value);
                        string increase = mobile.SkillIncrease(SkillName.Healing);
                        if (increase != string.Empty)
                            uip.Response += $"{increase}\n";
                        increase = mobile.StatIncrease(StatCode.Dexterity);
                        if (increase != string.Empty)
                            uip.Response += $"{increase}\n";
                    }
                    else
                    {   // Uses a health potion.
                        --mobile.HealthPotions;
                        effect = Potion.GetEffect(mobile.HitsMax);
                    }

                    if (mobile.Hits + effect >= mobile.HitsMax)
                    {
                        effect = mobile.HitsMax - mobile.Hits;
                        mobile.Hits = mobile.HitsMax;
                    }
                    else
                    {
                        mobile.Hits += effect;
                    }

                    uip.Response += $"You used one of your {item.Name} that heal {effect} health points.\nHealth: {mobile.Hits} / {mobile.HitsMax}. {item.Amount} {item.Name} remain.";
                    break;

                default:
                    return new ErrorPacket("Server: We can only use health potions and bandages for now.");
            }

            return uip;
        }

        private static Packet EquipEquippable(UseItemPacket uip, Mobile mobile, Equippable item)
        {
            mobile.Equip(item);
            uip.Response = $"You have equipped [{item.Name}].";
            return uip;
        }
        #endregion
    }
    #endregion
}
