using System;
using System.Collections.Generic;
using System.Linq;
using SUS.Shared;
using SUS.Shared.Packets;

namespace SUS.Client
{
    public static class InteractiveConsole
    {
        private static List<string> _lastOption;
        private static ClientState _state;
        public static SocketHandler SocketHandler;

        public static Packet Core()
        {
            var pos = 1;
            Console.WriteLine("\nChoose an option:");
            foreach (BaseOptions e in Enum.GetValues(typeof(BaseOptions)))
            {
                if (pos != (int) e)
                    continue;

                if (e == BaseOptions.None)
                    continue;

                Console.Write($"[{Enum.GetName(typeof(BaseOptions), e).ToLower()}] ");
                ++pos;
            }

            Console.WriteLine();

            do
            {
                string userInput;
                do
                {
                    Console.Write("\n > ");
                    userInput = Console.ReadLine();
                } while (string.IsNullOrWhiteSpace(userInput));

                Console.WriteLine();

                userInput = userInput.ToLower();
                _lastOption = userInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                try
                {
                    if (!InputParser(out var toServer))
                        continue;

                    toServer.PlayerId = State.PlayerId;
                    return toServer;
                }
                catch (IndexOutOfRangeException ie)
                {
                    Utility.ConsoleNotify(ie.Message);
                }
                catch (ArgumentException ae)
                {
                    Utility.ConsoleNotify(ae.Message);
                }
                catch (Exception e)
                {
                    Utility.ConsoleNotify($"An unknown error occurred: \n{e}");
                }
            } while (true);
        }

        #region Packet Parser

