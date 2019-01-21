using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using SUS.Objects;
using SUS.Objects.Items;
using SUS.Objects.Items.Consumables;
using SUS.Objects.Mobiles;
using SUS.Shared;
using SUS.Shared.Packets;

namespace SUS.Server
{
    public class ClientHandler : Handler
    {
        private Gamestate m_Gamestate;

        #region Constructors

        public ClientHandler(Socket socket)
        {
            Handler = new SocketHandler(socket, SocketHandler.Types.Client, true);
        }

        #endregion

        #region Getters / Setters

        private SocketHandler Handler { get; }

        private Gamestate Gamestate
        {
            get => m_Gamestate;
            set
            {
                if (value != null) m_Gamestate = value;
            }
        }

        #endregion

        #region Processor

        public override void Core()
        {
            var socketKill = new SocketKillPacket(0, false);

            var requests = 0;
            const int requestCap = 15;
            var timer = new Timer(5, Timer.Formats.Seconds);
            timer.Start();


            while (socketKill != null && socketKill.Kill == false)
            {
                var obj = Handler.FromClient();

                #region Check Timeout

                if (timer.Completed)
                {
                    timer.Restart();
                    requests = 0;
                }
                else if (!timer.Completed && requests >= requestCap)
                {
                    Handler.ToClient(
                        new ErrorPacket(
                                $"Server: You have exceeded {requestCap} requests in {timer.Limit / 1000} seconds and now on cooldown.")
                            .ToByte());
                    Thread.Sleep(timer.Limit * 3);
                    timer.Restart();
                    requests = 0;
                    continue;
                }
                else
                {
                    ++requests;
                }

                #endregion

                try
                {
                    if (obj is Packet req)
                    {
                        Parser(req); // If it is not a SocketKill, process it first.
                        if (req.Type == PacketTypes.SocketKill)
                            socketKill = req as SocketKillPacket; // This will lead to termination.
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Caught an exception: {e.Message}");
                    var skp = new SocketKillPacket(0); // Create a new packet.
                    Handler.ToClient(skp.ToByte()); // Send it to our client for a clean connection.
                    socketKill = skp; // Assign our local to break the loop.
                }
            }
        }

        /// <summary>
        ///     Processes and handles requests made by the client for server/gamestate information.
        ///     Responsible for also gathering and returning requested information to the client.
        /// </summary>
        private void Parser(IPacket request)
        {
            if (request.PlayerId == 0 && request.Type != PacketTypes.SocketKill)
            {
                Utility.ConsoleNotify(" [ERR] Player ID: 0 && !SocketKill.");
                return;
            }

            if (request.Type == PacketTypes.Authenticate)
            {
                var toClient = ProcAuthenticate(request as AccountAuthenticatePacket);
                Handler.ToClient(toClient.ToByte());
                return;
            }

            Gamestate = World.FindGamestate(request.PlayerId);
            if (m_Gamestate == null)
            {
                Handler.ToClient(new ErrorPacket($"Server: Bad Player ID provided: [ {request.PlayerId} ].").ToByte());
                return;
            }

            Packet clientInfo;

            switch (request.Type)
            {
                case PacketTypes.SocketKill:
                    Logout(request as SocketKillPacket);
                    clientInfo = null;
                    break;


                case PacketTypes.GetLocalMobiles:
                    clientInfo = GetLocalMobiles(request as GetMobilesPacket);
                    break;
                case PacketTypes.GetMobile:
                    clientInfo = GetMobileInfo(request as GetMobilePacket);
                    break;
                case PacketTypes.GetNode:
                    clientInfo = GetNode(request as GetNodePacket);
                    break;


                case PacketTypes.MobileCombat:
                    clientInfo = MobileActionHandler(request as CombatMobilePacket);
                    break;
                case PacketTypes.MobileMove:
                    clientInfo = MobileMove(request as MoveMobilePacket);
                    break;
                case PacketTypes.MobileResurrect:
                    clientInfo = MobileResurrect(request as ResurrectMobilePacket);
                    break;
                case PacketTypes.UseItem:
                    clientInfo = MobileUseItem(request as UseItemPacket);
                    break;


                default:
                    // Perhaps use "error" RequestType here.
                    clientInfo = new ErrorPacket("Server: Bad request received.");
                    break;
            }

            if (clientInfo != null) Handler.ToClient(clientInfo.ToByte());
        }

        #endregion

        #region Account Actions

        private Packet ProcAuthenticate(AccountAuthenticatePacket auth)
        {
            // Client imitated Authenticate, look up and verify information.
            // if not information found, prompt to create a new gamestate.
            var gamestate = World.FindGamestate(auth.PlayerId);

            // Send our response if no player is found, else send the client their GameState.
            if (gamestate == null) return NewPlayer(auth.PlayerId, auth.Name);

            var gsp = new AccountClientPacket(auth.PlayerId) {ClientState = gamestate.ToClientState()};
            return gsp;
        }

        /// <summary>
        ///     Receives a Player type from a client for authentication.
        /// </summary>
        /// <returns>Packaged GameState.</returns>
        private static Packet NewPlayer(ulong playerId, string name)
        {
            var szRegion = World.StartingZone;
            var region = World.FindNode(szRegion); // Assign the Starting Zone Node to the GameState.
            if (region == null) return new ErrorPacket("Server: Invalid location to move to.");

            var player = new Player(name, 45, 45, 10, szRegion, region.StartingLocation());
            // Assign the Starting Zone Location to the player.
            player.Login(); // Log the player in.

            // Client has sent a player, create a proper gamestate and send it to the client.
            var gamestate = new Gamestate(playerId, player, Regions.Basic);

            var gsp = new AccountClientPacket(playerId) {ClientState = gamestate.ToClientState()};
            return gsp;
        }

        /// <summary>
        ///     Looks up the Mobile and Node, removes the Mobile, updates our lists.
        /// </summary>
        /// <param name="sk">Socket Kill containing the User ID.</param>
        private static void Logout(IPacket sk)
        {
            if (sk.PlayerId == 0) return; // No User ID? Just return.

            var gs = World.FindGamestate(sk.PlayerId);
            if (gs == null) return; // No mobile by that User ID? Just return.

            gs.Account.Logout();
        }

        #endregion

        #region Information Requests 

        private Packet GetLocalMobiles(GetMobilesPacket gmp)
        {
            var lm = World.FindNearbyMobiles(gmp.Region, Gamestate.Account, Gamestate.Account.Vision);
            gmp.Mobiles = lm;
            return gmp;
        }

        /// <summary>
        ///     Requests a mobile from the server, if it is found then it is returned.
        /// </summary>
        /// <returns>Either an Error Request or a Request containing the mobile.</returns>
        private Packet GetMobileInfo(GetMobilePacket gmp)
        {
            var reason = gmp.Reason;

            while (reason != GetMobilePacket.RequestReason.None)
                foreach (GetMobilePacket.RequestReason r in Enum.GetValues(typeof(GetMobilePacket.RequestReason)))
                {
                    if (r == GetMobilePacket.RequestReason.None || (r & (r - 1)) != 0) continue;

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
                            foreach (var i in Gamestate.Account.Items) gmp.AddItem(i.Serial, i.ToString());

                            break;
                        case GetMobilePacket.RequestReason.Equipment:
                            foreach (var e in Gamestate.Account.Equipment.Values)
                                gmp.AddEquipment(e.Serial, e.ToString());

                            break;
                    }

                    reason &= ~r;
                }

            return gmp;
        }

