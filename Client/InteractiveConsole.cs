using System;
using SUS.Shared;
using SUS.Shared.Packets;

namespace SUS.Client
{
    #region Enums

    public enum BaseOptions
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

    public enum GetOptions
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

    public class InteractiveConsole
    {
        private readonly SocketHandler _socketHandler; // Remote connection.
        private Input _input; // Input from the user.
        private ClientState _state; // "Client State" to be controlled and used by the Console.

        #region Constructors

        public InteractiveConsole(SocketHandler socketHandler)
        {
            if (socketHandler == null)
                throw new ArgumentNullException(nameof(socketHandler), "Socket must be assigned.");

            _socketHandler = socketHandler;
        }

        #endregion

        #region Getters / Setters

        /// <summary>
        ///     Client State to be managed by the interactive console.
        /// </summary>
        private ClientState State
        {
            get => _state;
            set
            {
                if (value == null)
                    return;

                _state = value;
            }
        }

        #endregion

        /// <summary>
        ///     Main processor that the user interacts with.
        /// </summary>
        /// <returns>Packet information to be transmitted to the server.</returns>
        public Packet Core()
        {
            // Print out the available options that the user can choose from.
            Input.PrintUniqueOptions<BaseOptions>(false, 0);

            // Loop until we get valid user input as to which option to perform.
            do
            {
                var userInput = Input.Get();

                // Process the user input.
                _input = new Input(userInput);

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
        public bool PacketParser(Packet packet, out Packet toServer)
        {
            Console.WriteLine();
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

                    _socketHandler.Kill();
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

        #region Console Parsers

        /// <summary>
        ///     Parses user input assigned to '_lastOption'
        /// </summary>
        /// <param name="toServer">Packet to be sent to the server.</param>
        /// <returns>Success or failure of the action.</returns>
        private bool InputParser(out Packet toServer)
        {
            switch (_input.Base)
            {
                case BaseOptions.Help:
                    Input.PrintUniqueOptions<BaseOptions>(false, 0);
                    toServer = null;
                    break;

                case BaseOptions.Get:
                    toServer = GetParser();
                    break;
                case BaseOptions.Use:
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
                    throw new ArgumentOutOfRangeException(nameof(_input.Base));
            }

            return toServer != null;
        }

        private Packet GetParser()
        {
            var getOption = _input.GetOption;

            // Present the options and attempt to parse the input.
            if (getOption == GetOptions.None)
                getOption = Input.PrintAndGet<GetOptions>(false, 0);

            switch (getOption)
            {
                // Print the help message.
                case GetOptions.Help:
                    Input.PrintUniqueOptions<GetOptions>(false, 0);
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
                    throw new ArgumentOutOfRangeException(nameof(_input.GetOption));
            }
        }

        #endregion

        #region Socket Actions

        /// <summary>
        ///     A wrapper for the Socket Handler to send information to the remote connection.
        /// </summary>
        /// <param name="packet">Information to be sent.</param>
        public void ToServer(IPacket packet)
        {
            _socketHandler.Send(packet);
        }

        /// <summary>
        ///     A wrapper for the Socket Handler to receive information from the remote connection.
        /// </summary>
        /// <returns>Packet retrieved from the remote connection.</returns>
        public Packet FromServer()
        {
            if (!(_socketHandler.Receive() is Packet packet))
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
        private Packet MobileMove(MovePacket movePacket = null)
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
                case Packet.Stages.Two:
                    State.CurrentRegion = movePacket.NewRegion; // Reassign our region.
                    if (movePacket.DiscoveredRegions == Regions.None)
                        return null;

                    // If the client discovered a new location, add it to our potential locations.
                    State.AddUnlockedRegion(movePacket.NewRegion.Connections);
                    Console.WriteLine($"Discovered: {movePacket.DiscoveredRegions}!");
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(movePacket.Stage),
                        "An error occurred while trying to move.");
            }
        }

        /// <summary>
        ///     Perform combat against another mobile.
        /// </summary>
        /// <param name="combatPacket">Packet received from the remote connection to process.</param>
        /// <returns>Information to be sent to the remote connection.</returns>
        private Packet MobileCombat(CombatPacket combatPacket = null)
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
                    // Attempt the parse "last" from left-over user input.
                    while (_input.Iterate(out var input))
                    {
                        // See if the user input is "last".
                        if (!input.Contains("last"))
                            continue;

                        if (!combatPacket.AllowLast)
                            continue;

                        combatPacket.LastSelected = true;
                        return combatPacket;
                    }

                    // We cannot perform attacks on zero nearby targets.
                    if (combatPacket.Mobiles.Count == 0)
                    {
                        Utility.ConsoleNotify("No mobiles to attack.");
                        return null;
                    }

                    if (!combatPacket.SetTarget())
                        return null;

                    return combatPacket;

                // Action has been performed, print the result of the combat.
                case Packet.Stages.Three:
                    // Server response on the action.
                    Console.WriteLine(combatPacket.Result);
                    foreach (var str in combatPacket.Updates)
                        Console.WriteLine(str);

                    // Write it to a local log for debugging.
                    Utility.LogWrite("combat.txt", combatPacket.Updates);
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(combatPacket.Stage),
                        "An error occurred while trying to perform combat.");
            }
        }

        /// <summary>
        ///     Packet sent by the remote connection to bring the player back to life.
        /// </summary>
        /// <param name="resurrectPacket">Packet received from the remote connection to process.</param>
        /// <returns>Information to be sent to the remote connection.</returns>
        private Packet Resurrect(ResurrectPacket resurrectPacket)
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
        private Packet UseItem(UseItemPacket useItem = null)
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
                    while (_input.Iterate(out var item))
                    {
                        // See if the user input is an item that is owned.
                        if (GetItem(useItem, item))
                            return useItem;
                    }

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

                        // Item has been found, return a packet to the server.
                        useItem.Item = i;
                        return useItem;
                    }

                    return null;

                // Server has responded with the action being completed either successfully or failure.
                case Packet.Stages.Three:
                    Console.WriteLine($" {useItem.Response}");
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(useItem.Stage),
                        "An error occurred while trying to use the vendor.");
            }
        }

        /// <summary>
        ///     Uses a vendor to perform various vendor-based actions such as repairing, selling, buying, etc.
        /// </summary>
        /// <param name="useVendor">Packet received from the remote connection to process.</param>
        /// <returns>Information to be sent to the remote connection.</returns>
        private Packet UseVendor(UseVendorPacket useVendor = null)
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
                    // Attempt the parse the vendor name from left-over user input.
                    while (_input.Iterate(out var vendor))
                    {
                        // See if the user input is a vendor.
                        if (GetVendor(useVendor, vendor))
                            return useVendor;
                    }

                    // Print out the vendors.
                    foreach (var (key, value) in useVendor.LocalVendors)
                        Console.WriteLine($"[{key}] {value}");

                    Console.WriteLine(); // Blank line to make it pretty.

                    // Get the choice from the user as to what vendor (if any) should be used.
                    var choice = Utility.ReadInt(useVendor.LocalVendors.Count, useVendor.LocalVendors.ContainsKey(0));
                    if (useVendor.LocalVendors[choice] == NpcTypes.None)
                        return null; // If the user decided on "none", then return null.

                    // Assign the LocalNPC to the choice.
                    useVendor.LocalNpc = useVendor.LocalVendors[choice];
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
                    if (!useVendor.SetItem())
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
        private void GetInfo(GetInfoPacket getInfo)
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
        ///     Attempts to parse a vendor from a string and applies it to the packet.
        /// </summary>
        /// <param name="useVendor">Packet to add the vendor to.</param>
        /// <param name="vendor">Vendor to attempt to parse.</param>
        /// <returns>Success or failure of the action.</returns>
        private static bool GetVendor(UseVendorPacket useVendor, string vendor)
        {
            foreach (var (i, value) in useVendor.LocalVendors)
            {
                var name = Enum.GetName(typeof(NpcTypes), value);
                if (!name.ToLower().Contains(vendor))
                    continue;

                // Valid vendor.
                useVendor.LocalNpc = value;
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
        private MovePacket GetMovePacket(bool canNavigate, Regions nearbyAccessibleRegions)
        {
            // Try to parse the remainder of the user input.
            while (_input.Iterate(out var input))
            {
                // Attempt to parse a direction.
                if (Input.ConvertEnum<Directions>(input, out var dir, false, 0))
                {
                    // Be sure the parsed direction is possible in this region.
                    if (!canNavigate)
                        Utility.ConsoleNotify("You cannot navigate in this region.");
                    else
                        return new MovePacket(State.Account.Serial, State.CurrentRegion.Id, dir);
                }

                // Try to parse a nearby region.
                if (!Input.ConvertEnum<Regions>(input, out var reg, false, 0))
                    continue;

                // Verify it can be accessed.
                if ((State.NearbyAccessibleRegions & reg) != reg)
                    Utility.ConsoleNotify("That region is not accessible from your current location.");
                else
                    return new MovePacket(State.Account.Serial, reg);
            }

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

        #endregion
    }
}