        public static Packet PacketParser(Packet packet)
        {
            switch (packet)
            {
                case OkPacket pkt:
                    if (pkt.Message != string.Empty)
                        Utility.ConsoleNotify(pkt.Message);

                    break;
                case ErrorPacket pkt:
                    if (pkt.Message != string.Empty)
                        Utility.ConsoleNotify(pkt.Message);

                    break;
                case AccountClientPacket pkt:
                    State = pkt.ClientState;
                    break;
                case SocketKillPacket pkt:
                    Utility.ConsoleNotify("Exiting.");
                    if (pkt.Message != string.Empty)
                        Utility.ConsoleNotify("Reason: " + pkt.Message);

                    SocketHandler.Kill();
                    break;


                case GetInfoPacket pkt:
                    GetInfo(pkt);
                    break;


                case CombatPacket pkt:
                    return MobileCombat(pkt);
                case MovePacket pkt:
                    return MobileMove(pkt);
                case ResurrectPacket pkt:
                    return Resurrect(pkt); // If we require a new current node,


                case UseItemPacket pkt:
                    return UseItem(pkt);
                case UseVendorPacket pkt:
                    return UseVendor(pkt);


                case AccountAuthenticatePacket pkt:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        #endregion

        #region Enums

        private enum BaseOptions
        {
            None,
            Help,
            Get,
            Use,
            Move,
            Attack,
            Vendors,
            Exit,

            // Aliases
            Show = Get,
            List = Get,
            Quit = Exit
        }

        private enum GetOptions
        {
            None,
            Help,
            Paperdoll,
            Location,
            Equipment,
            Items,
            Vendors,
            Npcs
        }

        #endregion

        #region Getters / Setters

        private static ClientState State
        {
            get => _state;
            set
            {
                if (value == null)
                    return;

                _state = value;
            }
        }

        public static void SetHandler(SocketHandler handler)
        {
            if (SocketHandler != null)
                return;

            SocketHandler = handler;
        }

        #endregion

        #region Console Actions

        private static bool InputParser(out Packet toServer)
        {
            // We cannot operate on an invalid supplied list of options.
            if (_lastOption == null || _lastOption.Count == 0)
                throw new ArgumentException("User options cannot be null or empty.");

            var opt = BaseOptions.None;
            foreach (var enumName in Enum.GetNames(typeof(BaseOptions)))
            {
                if (!Enum.TryParse(enumName, out BaseOptions enumValue))
                    throw new ArgumentException("Tried to re-parse an unknown user option.");

                if (enumValue == BaseOptions.None)
                    continue;

                if (!enumName.ToLower().Contains(_lastOption[0]))
                    continue;

                opt = enumValue;
                break;
            }

            if (opt == BaseOptions.None)
                throw new ArgumentException("User input was not valid.");


            switch ((int) opt)
            {
                case (int) BaseOptions.Help:
                    Console.WriteLine("Options:");
                    foreach (var h in Enum.GetNames(typeof(BaseOptions))
                        .SkipWhile(x => x == Enum.GetName(typeof(BaseOptions), BaseOptions.None)))
                        Console.Write($"[{h.ToLower()}] ");

                    Console.WriteLine();
                    toServer = null;
                    break;

                case (int) BaseOptions.Get:
                    _lastOption.RemoveAt(0); // Remove the "get" portion of the command.
                    toServer = GetParser();
                    break;
                case (int) BaseOptions.Use:
                    _lastOption.RemoveAt(0); // Remove the "use" portion of the command.
                    toServer = UseItem();
                    break;
                case (int) BaseOptions.Move:
                    toServer = MobileMove();
                    break;
                case (int) BaseOptions.Attack:
                    toServer = MobileCombat();
                    break;
                case (int) BaseOptions.Vendors:
                    toServer = UseVendor();
                    break;

                case (int) BaseOptions.Exit:
                    toServer = new SocketKillPacket();
                    break;

                case (int) BaseOptions.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(opt));
            }

            return toServer != null;
        }

        private static Packet GetParser()
        {
            // We cannot operate on an invalid supplied list of options.
            if (_lastOption == null || _lastOption.Count == 0)
                throw new ArgumentException("That command requires more input, please use 'get help'.");

            var opt = GetOptions.None;
            foreach (GetOptions o in Enum.GetValues(typeof(GetOptions)))
            {
                if (o == GetOptions.None)
                    continue;

                var str = Enum.GetName(typeof(GetOptions), o).ToLower();
                if (str.Contains(_lastOption[0]))
                    opt = o;
            }

            if (opt == GetOptions.None)
                throw new ArgumentException("User input was not valid.");


            switch (opt)
            {
                case GetOptions.Help:
                    Console.WriteLine("Options:");
                    foreach (var h in Enum.GetNames(typeof(GetOptions))
                        .SkipWhile(x => x == Enum.GetName(typeof(GetOptions), GetOptions.None)))
                        Console.Write($"[{h.ToLower()}] ");

                    Console.WriteLine();
                    return null;
                case GetOptions.Npcs:
                    return new GetInfoPacket(GetInfoPacket.RequestReason.Npcs);
                case GetOptions.Paperdoll:
                    return new GetInfoPacket(GetInfoPacket.RequestReason.Paperdoll);
                case GetOptions.Location:
                    return new GetInfoPacket(GetInfoPacket.RequestReason.Location);
                case GetOptions.Vendors:
                    return new GetInfoPacket(GetInfoPacket.RequestReason.Vendors);
                case GetOptions.Equipment:
                    return new GetInfoPacket(GetInfoPacket.RequestReason.Equipment);
                case GetOptions.Items:
                    return new GetInfoPacket(GetInfoPacket.RequestReason.Items);
                case GetOptions.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(opt));
            }
        }

        #endregion

        #region Socket Actions

        public static void ToServer(IPacket packet)
        {
            if (SocketHandler == null)
                throw new InvalidSocketHandlerException("Socket handler for the server was never assigned.");

            SocketHandler.Send(packet);
        }

        public static Packet FromServer()
        {
            if (SocketHandler == null)
                throw new InvalidSocketHandlerException("Socket handler for the server was never assigned.");

            if (!(SocketHandler.Receive() is Packet packet))
                throw new InvalidPacketException("Server sent an invalid packet.");

            return packet;
        }

        #endregion

        #region Actions

