using System;
using System.Collections.Generic;
using SUS.Shared.Actions;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class CombatPacket : Packet
    {
        private int _mobileCount;
        private Dictionary<int, BaseMobile> _mobiles; // Local mobiles that exist in the area.
        private Regions _region; // Local region.
        private string _result; // Result of the combat.
        private List<int> _targets; // List of Targets
        private List<string> _updates; // Updates on all.

        public bool IsAlive { get; set; } // Determines if the Initiator (Player) died.

        public void AddTarget(BaseMobile tag)
        {
            if (!Targets.Contains(tag.Serial))
                Targets.Add(tag.Serial);
        }

        public void AddUpdate(List<string> info)
        {
            if (info == null)
                return;

            Updates.AddRange(info);
        }

        public void AddMobile(BaseMobile mobile, int position = -1)
        {
            var pos = position < 0 ? ++_mobileCount : position;
            Mobiles.Add(pos, mobile);
        }


        #region Getters / Setters

        public List<int> Targets => _targets ?? (_targets = new List<int>());

        public List<string> Updates => _updates ?? (_updates = new List<string>());

        public Regions Region
        {
            get => _region;
            private set => _region = value;
        }

        public Dictionary<int, BaseMobile> Mobiles
        {
            get => _mobiles ?? (_mobiles = new Dictionary<int, BaseMobile>());
            set
            {
                if (value == null)
                    return;

                _mobiles = value;
            }
        }

        public string Result
        {
            get => _result;
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

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
    public class MovePacket : Packet
    {
        private Directions _direction;
        private BaseRegion _newRegion;
        private Regions _region;

        #region Constructors

        public MovePacket(int mobileId, Regions region, Directions direction = Directions.None)
        {
            Action = new Move(mobileId, region, direction);
            Region = region;
            Direction = direction;
        }

        #endregion

        public Move Action { get; }

        public void AddDiscovery(Regions discovery)
        {
            DiscoveredRegions |= discovery;
        }

        #region Getters / Setters

        public Regions DiscoveredRegions { get; private set; }

        public Directions Direction
        {
            get => _direction;
            private set
            {
                if (value == Directions.None || value == Direction)
                    return; // Prevent assigning a bad value or reassigning.

                _direction = value;
            }
        }

        public Regions Region
        {
            get => _region;
            private set
            {
                if (Region != value)
                    _region = value;
            }
        }

        public BaseRegion NewRegion
        {
            get => _newRegion;
            set
            {
                if (NewRegion != value)
                    _newRegion = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class ResurrectPacket : Packet
    {
        private Regions _region; // Region to be sent to.
        private bool _success;

        #region Getters / Setters

        public Regions Region
        {
            get => _region;
            set
            {
                if (Region != value)
                    _region = value;
            }
        }

        public bool IsSuccessful
        {
            get => _success;
            set
            {
                if (value != IsSuccessful)
                    _success = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class UseItemPacket : Packet
    {
        private Dictionary<int, string> _items;
        private string _response = string.Empty;

        public void AddItem(int serial, string name)
        {
            Items.Add(serial, name);
        }

        #region Getters / Setters

        public int Item { get; set; }

        public Dictionary<int, string> Items => _items ?? (_items = new Dictionary<int, string>());

        public string Response
        {
            get => _response;
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                _response = value;
            }
        }

        /// <summary>
        ///     Removes non-required data at certain stages for cleaner transmission.
        /// </summary>
        protected override void Clean()
        {
            // We no longer need items to be transmitted in Stage Three.
            if (Stage == Stages.Three)
                _items = null;
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

        /// <summary>
        ///     Removes unneeded information from the packet for cleaner transmission.
        /// </summary>
        protected override void Clean()
        {
            switch (Stage)
            {
                // No longer need to include vendors in transmission.
                case Stages.Three:
                    LocalVendors = null;
                    break;

                // No longer need to include items in transmission.
                case Stages.Four:
                    Items = null;
                    break;
            }
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