        /// <summary>
        ///     Returns a Node to the client.
        /// </summary>
        /// <returns>Packaged Node.</returns>
        private static Packet GetNode(GetNodePacket gnp)
        {
            var n = World.FindNode(gnp.Region); // Fetch a new or updated node.
            if (n == null) return new ErrorPacket("Server: Bad node requested.");

            gnp.NewRegion = n.GetBase();
            return gnp;
        }

        #endregion

        #region Mobile Actions

        /// <summary>
        ///     Handles actions that a mobile wants to perform. (received from client)
        /// </summary>
        private Packet MobileActionHandler(CombatMobilePacket mobileAction)
        {
            var req = MobileActionHandlerAttack(mobileAction);
            return req ?? mobileAction;
        }

        /// <summary>
        ///     Performs the lookup and combating of mobiles.
        /// </summary>
        /// <returns>Packaged Error if occurred, otherwise should normally return null.</returns>
        private Packet MobileActionHandlerAttack(CombatMobilePacket cmp)
        {
            Mobile attacker = Gamestate.Account;
            if (!attacker.Alive) return new ErrorPacket("Server: You are dead and need to resurrect.");

            attacker.Target = null; // Reset the target.
            var mobiles = new List<Mobile>(); // This will hold all good mobiles.
            if (cmp.Targets.Count == 0) return new ErrorPacket("Server: No targets provided for attacking.");

            // Iterate each of the affected, adding it to our list of Mobiles.
            foreach (Serial serial in cmp.Targets)
            {
                // Lookup the affected mobile.
                var target = World.FindMobile(serial);
                if (target == null || !target.Alive)
                    return new ErrorPacket("Server: That target has moved or died recently.");

                if (attacker.Region != target.Region)
                    return new ErrorPacket("Server: That target is no longer in the area.");

                if (Point2D.Distance(attacker, target) > attacker.Vision)
                    return new ErrorPacket("Server: You are too far from the target.");

                if (target.IsPlayer)
                {
                    var p = target as Player;
                    if (p != null && !p.IsLoggedIn)
                        return new ErrorPacket("Server: That target has logged out recently.");
                }

                mobiles.Add(target); // Add it to our list of known "good" mobiles.
            }

            // Iterate our mobiles and perform combat.
            foreach (var m in mobiles)
            {
                if (!attacker.Alive) break;

                var target = m;

                // Combat the two objects.
                cmp.AddUpdate(CombatStage.Combat(attacker, target));
                cmp.IsAlive = attacker.Alive;
                cmp.Result += $"{attacker.Name} attacked {target.Name}. "; // TODO: Move this to MobileModifier.
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
            var loc = World.MoveMobile(mm.Region, Gamestate.Account, mm.Direction);
            if (loc == null) return new ErrorPacket("Server: Invalid location to move to.");

            mm.NewRegion = loc.GetBase();
            return mm;
        }

        /// <summary>
        ///     Brings a Mobile back to life and returns it to the client.
        /// </summary>
        /// <returns>Packaged Mobile.</returns>
        private Packet MobileResurrect(ResurrectMobilePacket res)
        {
            // Sends the mobile to the StartingZone, resurrects, and processes it as if an admin performed the action.
            World.Resurrect(World.StartingZone, Gamestate.Account);
            res.IsSuccessful = true;
            res.Region = World.StartingZone;
            return res;
        }

        private Packet MobileUseItem(UseItemPacket uip)
        {
            if (!Gamestate.Account.Alive) return new ErrorPacket("Server: You are dead.");

            var i = World.FindItem(uip.Item);
            if (i == null) return new ErrorPacket("Server: That item has been removed or deleted.");

            if (i.Owner == null || i.Owner.Serial != Gamestate.Account.Serial)
                return new ErrorPacket("Server: You no longer have that item.");

            switch (i.Type)
            {
                case ItemTypes.Consumable:
                    return UseConsumable(uip, i as Consumable);
                case ItemTypes.Equippable:
                case ItemTypes.Armor:
                case ItemTypes.Weapon:
                    return EquipItem(uip, i as Equippable);
                default:
                    return new ErrorPacket("Server: You no longer have that item.");
            }
        }

        #region Item Use

        private Packet UseConsumable(UseItemPacket uip, Consumable item)
        {
            Mobile mobile = Gamestate.Account;

            if (item.Amount <= 0) return new ErrorPacket("Server: Do you not have anymore of those.");

            switch (item.ConsumableType)
            {
                case ConsumableTypes.Bandages:
                case ConsumableTypes.HealthPotion:
                    if (mobile.Hits == mobile.HitsMax)
                        return new ErrorPacket("Server: You are already at full health.");

                    int effect;
                    if (item.ConsumableType == ConsumableTypes.Bandages)
                    {
                        // Uses a bandage.
                        --mobile.Bandages;
                        effect = Bandage.GetEffect(mobile.HitsMax, mobile.Skills[SkillName.Healing].Value);
                        var increase = mobile.SkillIncrease(SkillName.Healing);
                        if (increase != string.Empty) uip.Response += $"{increase}\n";

                        increase = mobile.StatIncrease(StatCode.Dexterity);
                        if (increase != string.Empty) uip.Response += $"{increase}\n";
                    }
                    else
                    {
                        // Uses a health potion.
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

                    uip.Response +=
                        $"You used one of your {item.Name} that heal {effect} health points.\nHealth: {mobile.Hits} / {mobile.HitsMax}. {item.Amount} {item.Name} remain.";
                    break;

                default:
                    return new ErrorPacket("Server: We can only use health potions and bandages for now.");
            }

            return uip;
        }

        private Packet EquipItem(UseItemPacket uip, Equippable item)
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