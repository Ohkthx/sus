using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;
using SUS.Server.Objects;
using SUS.Server.Objects.Items;
using SUS.Server.Objects.Items.Consumables;
using SUS.Server.Objects.Mobiles;
using SUS.Shared;
using SUS.Shared.Actions;
using SUS.Shared.Packets;

namespace SUS.Server.Server
{
    public class ClientHandler : IHandler
    {
        private Gamestate _gamestate;

        #region Constructors

        public ClientHandler(Socket socket)
        {
            Handler = new SocketHandler(socket, SocketHandler.Types.Client, true);
        }

        #endregion

        public void Close()
        {
            Handler.Kill();
        }

        #region Information Requests 

        /// <summary>
        ///     Requests a mobile from the server, if it is found then it is returned.
        /// </summary>
        /// <returns>Either an Error Request or a Request containing the mobile.</returns>
        private Packet GetInfo(GetInfoPacket getInfo)
        {
            switch (getInfo.Reason)
            {
                case GetInfoPacket.RequestReason.Paperdoll:
                    getInfo.Info = Gamestate.Account.ToString();
                    break;
                case GetInfoPacket.RequestReason.Location:
                    var point2d = Gamestate.Account.Location.IsValid
                        ? Gamestate.Account.Location.ToString()
                        : string.Empty;
                    getInfo.Info = $"{Enum.GetName(typeof(Regions), Gamestate.Account.Region)} {point2d}";
                    break;
                case GetInfoPacket.RequestReason.Equipment:
                    foreach (var e in Gamestate.Account.Equipment.Values)
                    {
                        // Prevent starter gear being sent as "equipped"
                        if (e.IsStarter)
                            continue;

                        getInfo.AddInfo(e.ToString());
                    }

                    break;
                case GetInfoPacket.RequestReason.Items:
                    foreach (var i in Gamestate.Account.Items)
                    {
                        if (i is Equippable e && e.IsStarter)
                            continue;

                        getInfo.AddInfo(i.ToString());
                    }

                    break;
                case GetInfoPacket.RequestReason.Vendors:
                case GetInfoPacket.RequestReason.Npcs:
                    if (!World.FindRegion(Gamestate.Account.Region, out var region))
                    {
                        var loc = Enum.GetName(typeof(Regions), Gamestate.Account.Region);
                        return new ErrorPacket($"You are somehow in an unknown location: '{loc}'");
                    }

                    switch (getInfo.Reason)
                    {
                        case GetInfoPacket.RequestReason.Vendors:
                            foreach (var (_, type) in region.GetLocalNpcs())
                            {
                                var n = Enum.GetName(typeof(NPCTypes), type);
                                getInfo.AddInfo(n);
                            }

                            break;
                        case GetInfoPacket.RequestReason.Npcs:
                            var mobiles =
                                World.FindNearbyMobiles(Gamestate.Account, region.Id, Gamestate.Account.Vision);
                            foreach (var m in mobiles)
                                getInfo.AddInfo(m.Name);

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(getInfo.Reason));
                    }

                    break;
                default:
                    var err = $"Unknown request for information: {getInfo.Reason}";
                    Utility.ConsoleNotify(err);
                    Utility.LogWrite("err.txt", err);
                    return new ErrorPacket("You attempted to request unknown information.");
            }

            return getInfo;
        }

        #endregion

        #region Getters / Setters

        private SocketHandler Handler { get; }

        private Gamestate Gamestate
        {
            get => _gamestate;
            set
            {
                if (value != null)
                    _gamestate = value;
            }
        }

        #endregion

        #region Processor