        private static Packet MobileMove(MovePacket movePacket = null)
        {
            if (movePacket == null || movePacket.Stage == Packet.Stages.One)
                return GetMovePacket(State.CurrentRegion.Navigable, State.NearbyAccessibleRegions);

            switch (movePacket.Stage)
            {
                case Packet.Stages.One:
                    return GetMovePacket(State.CurrentRegion.Navigable, State.NearbyAccessibleRegions);
                case Packet.Stages.Two:
                case Packet.Stages.Three:
                case Packet.Stages.Four:
                case Packet.Stages.Five:
                default:
                    State.CurrentRegion = movePacket.NewRegion; // Reassign our region.
                    if (movePacket.DiscoveredRegions == Regions.None)
                        return null;

                    // If the client discovered a new location, add it to our potential locations.
                    State.AddUnlockedRegion(movePacket.NewRegion.Connections);
                    Console.WriteLine($"Discovered: {movePacket.DiscoveredRegions}!");
                    return null;
            }
        }

        private static Packet MobileCombat(CombatPacket combatPacket = null)
        {
            if (combatPacket == null || combatPacket.Stage == Packet.Stages.One)
                // Get our targets from the server.
                return new CombatPacket();

            switch (combatPacket.Stage)
            {
                case Packet.Stages.One:
                    return new CombatPacket();
                case Packet.Stages.Two:
                    // List of the possible mobiles to attack.
                    if (combatPacket.Mobiles.Count == 0)
                    {
                        Utility.ConsoleNotify("No mobiles to attack.");
                        return null;
                    }

                    if (!combatPacket.Mobiles.ContainsKey(0))
                        Console.WriteLine(" [0] None");

                    foreach (var (value, mobile) in combatPacket.Mobiles)
                        Console.WriteLine($" [{value}] {mobile.Name}");

                    Console.WriteLine(); // Blank line to make it pretty.

                    // Get the choice from the user as to what mobile (if any) should be attacked.
                    var choice = Utility.ReadInt(combatPacket.Mobiles.Count + 1, true);
                    if (choice == 0)
                        return null; // If the user decided on "none", then return null.

                    // Assign the LocalNPC to the choice.
                    combatPacket.AddTarget(combatPacket.Mobiles[choice]);
                    return combatPacket;
                case Packet.Stages.Three:
                default:
                    // Server response on the action.
                    Console.WriteLine(combatPacket.Result);
                    foreach (var str in combatPacket.Updates)
                        Console.WriteLine(str);

                    Utility.LogWrite("combat.txt", combatPacket.Updates);
                    return null;
            }
        }

        private static Packet Resurrect(ResurrectPacket rez)
        {
            // The resurrection was not targeted to this player.
            if (rez.PlayerId != State.PlayerId)
                return null;

            State.Resurrect(); // Bring the player back to life.

            // If it was successful, get the new location.
            return rez.IsSuccessful ? new GetNodePacket(rez.Region) : null;
        }

        private static Packet UseItem(UseItemPacket packet = null)
        {
            if (packet == null || packet.Stage == Packet.Stages.One)
                // Get our items from the server.
                return new UseItemPacket();

            switch (packet.Stage)
            {
                case Packet.Stages.One:
                    return new UseItemPacket();
                case Packet.Stages.Two:
                    if (_lastOption != null
                        && _lastOption.Count > 0 &&
                        GetItem(packet, string.Join(" ", _lastOption)))
                        return packet;

                    var pos = 0;
                    Console.WriteLine($" [{pos++}] None");
                    foreach (var i in packet.Items.Values)
                        Console.WriteLine($" [{pos++}] {i}");

                    Console.WriteLine();

                    var opt = Utility.ReadInt(packet.Items.Count + 1, true);
                    if (opt == 0)
                        return null;

                    pos = 0;
                    foreach (var i in packet.Items.Keys)
                    {
                        if (++pos != opt)
                            continue;

                        packet.Item = i;
                        return packet;
                    }

                    return null;

                default:
                    Console.WriteLine($" {packet.Response}");
                    return null;
            }
        }

