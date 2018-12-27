using System;
using System.Collections.Generic;
using SUS.Shared.Packets;
using SUS.Shared;
using SUS.Objects.Items;
using SUS.Objects;

namespace SUS.Server
{
    public class ClientHandler
    {
        private Gamestate m_Gamestate;
        private SocketHandler m_Handler;

        #region Constructors
        public ClientHandler(SocketHandler handler)
        {
            m_Handler = handler;
        }
        #endregion

        #region Getters / Setters
        public SocketHandler Handler { get { return m_Handler; } }

        public Gamestate Gamestate
        {
            get { return m_Gamestate; }
            private set
            {
                if (value != null)
                    m_Gamestate = value;
            }
        }
        #endregion

        /// <summary>
        ///     Processes and handles requests made by the client for server/gamestate information.
        ///     Responsible for also gathering and returning requested information to the client.
        /// </summary>
        /// <param name="socketHandler">Socket Handler to act upon.</param>
        /// <param name="req">Request Object (includes types.)</param>
        public void Parser(Packet Request)
        {
            if (Request.PlayerID == 0 && Request.Type != PacketTypes.SocketKill)
            {
                Utility.ConsoleNotify(" [ERR] Player ID: 0 && !SocketKill.");
                return;
            }
            else if (Request.Type == PacketTypes.Authenticate)
            {
                Handler.ToClient(ProcAuthenticate(Request as AccountAuthenticatePacket).ToByte());
                return;
            }
            else
            {
                Gamestate = World.FindGamestate(Request.PlayerID);
                if (m_Gamestate == null)
                {
                    Handler.ToClient(new ErrorPacket($"Server: Bad Player ID provided: [ {Request.PlayerID} ].").ToByte());
                    return;
                }
            }

            Packet clientInfo;

            switch (Request.Type)
            {
                case PacketTypes.SocketKill:
                    Logout(Request as SocketKillPacket);
                    clientInfo = null;
                    break;


                case PacketTypes.GetLocalMobiles:
                    clientInfo = GetLocalMobiles(Request as GetMobilesPacket);
                    break;
                case PacketTypes.GetMobile:
                    clientInfo = GetMobileInfo(Request as GetMobilePacket);
                    break;
                case PacketTypes.GetNode:
                    clientInfo = GetNode(Request as GetNodePacket);
                    break;


                case PacketTypes.MobileCombat:
                    clientInfo = MobileActionHandler(Request as CombatMobilePacket);
                    break;
                case PacketTypes.MobileMove:
                    clientInfo = MobileMove(Request as MoveMobilePacket);
                    break;
                case PacketTypes.MobileResurrect:
                    clientInfo = MobileRessurrect(Request as RessurrectMobilePacket);
                    break;
                case PacketTypes.UseItem:
                    clientInfo = MobileUseItem(Request as UseItemPacket);
                    break;


                default:
                    // Perhaps use "error" RequestType here.
                    clientInfo = new ErrorPacket("Server: Bad request received.");
                    break;
            }

            if (clientInfo != null)
                Handler.ToClient(clientInfo.ToByte());
        }

        #region Account Actions
        private Packet ProcAuthenticate(AccountAuthenticatePacket auth)
        {
            // Client initated Authenticate, look up and verify information.
            // if not information found, prompt to create a new gamestate.
            Gamestate gamestate = World.FindGamestate(auth.PlayerID);

            // Send our response if no player is found, else send the client their GameState.
            if (gamestate == null)
                return NewPlayer(auth.PlayerID, auth.Name);

            AccountClientPacket gsp = new AccountClientPacket(auth.PlayerID);
            gsp.ClientState = gamestate.ToClientState();
            return gsp;
        }

        /// <summary>
        ///     Receives a Player type from a client for authentication.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>Packaged GameState.</returns>
        private Packet NewPlayer(UInt64 playerID, string name)
        {
            Regions szRegion = World.StartingZone;
            Node region = World.FindNode(szRegion); // Assign the Starting Zone Node to the GameState.
            if (region == null)
                return new ErrorPacket("Server: Invalid location to move to.");

            Player player = new Player(name, 45, 45, 10);
            player.Region = szRegion;   // Assign the Starting Zone Location to the player.
            player.Location = region.StartingLocation();
            player.Login();             // Log the player in.


            // Client has sent a player, create a proper gamestate and send it to the client.
            Gamestate gamestate = new Gamestate(playerID, player, Regions.Basic);

            AccountClientPacket gsp = new AccountClientPacket(playerID);
            gsp.ClientState = gamestate.ToClientState();
            return gsp;
        }

        /// <summary>
        ///     Looks up the Mobile and Node, removes the Mobile, updates our lists.
        /// </summary>
        /// <param name="sk">Socket Kill containing the User ID.</param>
        private void Logout(SocketKillPacket sk)
        {
            if (sk.PlayerID == 0)
                return;             // No User ID? Just return.

            Gamestate gs = World.FindGamestate(sk.PlayerID);
            if (gs == null)
                return;             // No mobile by that User ID? Just return.

            Player account = World.FindMobile(gs.Account.Serial) as Player;
            if (account == null)
                return;

            account.Logout();
            //World.RemoveGamestate(gs);
        }
        #endregion

        #region Information Requests 
        private Packet GetLocalMobiles(GetMobilesPacket gmp)
        {
            HashSet<BaseMobile> lm = World.FindNearbyMobiles(gmp.Region, Gamestate.Account, Gamestate.Account.Vision);
            gmp.Mobiles = lm;
            return gmp;
        }

