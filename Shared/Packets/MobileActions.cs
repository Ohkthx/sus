using System;
using System.Collections.Generic;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class CombatMobilePacket : Packet
    {
        private string _result; // Result of the combat.
        private List<int> _targets; // List of Targets
        private List<string> _updates; // Updates on all.

        #region Constructors

        public CombatMobilePacket(ulong playerId)
            : base(PacketTypes.MobileCombat, playerId)
        {
        }

        #endregion

        public bool IsAlive { get; set; } // Determines if the Initiator (Player) died.

        public void AddTarget(BaseMobile tag)
        {
            if (!Targets.Contains(tag.Serial)) Targets.Add(tag.Serial);
        }

        public void AddUpdate(List<string> info)
        {
            if (info == null) return;

            Updates.AddRange(info);
        }

        #region Getters / Setters

        public List<int> Targets => _targets ?? (_targets = new List<int>());

        public List<string> Updates => _updates ?? (_updates = new List<string>());

        public string Result
        {
            get => _result;
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                _result = value;
            }
        }

        #endregion
    }

    /// <summary>
    ///     Requests a mobile to be moved if sent to the server. If sent to the client, it is an update.
    ///     DiscoveredRegions is toggled to be 'true' when the mobile discovers a new location. NewRegion
    ///     will have an updated "ConnectedRegions" to reflect this.
    /// </summary>
    [Serializable]
    public class MoveMobilePacket : Packet
    {
        private MobileDirections _direction;
        private BaseRegion _newRegion;
        private Regions _region;

        #region Constructors

        public MoveMobilePacket(Regions region, ulong playerId, MobileDirections direction = MobileDirections.None,
            Regions discoveredRegion = Regions.None)
            : base(PacketTypes.MobileMove, playerId)
        {
            Region = region;
            Direction = direction;
            DiscoveredRegion = discoveredRegion;
        }

        #endregion

        #region Getters / Setters

        public Regions DiscoveredRegion { get; set; }

        public MobileDirections Direction
        {
            get => _direction;
            private set
            {
                if (value == MobileDirections.None || value == Direction)
                    return; // Prevent assigning a bad value or reassigning.

                _direction = value;
            }
        }

        public Regions Region
        {
            get => _region;
            private set
            {
                if (Region != value) _region = value;
            }
        }

        public BaseRegion NewRegion
        {
            get => _newRegion;
            set
            {
                if (NewRegion != value) _newRegion = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class ResurrectMobilePacket : Packet
    {
        private Regions _region; // Region to be sent to.
        private bool _success;

        #region Constructors

        public ResurrectMobilePacket(ulong playerId)
            : base(PacketTypes.MobileResurrect, playerId)
        {
        }

        #endregion

        #region Getters / Setters

        public Regions Region
        {
            get => _region;
            set
            {
                if (Region != value) _region = value;
            }
        }

        public bool IsSuccessful
        {
            get => _success;
            set
            {
                if (value != IsSuccessful) _success = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class UseItemPacket : Packet
    {
        private string _response = string.Empty;

        #region Constructor

        public UseItemPacket(int serial, ulong playerId)
            : base(PacketTypes.UseItem, playerId)
        {
            Item = serial;
        }

        #endregion

        public int Item { get; }

        #region Getters / Setters

        public string Response
        {
            get => _response;
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                _response = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class UseVendorPacket : Packet
    {
        public enum Choices
        {
            Yes,
            No
        }

        #region Constructors

        public UseVendorPacket(ulong playerId)
            : base(PacketTypes.UseVendor, playerId)
        {
        }

        #endregion

        public static BaseItem PrintItems(Dictionary<BaseItem, int> items, bool zeroIsNone = false)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var optionMapping = new Dictionary<int, BaseItem>(); // <pos, item>

            if (zeroIsNone)
                Console.WriteLine($"[{0,-2}] none");

            var i = 1;
            foreach (var (item, cost) in items)
            {
                optionMapping.Add(i, item);
                Console.WriteLine($"[{i,-2}] {cost + "gp",-8} - {item.Name}");
                ++i;
            }

            // Get the choice from the user.
            var choice = Utility.ReadInt(items.Count + 1, zeroIsNone);

            // Return an empty item, "none" was selected.
            if (zeroIsNone && choice == 0)
                return new BaseItem();

            if (!optionMapping.ContainsKey(choice))
                throw new IndexOutOfRangeException("Item choice is not valid.");

            return optionMapping[choice];
        }

        #region Getters / Setters

        public Dictionary<int, NPCTypes> LocalVendors { get; set; } =
            new Dictionary<int, NPCTypes>(); // < Position, Type >

        public Dictionary<BaseItem, int> Items { get; set; } = new Dictionary<BaseItem, int>(); // < Cost, BaseItem >

        public NPCTypes LocalNPC { get; set; }

        public BaseItem Item { get; set; }

        public int Transaction { get; set; }

        public Choices PerformAction { get; set; }

        #endregion
    }
}