        private static Packet UseVendor(UseVendorPacket useVendor = null)
        {
            if (useVendor == null)
                return new UseVendorPacket();

            switch (useVendor.Stage)
            {
                case Packet.Stages.Two:
                    // Print out the options,
                    foreach (var (key, value) in useVendor.LocalVendors)
                        Console.WriteLine($"[{key}] {value}");

                    Console.WriteLine(); // Blank line to make it pretty.

                    // Get the choice from the user as to what vendor (if any) should be used.
                    var choice = Utility.ReadInt(useVendor.LocalVendors.Count, useVendor.LocalVendors.ContainsKey(0));
                    if (useVendor.LocalVendors[choice] == NPCTypes.None)
                        return null; // If the user decided on "none", then return null.

                    // Assign the LocalNPC to the choice.
                    useVendor.LocalNPC = useVendor.LocalVendors[choice];
                    break;

                case Packet.Stages.Three:
                    if (useVendor.Items == null || useVendor.Items.Count == 0)
                    {
                        Utility.ConsoleNotify("Unable to use that service.");
                        return null;
                    }

                    useVendor.Item = UseVendorPacket.PrintItems(useVendor.Items, true);
                    if (useVendor.Item.Default)
                        return null;

                    break;

                case Packet.Stages.Four:
                    Console.WriteLine($" Cost of transaction: {useVendor.Transaction}gp. Do you wish to pay this?\n");
                    useVendor.PerformAction = Utility.ReadEnum<UseVendorPacket.Choices>();

                    // Return null if the user opted out of using the vendor.
                    if (useVendor.PerformAction == UseVendorPacket.Choices.No)
                        return null;

                    break;

                default:
                    Console.WriteLine("Defaulting");
                    break;
            }

            return useVendor;
        }

        public static void GetInfo(GetInfoPacket getInfo)
        {
            switch (getInfo.Reason)
            {
                case GetInfoPacket.RequestReason.Paperdoll:
                    Console.WriteLine(getInfo.Info);
                    break;
                case GetInfoPacket.RequestReason.Location:
                    Console.WriteLine($" Location: {getInfo.Info}");
                    break;
                case GetInfoPacket.RequestReason.Items:
                case GetInfoPacket.RequestReason.Equipment:
                case GetInfoPacket.RequestReason.Vendors:
                case GetInfoPacket.RequestReason.Npcs:
                    if (getInfo.InfoList.Count == 0)
                    {
                        Utility.ConsoleNotify("There is not anything to show.");
                        break;
                    }

                    foreach (var i in getInfo.InfoList)
                        Console.WriteLine($" {i}");

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(getInfo.Reason));
            }
        }

        #endregion

        #region Miscellaneous

        private static bool GetItem(UseItemPacket useItem, string item)
        {
            foreach (var (value, i) in useItem.Items)
            {
                if (!i.ToLower().Contains(item))
                    continue;

                useItem.Item = value;
                return true;
            }

            return false;
        }

        private static MovePacket GetMovePacket(bool canNavigate, Regions nearbyUnlockedRegions)
        {
            var count = 0;
            Console.WriteLine($" [{count++}] None");
            if (canNavigate)
            {
                Console.WriteLine("\nDirections:");
                foreach (Directions dir in Enum.GetValues(typeof(Directions)))
                {
                    if (dir == Directions.None)
                        continue;

                    Console.WriteLine($" [{count++}] {Enum.GetName(typeof(Directions), dir)}");
                }
            }

            Console.WriteLine("\nNearby Regions:");
            foreach (Regions regionId in Enum.GetValues(typeof(Regions)))
            {
                if (regionId == Regions.None)
                    continue;

                if (!State.NearbyAccessibleRegions.HasFlag(regionId))
                    continue;

                Console.WriteLine($" [{count++}] {Enum.GetName(typeof(Regions), regionId)}");
            }

            var position = Utility.ReadInt(count + 1, true);
            if (position == 0)
                return null;

            count = 1;
            if (canNavigate)
                foreach (Directions dir in Enum.GetValues(typeof(Directions)))
                {
                    if (dir == Directions.None)
                        continue;

                    if (count++ == position)
                        return new MovePacket(State.Account.Serial, State.CurrentRegion.Id, dir);
                }

            foreach (Regions regionId in Enum.GetValues(typeof(Regions)))
            {
                if (regionId == Regions.None)
                    continue;

                if (!State.NearbyAccessibleRegions.HasFlag(regionId))
                    continue;

                if (count++ == position)
                    return new MovePacket(State.Account.Serial, regionId);
            }

            throw new IndexOutOfRangeException("Request movement cannot be decided.");
        }

        #endregion
    }
}