        public void Core()
        {
            var socketKill = new SocketKillPacket(kill: false);

            var requests = 0;
            const int requestCap = 15;
            var timer = new Timer(5, Timer.Formats.Seconds);
            timer.Start();


            while (socketKill.Kill == false)
            {
                var clientResponse = Handler.Receive();

                #region Check Timeout

                if (timer.Completed)
                {
                    timer.Restart();
                    requests = 0;
                }
                else if (!timer.Completed && requests >= requestCap)
                {
                    Handler.Send(
                        new ErrorPacket(
                            $"Server: You have exceeded {requestCap} requests in {timer.Limit / 1000} seconds and now on cooldown."));
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
                    if (clientResponse is Packet packet)
                    {
                        var toClient = Parser(packet); // If it is not a SocketKill, process it first.
                        if (toClient != null)
                        {
                            ++toClient.Stage;
                            Handler.Send(toClient);
                        }

                        if (packet is SocketKillPacket skp)
                            socketKill = skp; // This will lead to termination.
                    }
                }
                catch (NotEnoughGoldException)
                {
                    var errPkt = new ErrorPacket("Not enough gold.");
                    Handler.Send(errPkt);
                }
                catch (UnknownItemException inf)
                {
                    var errPkt = new ErrorPacket(inf.Message);
                    Handler.Send(errPkt);
                }
                catch (UnknownMobileException mnf)
                {
                    var errPkt = new ErrorPacket(mnf.Message);
                    Handler.Send(errPkt);
                }
                catch (UnknownRegionException lnf)
                {
                    if (World.FindMobile(lnf.MobileId, out _))
                        World.MoveMobile(new Move(lnf.MobileId, World.StartingZone), out _, true);

                    var errPkt = new ErrorPacket(lnf.Message);
                    Handler.Send(errPkt);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Caught an exception: {e.Message}");
                    var skp = new SocketKillPacket(); // Create a new packet.
                    Handler.Send(skp); // Send it to our client for a clean connection.
                    socketKill = skp; // Assign our local to break the loop.
                }
            }
        }

        /// <summary>
        ///     Processes and handles requests made by the client for server/gamestate information.
        ///     Responsible for also gathering and returning requested information to the client.
        /// </summary>
        private Packet Parser(IPacket request)
        {
            if (request is AccountAuthenticatePacket accpkt)
                return ProcAuthenticate(accpkt);

            Gamestate = World.FindGamestate(request.PlayerId);
            if (_gamestate == null)
                return new ErrorPacket($"Server: Bad Player ID provided: [ {request.PlayerId} ].");

            switch (request)
            {
                case SocketKillPacket pkt:
                    Logout(pkt);
                    return pkt;


                case GetInfoPacket pkt:
                    return GetInfo(pkt);


                case CombatPacket pkt:
                    return MobileActionHandler(pkt);
                case MovePacket pkt:
                    return MobileMove(pkt);
                case ResurrectPacket pkt:
                    return MobileResurrect(pkt);


                case UseItemPacket pkt:
                    return UseItem(pkt);
                case UseVendorPacket pkt:
                    return UseVendor(pkt);


                default:
                    return new ErrorPacket("Server: Bad request received.");
            }
        }

        #endregion

        #region Account Actions

        private Packet ProcAuthenticate(AccountAuthenticatePacket auth)
        {
            // Client imitated Authenticate, look up and verify information.
            // if not information found, prompt to create a new gamestate.
            var gamestate = World.FindGamestate(auth.PlayerId);

            // Send our response if no player is found, else send the client their GameState.
            if (gamestate == null)
                return NewPlayer(auth.PlayerId, auth.Name);

            var gsp = new AccountClientPacket {ClientState = gamestate.ToClientState()};
            return gsp;
        }

        /// <summary>
        ///     Receives a Player type from a client for authentication.
        /// </summary>
        /// <returns>Packaged GameState.</returns>
        private static Packet NewPlayer(ulong playerId, string name)
        {
            var szRegion = World.StartingZone;
            if (!World.FindRegion(szRegion, out var region))
                return new ErrorPacket("Server: Invalid location to move to.");

            var player = new Player(name, 45, 45, 10, szRegion, region.StartingLocation());
            // Assign the Starting Zone Location to the player.
            player.Login(); // Log the player in.
            // Give the player the basic zones.
            player.AddRegionAccess(Regions.Britain | Regions.Wilderness | Regions.Sewers);

            // Client has sent a player, create a proper gamestate and send it to the client.
            var gamestate = new Gamestate(playerId, player);

            var gsp = new AccountClientPacket {ClientState = gamestate.ToClientState()};
            return gsp;
        }

        /// <summary>
        ///     Looks up the Mobile and Node, removes the Mobile, updates our lists.
        /// </summary>
        /// <param name="sk">Socket Kill containing the User ID.</param>
        private static void Logout(IPacket sk)
        {
            if (sk.PlayerId == 0)
                return; // No User ID? Just return.

            var gs = World.FindGamestate(sk.PlayerId);
            if (gs == null)
                return; // No mobile by that User ID? Just return.

            gs.Account.Logout();
        }

