using System;
using System.Collections.Generic;
using System.Linq;
using SUS.Shared;
using SUS.Shared.Packets;

namespace SUS.Client
{
    public static class InteractiveConsole
    {
        private static List<string> _lastOption; // Stores the last option chosen by the user.
        private static ClientState _state; // "Client State" to be controlled and used by the Console.
        public static SocketHandler SocketHandler; // Remote connection.

        /// <summary>
        ///     Main processor that the user interacts with.
        /// </summary>
        /// <returns>Packet information to be transmitted to the server.</returns>
        public static Packet Core()
        {
            // Print out the available options that the user can choose from.
            Console.WriteLine("\nChoose an option:");
            foreach (var e in UniqueEnumNames<BaseOptions>(false, 0))
                Console.Write($"[{e.ToLower()}] ");

            Console.WriteLine(); // Blank line for styling.

            // Loop until we get valid user input as to which option to perform.
            do
            {
                string userInput;
                do
                {
                    Console.Write("\n > ");
                    userInput = Console.ReadLine();
                } while (string.IsNullOrWhiteSpace(userInput));

                Console.WriteLine();

                // Convert to lowercase and split the input into a list.
                userInput = userInput.ToLower();
                _lastOption = userInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                try
                {
                    // Parse the '_lastOption' the user decided on, if it fails; try again.
                    if (!InputParser(out var toServer))
                        continue;

                    // Valid input received. Attach the PlayerId to the packet and return it to be sent.
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

        /// <summary>
        ///     Parses a packet to present or save the information sent by the remove host.
        /// </summary>
        /// <param name="packet">Packet to process.</param>
        /// <param name="toServer">Information to be sent back to the remove host.</param>
        /// <returns>Success or failure of the action..</returns>
        public static bool PacketParser(Packet packet, out Packet toServer)
        {
            toServer = null;

            switch (packet)
            {
                // Basic information from server.
                case OkPacket pkt:
                    if (pkt.Message != string.Empty)
                        Utility.ConsoleNotify(pkt.Message);

                    break;
                case ErrorPacket pkt:
                    if (pkt.Message != string.Empty)
                        Utility.ConsoleNotify(pkt.Message);

                    break;

                // Client / Server Authentication.
                case AccountClientPacket pkt:
                    State = pkt.ClientState;
                    break;
                case SocketKillPacket pkt:
                    Utility.ConsoleNotify("Exiting.");
                    if (pkt.Message != string.Empty)
                        Utility.ConsoleNotify("Reason: " + pkt.Message);

                    SocketHandler.Kill();
                    break;

                // Information requested by the server to be displayed.
                case GetInfoPacket pkt:
                    GetInfo(pkt);
                    break;

                // Actions the user can take.
                case CombatPacket pkt:
                    toServer = MobileCombat(pkt);
                    break;
                case MovePacket pkt:
                    toServer = MobileMove(pkt);
                    break;
                case ResurrectPacket pkt:
                    toServer = Resurrect(pkt); // If we require a new current node,
                    break;
                case UseItemPacket pkt:
                    toServer = UseItem(pkt);
                    break;
                case UseVendorPacket pkt:
                    toServer = UseVendor(pkt);
                    break;

                case AccountAuthenticatePacket pkt:
                default:
                    throw new ArgumentOutOfRangeException(nameof(packet),
                        "Unimplemented information received from host.");
            }

            return toServer != null;
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

        /// <summary>
        ///     Client State to be managed by the interactive console.
        /// </summary>
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

        /// <summary>
        ///     Socket Handler to be utilized by the interactive console.
        /// </summary>
        /// <param name="handler"></param>
        public static void SetHandler(SocketHandler handler)
        {
            if (SocketHandler != null)
                return;

            SocketHandler = handler;
        }

        #endregion

        #region Console Actions

        /// <summary>
        ///     Parses user input assigned to '_lastOption'
        /// </summary>
        /// <param name="toServer">Packet to be sent to the server.</param>
        /// <returns>Success or failure of the action.</returns>
        private static bool InputParser(out Packet toServer)
        {
            // We cannot operate on an invalid supplied list of options.
            if (_lastOption == null || _lastOption.Count == 0)
                throw new ArgumentException("User input cannot be null or empty.");

            // Attempt to convert the input to an Enum value ignoring duplicates.
            if (!ConvertEnum<BaseOptions>(_lastOption[0], out var opt, false, 0))
                throw new ArgumentException("User input was not valid.");

            switch (opt)
            {
                case BaseOptions.Help:
                    Console.WriteLine("Options:");
                    foreach (var e in UniqueEnumNames<BaseOptions>(false, 0))
                        Console.Write($"[{e.ToLower()}] ");

                    Console.WriteLine();
                    toServer = null;
                    break;

                case BaseOptions.Get:
                    _lastOption.RemoveAt(0); // Remove the "get" portion of the command.
                    toServer = GetParser();
                    break;
                case BaseOptions.Use:
                    _lastOption.RemoveAt(0); // Remove the "use" portion of the command.
                    toServer = UseItem();
                    break;
                case BaseOptions.Move:
                    toServer = MobileMove();
                    break;
                case BaseOptions.Attack:
                    toServer = MobileCombat();
                    break;
                case BaseOptions.Vendors:
                    toServer = UseVendor();
                    break;

                case BaseOptions.Exit:
                    toServer = new SocketKillPacket();
                    break;

                case BaseOptions.None:
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

            // Attempt to convert the input to an Enum value ignoring duplicates.
            if (!ConvertEnum<GetOptions>(_lastOption[0], out var opt, false, 0))
                throw new ArgumentException("User input was not valid.");

            switch (opt)
            {
                // Print the help message.
                case GetOptions.Help:
                    Console.WriteLine("Options:");
                    foreach (var e in UniqueEnumNames<GetOptions>(false))
                        Console.Write($"[{e.ToLower()}] ");

                    Console.WriteLine();
                    return null;

                // Information to retrieve from the remote connection.
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

        /// <summary>
        ///     A wrapper for the Socket Handler to send information to the remote connection.
        /// </summary>
        /// <param name="packet">Information to be sent.</param>
        public static void ToServer(IPacket packet)
        {
            if (SocketHandler == null)
                throw new InvalidSocketHandlerException("Socket handler for the server was never assigned.");

            SocketHandler.Send(packet);
        }

        /// <summary>
        ///     A wrapper for the Socket Handler to receive information from the remote connection.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     Negotiates between the remote connection and the console for moving the player.
        /// </summary>
        /// <param name="movePacket">Packet received by the remote connection to process.</param>
        /// <returns>Information to be sent to the remote connection.</returns>
        private static Packet MobileMove(MovePacket movePacket = null)
        {
            // Initiate stage, get either a location or direction from the user. Send it off.
            if (movePacket == null || movePacket.Stage == Packet.Stages.One)
                return GetMovePacket(State.CurrentRegion.Navigable, State.NearbyAccessibleRegions);

            switch (movePacket.Stage)
            {
                // Safeguard to perform the same exact process as above if for some reason it was skipped.
                case Packet.Stages.One:
                    return GetMovePacket(State.CurrentRegion.Navigable, State.NearbyAccessibleRegions);

                // Process the result from the remote connection.
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

        /// <summary>
        ///     Perform combat against another mobile.
        /// </summary>
        /// <param name="combatPacket">Packet received from the remote connection to process.</param>
        /// <returns>Information to be sent to the remote connection.</returns>
        private static Packet MobileCombat(CombatPacket combatPacket = null)
        {
            // Initiate stage, request potential targets from the server.
            if (combatPacket == null || combatPacket.Stage == Packet.Stages.One)
                return new CombatPacket();

            switch (combatPacket.Stage)
            {
                // Safeguard to perform the same exact process as above if for some reason it was skipped.
                case Packet.Stages.One:
                    return new CombatPacket();

                // Packet received, potential targets included.
                case Packet.Stages.Two:
                    // We cannot perform attacks on zero nearby targets.
                    if (combatPacket.Mobiles.Count == 0)
                    {
                        Utility.ConsoleNotify("No mobiles to attack.");
                        return null;
                    }

                    // For display purposes when the user is prompted for an action.
                    if (!combatPacket.Mobiles.ContainsKey(0))
                        Console.WriteLine(" [0] None");

                    // List of the possible mobiles to attack.
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

                // Action has been performed, print the result of the combat.
                default:
                    // Server response on the action.
                    Console.WriteLine(combatPacket.Result);
                    foreach (var str in combatPacket.Updates)
                        Console.WriteLine(str);

                    // Write it to a local log for debugging.
                    Utility.LogWrite("combat.txt", combatPacket.Updates);
                    return null;
            }
        }

        /// <summary>
        ///     Packet sent by the remote connection to bring the player back to life.
        /// </summary>
        /// <param name="resurrectPacket">Packet received from the remote connection to process.</param>
        /// <returns>Information to be sent to the remote connection.</returns>
        private static Packet Resurrect(ResurrectPacket resurrectPacket)
        {
            // The resurrection was not targeted to this player.
            if (resurrectPacket.PlayerId != State.PlayerId)
                return null;

            State.Resurrect(); // Bring the player back to life.

            // If it was successful, get the new location.
            return resurrectPacket.IsSuccessful ? new GetNodePacket(resurrectPacket.Region) : null;
        }

        /// <summary>
        ///     Uses an item that is owned by the player.
        /// </summary>
        /// <param name="useItem">Packet received from the remote connection to process.</param>
        /// <returns>Information to be sent to the remote connection.</returns>
        private static Packet UseItem(UseItemPacket useItem = null)
        {
            // Initial stage, get the items from the remote connection that the player owns.
            if (useItem == null || useItem.Stage == Packet.Stages.One)
                return new UseItemPacket();

            switch (useItem.Stage)
            {
                // Safeguard that performs the same action as above in the event it was somehow skipped.
                case Packet.Stages.One:
                    return new UseItemPacket();

                // Decide which item to utilize.
                case Packet.Stages.Two:
                    // Attempt the parse the item name from left-over user input.
                    if (_lastOption != null && _lastOption.Count > 0 && GetItem(useItem, string.Join(" ", _lastOption)))
                        return useItem;

                    var pos = 0;
                    Console.WriteLine($" [{pos++}] None");

                    // Print the item choices.
                    foreach (var i in useItem.Items.Values)
                        Console.WriteLine($" [{pos++}] {i}");

                    Console.WriteLine();

                    // Read the users choice.
                    var opt = Utility.ReadInt(useItem.Items.Count + 1, true);
                    if (opt == 0)
                        return null; // User opted to not use any items.

                    pos = 0;
                    // Locate the chosen item from the list of items.
                    foreach (var i in useItem.Items.Keys)
                    {
                        if (++pos != opt)
                            continue;

                        // Item has been found, return a packet to ther server.
                        useItem.Item = i;
                        return useItem;
                    }

                    return null;

                // Server has responded with the action being completed either successfully or failure.
                default:
                    Console.WriteLine($" {useItem.Response}");
                    return null;
            }
        }

        /// <summary>
        ///     Uses a vendor to perform various vendor-based actions such as repairing, selling, buying, etc.
        /// </summary>
        /// <param name="useVendor">Packet received from the remote connection to process.</param>
        /// <returns>Information to be sent to the remote connection.</returns>
        private static Packet UseVendor(UseVendorPacket useVendor = null)
        {
            // Initial stage, get the local vendors from the remote connection.
            if (useVendor == null || useVendor.Stage == Packet.Stages.One)
                return new UseVendorPacket();

            switch (useVendor.Stage)
            {
                // Safeguard to perform the same action as above in the event it somehow didn't happen.
                case Packet.Stages.One:
                    return new UseItemPacket();

                // Prompt the user to decide which vendor and services to use.
                case Packet.Stages.Two:
                    // Print out the vendors.
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

                // Prompt the user to decide which item to choose for the service.
                case Packet.Stages.Three:
                    // Cannot act on empty or zero items, just print and return.
                    if (useVendor.Items == null || useVendor.Items.Count == 0)
                    {
                        Utility.ConsoleNotify("Unable to use that service.");
                        return null;
                    }

                    // Get the item choice from the user.
                    useVendor.Item = UseVendorPacket.PrintItems(useVendor.Items, true);
                    if (useVendor.Item.Default)
                        return null;

                    break;

                // Prompt the user to confirm the cost of the service and if that is ok.
                case Packet.Stages.Four:
                    Console.WriteLine($" Cost of transaction: {useVendor.Transaction}gp. Do you wish to pay this?\n");
                    useVendor.PerformAction = Utility.ReadEnum<UseVendorPacket.Choices>();

                    // Return null if the user opted out of using the vendor.
                    if (useVendor.PerformAction == UseVendorPacket.Choices.No)
                        return null;

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(useVendor.Stage),
                        "An error occurred while trying to use the vendor.");
            }

            return useVendor;
        }

        /// <summary>
        ///     Processes 'get' information received from the remote connection.
        /// </summary>
        /// <param name="getInfo">Information received to parse.</param>
        private static void GetInfo(GetInfoPacket getInfo)
        {
            switch (getInfo.Reason)
            {
                // Single string based information.
                case GetInfoPacket.RequestReason.Paperdoll:
                    Console.WriteLine(getInfo.Info);
                    break;
                case GetInfoPacket.RequestReason.Location:
                    Console.WriteLine($" Location: {getInfo.Info}");
                    break;

                // List based information.
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

                // An error of sorts occurred.
                default:
                    throw new ArgumentOutOfRangeException(nameof(getInfo.Reason));
            }
        }

        #endregion

        #region Miscellaneous

        /// <summary>
        ///     Attempts to parse an item from a string and applies it to the packet.
        /// </summary>
        /// <param name="useItem">Packet to add the item to.</param>
        /// <param name="item">Item to attempt to parse.</param>
        /// <returns>Success or failure of the action.</returns>
        private static bool GetItem(UseItemPacket useItem, string item)
        {
            // Iterate each of the items provided.
            foreach (var (value, i) in useItem.Items)
            {
                // If it isn't a match, continue.
                if (!i.ToLower().Contains(item))
                    continue;

                // Valid item, assign it.
                useItem.Item = value;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets either a Location or Direction from the user as to where to move.
        /// </summary>
        /// <param name="canNavigate">Can the user navigate in the current zone?</param>
        /// <param name="nearbyAccessibleRegions">Nearby regions that are available to the user.</param>
        /// <returns></returns>
        private static MovePacket GetMovePacket(bool canNavigate, Regions nearbyAccessibleRegions)
        {
            var count = 0;
            Console.WriteLine($" [{count++}] None");

            // If the zone can be navigated within, print out the potential directions you can move.
            if (canNavigate)
            {
                Console.WriteLine("\nDirections:");
                foreach (Directions dir in Enum.GetValues(typeof(Directions)))
                {
                    // "None" is not valid in this context, skip it.
                    if (dir == Directions.None)
                        continue;

                    Console.WriteLine($" [{count++}] {Enum.GetName(typeof(Directions), dir)}");
                }
            }

            // Print out the nearby regions the player can travel.
            Console.WriteLine("\nNearby Regions:");
            foreach (Regions regionId in Enum.GetValues(typeof(Regions)))
            {
                // "None" is not valid in this context, skip it.
                if (regionId == Regions.None)
                    continue;

                // If the region isn't in the list of accessible regions, skip it.
                if (!State.NearbyAccessibleRegions.HasFlag(regionId))
                    continue;

                Console.WriteLine($" [{count++}] {Enum.GetName(typeof(Regions), regionId)}");
            }

            // Read the choice from the user.
            var position = Utility.ReadInt(count, true);
            if (position == 0)
                return null; // User opted out of moving.

            count = 1;
            // Try to get the direction based on the number the user provided.
            if (canNavigate)
                foreach (Directions dir in Enum.GetValues(typeof(Directions)))
                {
                    // Skip "none" direction.
                    if (dir == Directions.None)
                        continue;

                    if (count++ == position)
                        return new MovePacket(State.Account.Serial, State.CurrentRegion.Id, dir);
                }

            // Try to get the region based on the number provided.
            foreach (Regions regionId in Enum.GetValues(typeof(Regions)))
            {
                // Skip "none" region.
                if (regionId == Regions.None)
                    continue;

                // Skip inaccessible regions.
                if (!State.NearbyAccessibleRegions.HasFlag(regionId))
                    continue;

                if (count++ == position)
                    return new MovePacket(State.Account.Serial, regionId);
            }

            throw new IndexOutOfRangeException("Request movement cannot be decided.");
        }

        /// <summary>
        ///     Gets a list of enum names that are unique in their underlying value. Ignoring duplicates and aliases.
        /// </summary>
        /// <typeparam name="TEnum">Type of the Enum.</typeparam>
        /// <param name="allowNone">Is none a valid argument to consider?</param>
        /// <param name="noneValue">If None is in the enum, the value it holds.</param>
        /// <returns>List of unique enum names pertaining to the TEnum type.</returns>
        private static List<string> UniqueEnumNames<TEnum>(bool allowNone, int noneValue = -1)
            where TEnum : struct, IConvertible, IFormattable
        {
            var uniqueNames = new List<string>();

            var enumPos = 0;
            foreach (var e in (TEnum[]) Enum.GetValues(typeof(TEnum)))
            {
                // Get the integer and string value of the enum.
                var enumValue = Convert.ToInt32(e);
                var enumName = Enum.GetName(typeof(TEnum), e);

                // If it is a duplicate or alias, skip it.
                if (enumPos != enumValue)
                    continue;

                // If the value is "none" and we are ignoring "none", continue.
                if (!allowNone && noneValue >= 0 && enumPos == noneValue && enumName.ToLower() == "none")
                {
                    ++enumPos;
                    continue;
                }

                // Add the value to the list to be returned.
                uniqueNames.Add(enumName);
                ++enumPos;
            }

            return uniqueNames;
        }

        /// <summary>
        ///     Takes in a string, attempts to match it to a value of an Enum. Works for integer based Enums.
        /// </summary>
        /// <typeparam name="TEnum">Type of Enum</typeparam>
        /// <param name="value">String to convert to Enum.</param>
        /// <param name="input">If successful, holds the converted value.</param>
        /// <param name="allowNone">Is none a valid argument to consider?</param>
        /// <param name="noneValue">If None is in the enum, the value it holds.</param>
        /// <returns>Success or failure of the action.</returns>
        private static bool ConvertEnum<TEnum>(string value, out TEnum input, bool allowNone, int noneValue = -1)
            where TEnum : struct, IConvertible, IFormattable
        {
            // Set the default value to the value of "none" in the event we return false.
            input = (TEnum) Enum.Parse(typeof(TEnum), noneValue.ToString(), true);

            var enumPos = -1;
            foreach (var enumName in Enum.GetNames(typeof(TEnum)))
            {
                ++enumPos; // Increment the position of the enum.

                // Get the enum in the form of the enum type and integer.
                var enumCode = (TEnum) Enum.Parse(typeof(TEnum), enumName, true);
                var enumValue = Convert.ToInt32(enumCode);

                // If the value is "none" and we are ignoring "none", continue.
                if (!allowNone && noneValue >= 0 && enumPos == noneValue && enumName.ToLower() == "none")
                    continue;

                // Compare if the enum contains the passed value, continue if not.
                if (!enumName.ToLower().Contains(value.ToLower()))
                    continue;

                // Success, convert it to the first occurrence in the enum (aliases / duplicates are treated as same value.
                input = (TEnum) Enum.Parse(typeof(TEnum), enumValue.ToString(), true);
                return true;
            }

            return false;
        }

        #endregion
    }
}