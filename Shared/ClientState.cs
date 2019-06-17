﻿using System;
using System.Collections.Generic;
using System.IO;
using SUS.Shared.Packets;

namespace SUS.Shared
{
    [Serializable]
    public class ClientState
    {
        private BaseMobile _account;
        private BaseRegion _currentRegion;
        private Dictionary<int, string> _equippedItems; // Equipped items.
        private bool _isAlive;
        private Dictionary<int, string> _items; // Items in the inventory.
        private BaseRegion _lastRegion;
        private BaseMobile _lastTarget;

        // Objects that need to be requested from the server.
        private HashSet<BaseMobile> _localMobiles; // Local / Nearby creatures.
        private ulong _playerId;

        #region Constructors

        public ClientState(ulong playerId, BaseMobile account, BaseRegion currentRegion, Regions unlockedRegions)
        {
            PlayerId = playerId;
            Account = account;
            CurrentRegion = currentRegion;
            UnlockedRegions |= unlockedRegions;
        }

        #endregion

        public void AddUnlockedRegion(Regions unlockedRegions)
        {
            UnlockedRegions |= unlockedRegions;
        }

        #region Packet Parsing

        public void ParseGetMobilePacket(GetMobilePacket gmp)
        {
            var reason = gmp.Reason;

            Console.WriteLine();
            while (reason != GetMobilePacket.RequestReason.None)
                foreach (GetMobilePacket.RequestReason r in Enum.GetValues(typeof(GetMobilePacket.RequestReason)))
                {
                    if (r == GetMobilePacket.RequestReason.None || (r & (r - 1)) != 0) continue;

                    switch (reason & r)
                    {
                        case GetMobilePacket.RequestReason.Paperdoll:
                            Console.WriteLine("Paper Doll Information:");
                            Console.WriteLine(gmp.Paperdoll);
                            break;
                        case GetMobilePacket.RequestReason.Location:
                            Console.WriteLine("Location Information:");
                            Console.WriteLine(gmp.Region.ToString());
                            break;
                        case GetMobilePacket.RequestReason.IsDead:
                            Console.WriteLine($"Is Alive?: {gmp.IsAlive.ToString()}.");
                            break;
                        case GetMobilePacket.RequestReason.Items:
                            Console.WriteLine("Received updated items.");
                            _items = gmp.Items;
                            break;
                        case GetMobilePacket.RequestReason.Equipment:
                            Console.WriteLine("Received updated equipment.");
                            _equippedItems = gmp.Equipment;
                            break;
                    }

                    reason &= ~r;
                }
        }

        #endregion

        #region Getters / Setters

        public ulong PlayerId
        {
            get => _playerId;
            private set => _playerId = value;
        }

        public BaseMobile Account
        {
            get => _account;
            private set
            {
                if (!value.IsPlayer) return;

                _account = value;
            }
        }

        /// <summary>
        ///     Gets all of the current nearby regions, excluding the one that we are currently in.
        /// </summary>
        public Regions NearbyUnlockedRegions => CurrentRegion.Connections & UnlockedRegions & ~CurrentRegion.Location;

        public BaseRegion CurrentRegion
        {
            get => _currentRegion;
            set
            {
                if (!value.IsValid || value.Location == CurrentRegion.Location) return;

                LastRegion = CurrentRegion; // Swap the Node.
                _currentRegion = value; // Assign the new
            }
        }

        public BaseRegion LastRegion
        {
            get => _lastRegion;
            private set
            {
                if (!value.IsValid || value.Location == LastRegion.Location) return;

                _lastRegion = value; // Updates our Last Node accessed.
            }
        }

        public BaseMobile LastTarget
        {
            get => _lastTarget;
            set => _lastTarget = value;
        }

        public HashSet<BaseMobile> LocalMobiles
        {
            get => _localMobiles ?? (_localMobiles = new HashSet<BaseMobile>());
            set
            {
                if (value == null) return;

                _localMobiles = value;
            }
        }

        public bool IsAlive
        {
            get => _isAlive;
            private set => _isAlive = value;
        }

        public Regions UnlockedRegions { get; private set; }

        #endregion

        #region Mobile Actions