        #endregion

        #region Mobile Actions

        /// <summary>
        ///     Handles actions that a mobile wants to perform. (received from client)
        /// </summary>
        private Packet MobileActionHandler(CombatPacket combatPacket)
        {
            var req = MobileActionHandlerAttack(combatPacket);
            return req ?? combatPacket;
        }

        /// <summary>
        ///     Performs the lookup and combating of mobiles.
        /// </summary>
        /// <returns>Packaged Error if occurred, otherwise should normally return null.</returns>
        private Packet MobileActionHandlerAttack(CombatPacket combatPacket)
        {
            Mobile attacker = Gamestate.Account;
            if (!attacker.Alive)
                return new ErrorPacket("Server: You are dead and need to resurrect.");

            if (!World.FindRegion(Gamestate.Account.Region, out var currentNode))
                throw new UnknownRegionException(attacker.Serial,
                    "You have somehow wandered that where you do not belong.");

            if (combatPacket.Stage == Packet.Stages.One)
            {
                var localMobiles = World.FindNearbyMobiles(Gamestate.Account, currentNode.Id,
                    Gamestate.Account.Vision);
                foreach (var m in localMobiles)
                    combatPacket.AddMobile(m);

                return combatPacket;
            }

            if (combatPacket.Stage >= Packet.Stages.Two && combatPacket.Targets.Count == 0)
                return new ErrorPacket("Server: No targets provided for attacking.");

            attacker.Target = null; // Reset the target.
            var mobiles = new List<Mobile>(); // This will hold all good mobiles.


            // Iterate each of the affected, adding it to our list of Local Mobiles.
            foreach (Serial serial in combatPacket.Targets)
            {
                // Lookup the affected mobile.
                if (!World.FindMobile(serial, out var target) || !target.Alive)
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
                if (!attacker.Alive)
                    break;

                var target = m;

                // Combat the two objects.
                combatPacket.AddUpdate(CombatStage.Combat(attacker, target));
                combatPacket.IsAlive = attacker.Alive;
                combatPacket.Result +=
                    $"{attacker.Name} attacked {target.Name}. "; // TODO: Move this to MobileModifier.
            }

            return null;
        }

        /// <summary>
        ///     Moves a Mobile from one Node to another Node.
        /// </summary>
        /// <param name="movePacket">Node / Mobile pair to move.</param>
        /// <returns>Packed "OK" server response.</returns>
        private Packet MobileMove(MovePacket movePacket)
        {
            var originalUnlocks = Gamestate.AccessibleRegions; // Original unlocks for comparison.

            if (!World.MoveMobile(movePacket.Action, out var loc))
                return new ErrorPacket("Server: Did not move.");

            movePacket.NewRegion = loc.GetBase();

            if (originalUnlocks != Gamestate.Account.AccessibleRegions)
                // Extract the new locations.
                movePacket.AddDiscovery(originalUnlocks ^ Gamestate.Account.AccessibleRegions);

            return movePacket;
        }

        /// <summary>
        ///     Brings a Mobile back to life and returns it to the client.
        /// </summary>
        /// <returns>Packaged Mobile.</returns>
        private Packet MobileResurrect(ResurrectPacket res)
        {
            // Sends the mobile to the StartingZone, resurrects, and processes it as if an admin performed the action.a
            if (!World.Resurrect(Gamestate.Account, World.StartingZone))
                return new ErrorPacket("Unable to resurrect.");

            res.IsSuccessful = true;
            res.Region = World.StartingZone;

            return res;
        }