        /// <summary>
        ///     Requests a mobile from the server, if it is found then it is returned.
        /// </summary>
        /// <param name="mobile">Type and ID of the mobile in the form of a MobileTag.</param>
        /// <returns>Either an Error Request or a Request containing the mobile.</returns>
        private Packet GetMobileInfo(GetMobilePacket gmp)
        {
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
                            gmp.Paperdoll = Gamestate.Account.ToString();
                            break;
                        case GetMobilePacket.RequestReason.Location:
                            gmp.Region = Gamestate.Account.Region;
                            break;
                        case GetMobilePacket.RequestReason.IsDead:
                            gmp.IsAlive = Gamestate.Account.Alive;
                            break;
                        case GetMobilePacket.RequestReason.Items:
                            foreach (Item i in Gamestate.Account.Items.Values)
                                gmp.AddItem(i.Serial, i.ToString());
                            break;
                        case GetMobilePacket.RequestReason.Equipment:
                            foreach (Equippable e in Gamestate.Account.Equipment.Values)
                                gmp.AddEquipment(e.Serial, e.ToString());
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
        private Packet GetNode(GetNodePacket gnp)
        {
            Node n = World.FindNode(gnp.Region);  // Fetch a new or updated node.
            if (n == null)
                return new ErrorPacket("Server: Bad node requested.");

            gnp.NewRegion = n.GetBase();
            return gnp;
        }
        #endregion

        #region Mobile Actions
        /// <summary>
        ///     Handles actions that a mobile wants to perform. (recieved from client)
        /// </summary>
        private Packet MobileActionHandler(CombatMobilePacket mobileAction)
        {
            Packet req = MobileActionHandlerAttack(mobileAction);
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
        private Packet MobileActionHandlerAttack(CombatMobilePacket cmp)
        {
            Mobile attacker = Gamestate.Account;
            if (!attacker.Alive)
                return new ErrorPacket("Server: You are dead and need to ressurrect.");

            attacker.Target = null;                        // Reset the target.
            List<Mobile> mobiles = new List<Mobile>();      // This will hold all good mobiles.
            if (cmp.Targets.Count == 0)
                return new ErrorPacket("Server: No targets provided for attacking.");

            // Iterate each of the affected, adding it to our list of Mobiles.
            foreach (Serial serial in cmp.Targets)
            {
                // Lookup the affected mobile.
                 Mobile target = World.FindMobile(serial);
                if (target == null || !target.Alive)
                    return new ErrorPacket("Server: That target has moved or died recently.");
                else if (attacker.Region != target.Region)
                    return new ErrorPacket("Server: That target is no longer in the area.");
                else if (Point2D.Distance(attacker, target) > attacker.Vision)
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
                if (!attacker.Alive)
                    break;

                Mobile target = m;

                // Combat the two objects.
                cmp.AddUpdate(CombatStage.Combat(attacker, target));
                cmp.IsAlive = attacker.Alive;
                cmp.Result += $"{attacker.Name} attacked {target.Name}. ";    // TODO: Move this to MobileModifier.
            }

            return null;
        }

        /// <summary>
        ///     Moves a Mobile from one Node to another Node.
        /// </summary>
        /// <param name="mm">Node / Mobile pair to move.</param>
        /// <returns>Packed "OK" server response.</returns>
        private Packet MobileMove(MoveMobilePacket mm)
        {
            Node loc = World.MoveMobile(mm.Region, Gamestate.Account, direction: mm.Direction);
            if (loc == null)
                return new ErrorPacket("Server: Invalid location to move to.");

            mm.NewRegion = loc.GetBase();
            return mm;
        }

        /// <summary>
        ///     Brings a Mobile back to life and returns it to the client.
        /// </summary>
        /// <param name="mobile">Mobile to return to life.</param>
        /// <returns>Packaged Mobile.</returns>
        private Packet MobileRessurrect(RessurrectMobilePacket res)
        {   // Sends the mobile to the StartingZone, ressurrects, and processes it as if an admin performed the action.
            World.Ressurrect(World.StartingZone, Gamestate.Account);
            res.isSuccessful = true;
            res.Region = World.StartingZone;
            return res;
        }

        private Packet MobileUseItem(UseItemPacket uip)
        {
            if (!Gamestate.Account.Alive)
                return new ErrorPacket("Server: You are dead.");

            if (!Gamestate.Account.HasItem(uip.Item))
                return new ErrorPacket("Server: You no longer have that item.");

            Item i = Gamestate.Account.FindItem(uip.Item);
            if (i == null)
                return new ErrorPacket("Server: You no longer have that item.");


            switch(i.Type)
            {
                case ItemTypes.Consumable:
                    return UseConsumable(uip, i as Consumable);
                case ItemTypes.Equippable:
                case ItemTypes.Armor:
                case ItemTypes.Weapon:
                    return EquipEquippable(uip, i as Equippable);
                default:
                    return new ErrorPacket("Server: You no longer have that item.");
            }
        }

        #region Item Use
        private Packet UseConsumable(UseItemPacket uip, Consumable item)
        {
            Mobile mobile = Gamestate.Account;

            if (item.Amount <= 0)
                return new ErrorPacket("Server: Do you not have anymore of those.");

            switch (item.ConsumableType)
            {
                case ConsumableTypes.Bandages:
                case ConsumableTypes.HealthPotion:
                    if (mobile.Hits == mobile.HitsMax)
                        return new ErrorPacket("Server: You are already at full health.");

                    int effect = 0;
                    if (item.ConsumableType == ConsumableTypes.Bandages)
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

        private Packet EquipEquippable(UseItemPacket uip, Equippable item)
        {
            Mobile mobile = Gamestate.Account;
            mobile.Equip(item);
            uip.Response = $"You have equipped [{item.Name}].";
            return uip;
        }
        #endregion
    }
    #endregion
}