        public void MobileActionHandler(CombatMobilePacket cmp)
        {
            if (!cmp.IsAlive) IsAlive = false;

            var u = cmp.Updates;
            Console.WriteLine("\nServer response:");
            if (u == null)
            {
                Utility.ConsoleNotify("Server sent back a bad combat log.");
                return;
            }

            // Get the Desktop Folder location for the local user.
            var desktopLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Concatenate "combat.log" to the the Desktop location.
            var fn = Path.Combine(desktopLocation, "combat.log");
            using (var sw = File.AppendText(fn))
            {
                // Appends to the file if it exists, otherwise it will be created and written to.
                sw.WriteLine($"[{DateTime.Now}]"); // Timestamp the log.
                foreach (var str in u) sw.WriteLine(str); // Write the server responses to the log.
                sw.WriteLine(); // Blank line for the next log.
            }

            // Print the contents to the console.
            foreach (var str in u) Console.WriteLine(str);
        }

        public Packet Resurrect(ResurrectMobilePacket rez)
        {
            if (rez.PlayerId != PlayerId) return null; // The resurrection was not targeted to this player.

            IsAlive = true;
            return rez.IsSuccessful ? new GetNodePacket(rez.Region, PlayerId) : null;
        }

        public static void UseItemResponse(UseItemPacket uip)
        {
            Console.WriteLine(uip.Response);
        }

        public Packet UseItems()
        {
            if (_items == null) return new GetMobilePacket(GetMobilePacket.RequestReason.Items, PlayerId);

            var pos = 0;
            foreach (var i in _items.Values)
            {
                ++pos;
                Console.WriteLine($" [{pos}] {i}");
            }

            Console.WriteLine();

            var opt = Utility.ReadInt(_items.Count);
            pos = 0;
            foreach (var i in _items.Keys)
            {
                ++pos;
                if (pos == opt) return new UseItemPacket(i, PlayerId);
            }

            return null;
        }

        public Packet UseVendorProcessor(UseVendorPacket uvp)
        {
            switch (uvp.Stage)
            {
                case Packet.Stages.Two:
                    // Print out the options,
                    foreach (var (key, value) in uvp.LocalVendors) Console.WriteLine($"[{key}] {value}");
                    Console.WriteLine(); // Blank line to make it pretty.

                    // Get the choice from the user as to what vendor (if any) should be used.
                    var choice = Utility.ReadInt(uvp.LocalVendors.Count, uvp.LocalVendors.ContainsKey(0));
                    if (uvp.LocalVendors[choice] == NPCTypes.None)
                        return null; // If the user decided on "none", then return null.

                    // Assign the LocalNPC to the choice.
                    uvp.LocalNPC = uvp.LocalVendors[choice];
                    break;

                case Packet.Stages.Three:
                    if (uvp.Items == null || uvp.Items.Count == 0)
                    {
                        Utility.ConsoleNotify("Unable to use that service.");
                        return null;
                    }

                    uvp.Item = UseVendorPacket.PrintItems(uvp.Items, true);
                    if (uvp.Item.Default)
                        return null;

                    break;

                case Packet.Stages.Four:
                    Console.WriteLine($" Cost of transaction: {uvp.Transaction}gp. Do you wish to pay this?\n");
                    uvp.PerformAction = Utility.ReadEnum<UseVendorPacket.Choices>();

                    // Return null if the user opted out of using the vendor.
                    if (uvp.PerformAction == UseVendorPacket.Choices.No)
                        return null;
                    break;

                default:
                    Console.WriteLine("Defaulting");
                    break;
            }

            return uvp;
        }

        #endregion

        #region Node / Location Actions

        /// <summary>
        ///     Attempts to convert a string (either integer or location name) to a location that has a connection.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Regions StringToLocation(string location)
        {
            if (int.TryParse(location, out var pos) && pos < 0)
                return Regions.None; // User attempted a negative number.

            if (pos == 0) return CurrentRegion.Location;

            var count = 0;
            foreach (var loc in Utility.EnumToIEnumerable<Regions>(NearbyUnlockedRegions, true))
            {
                // A connection cannot be 'None'
                if (loc == Regions.None)
                    continue;

                // Check if this is not a power of two (indicating it is a combination location)
                if ((loc & (loc - 1)) != 0)
                    continue; //  It was a combination.

                ++count;
                // Attempts to check the integer conversion.
                if (count == pos)
                    return loc; //  if a match is found, return it.
            }

            return Regions.None;
        }

        public MobileDirections StringToDirection(string location)
        {
            if (!CurrentRegion.Navigable) return MobileDirections.None;

            foreach (MobileDirections dir in Enum.GetValues(typeof(MobileDirections)))
            {
                if (dir == MobileDirections.None) continue;

                if (Enum.GetName(typeof(MobileDirections), dir)?.ToLower() == location.ToLower()) return dir;
            }

            return MobileDirections.None;
        }

        #endregion
    }
}