        private Packet UseItem(UseItemPacket useItem)
        {
            if (!Gamestate.Account.Alive)
                return new ErrorPacket("Server: You are dead.");

            switch (useItem.Stage)
            {
                case Packet.Stages.One:
                    foreach (var item in Gamestate.Account.Items)
                    {
                        // Ensure that we do not send starter gear to the client.
                        if (item is Equippable equippable && equippable.IsStarter)
                            continue;

                        useItem.AddItem(item.Serial, item.ToString());
                    }

                    return useItem;
                case Packet.Stages.Two:

                    if (!World.FindItem(useItem.Item, out var i))
                        return new ErrorPacket("Server: That item has been removed or deleted.");

                    if (i.Owner == null || i.Owner.Serial != Gamestate.Account.Serial)
                        return new ErrorPacket("Server: You no longer have that item.");

                    switch (i.Type)
                    {
                        case ItemTypes.Consumable:
                            return UseConsumable(useItem, i as Consumable);
                        case ItemTypes.Equippable:
                        case ItemTypes.Armor:
                        case ItemTypes.Weapon:
                            return EquipItem(useItem, i as Equippable);
                        default:
                            return new ErrorPacket("Server: You no longer have that item.");
                    }

                default:
                    throw new ArgumentOutOfRangeException();
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

                    var effect = 0;
                    if (item.ConsumableType == ConsumableTypes.Bandages)
                    {
                        // Uses a bandage.
                        --mobile.Bandages;
                        effect = Bandage.GetEffect(mobile.HitsMax, mobile.Skills[SkillName.Healing].Value);
                        var increase = mobile.SkillIncrease(SkillName.Healing);
                        if (increase != string.Empty)
                            uip.Response += $"{increase}\n";

                        increase = mobile.StatIncrease(StatCode.Dexterity);
                        if (increase != string.Empty)
                            uip.Response += $"{increase}\n";
                    }
                    else
                    {
                        // Uses a health potion.
                        --mobile.HealthPotions;
                        effect = Potion.GetEffect(mobile.HitsMax);
                    }

                    // Apply the heal.
                    effect = mobile.Heal(effect, mobile);

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
            if (item.IsBroken)
            {
                uip.Response = "Cannot equip that item, it is broken.";
                return uip;
            }

            Mobile mobile = Gamestate.Account;
            mobile.Equip(item);
            uip.Response = $"You have equipped [{item.Name}].";
            return uip;
        }

        #endregion

        private Packet UseVendor(UseVendorPacket useVendor)
        {
            if (!World.FindRegion(Gamestate.Account.Region, out var currentRegion))
                throw new UnknownRegionException(Gamestate.Account.Serial,
                    $"Attempted to access {Gamestate.Account.Region} while using a vendor.");

            NPC vendor = null;
            if (useVendor.Stage >= Packet.Stages.Two && useVendor.LocalNPC != NPCTypes.None)
                vendor = currentRegion.FindNpc(useVendor.LocalNPC);

            Item item = null;
            if (useVendor.Stage >= Packet.Stages.Three && !useVendor.Item.Default)
                if (!World.FindItem(useVendor.Item.Serial, out item))
                    return new ErrorPacket("That is an unknown item.");


            switch (useVendor.Stage)
            {
                case Packet.Stages.One:
                    // Present the local vendors if there are some.
                    useVendor.LocalVendors = currentRegion.GetLocalNpcs(true);
                    if (useVendor.LocalVendors.Count <= 1 && useVendor.LocalVendors[0] == NPCTypes.None)
                        return new ErrorPacket("There are no vendors in this area.");

                    break;

                case Packet.Stages.Two:
                    if (vendor == null)
                        break;

                    // Get the items that are part of the desired service.
                    useVendor.Items = vendor.ServiceableItems(Gamestate.Account);
                    if (useVendor.Items == null || useVendor.Items.Count == 0)
                        return new ErrorPacket("There are not any items that service applies to.");

                    break;

                case Packet.Stages.Three:
                    if (vendor == null)
                        break;

                    useVendor.Transaction = vendor.ServicePrice(item);
                    break;

                case Packet.Stages.Four:
                    if (vendor == null)
                        break;

                    if (useVendor.PerformAction == UseVendorPacket.Choices.No)
                        break;

                    var price = vendor.PerformService(Gamestate.Account, item);
                    if (price > 0)
                        return new OkPacket($"Total cost charged was: {price}gp.");

                    return new OkPacket("You were not changed any gold for the service.");

                default:
                    throw new InvalidEnumArgumentException("Unexpected action attempted while using a vendor.");
            }

            if (useVendor.Stage != Packet.Stages.One && vendor == null)
                return new ErrorPacket("That is an unknown vendor.");

            return useVendor;
        }

        #endregion
    }
}