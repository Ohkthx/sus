using System;
using System.Collections.Generic;
using SUS.Shared.Actions;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class CombatPacket : Packet
    {
        private Dictionary<int, BaseMobile> _mobiles; // Local mobiles that exist in the area.
        private string _result; // Result of the combat.
        private List<string> _updates; // Updates on all.

        public bool IsAlive { get; set; } // Determines if the Initiator (Player) died.

        /// <summary>
        ///     Add an update from the result of the combat.
        /// </summary>
        /// <param name="info">Information that is to be added.</param>
        public void AddUpdate(List<string> info)
        {
            if (info == null)
                return;

            Updates.AddRange(info);
        }

        /// <summary>
        ///     Add a mobile to the list of potential targets.
        /// </summary>
        /// <param name="mobile">Mobile to be added.</param>
        public void AddMobile(BaseMobile mobile)
        {
            Mobiles.Add(mobile.Serial, mobile);
        }

        #region Getters / Setters

        /// <summary>
        ///     Signifies that "last" a valid option.
        /// </summary>
        public bool AllowLast { get; set; }

        public bool LastSelected { get; set; }

        public int Target { get; private set; }

        public List<string> Updates => _updates ?? (_updates = new List<string>());

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

        /// <summary>
        ///     Sets the target to be attacked, if any.
        /// </summary>
        /// <returns>True if a target is chosen.</returns>
        public bool SetTarget()
        {
            if (Mobiles == null)
                throw new ArgumentNullException(nameof(Mobiles), "There are no mobiles to display.");

            var optionMapping = new Dictionary<int, BaseMobile>(); // <pos, mobile>
            var totalCount = Mobiles.Count;

            Console.WriteLine($"[{0,-2}] none");
            ++totalCount;

            var i = 1;
            foreach (var (serial, mobile) in Mobiles)
            {
                optionMapping.Add(i, mobile);
                Console.WriteLine($"[{i,-2}] {mobile.Name}");
                ++i;
            }

            if (AllowLast)
            {
                Console.WriteLine($"[{i,-2}] last");
                ++totalCount;
            }

            // Get the choice from the user.
            var choice = Utility.ReadInt(totalCount, true);

            // Return an empty item, "none" was selected.
            if (choice == 0)
                return false;

            if (choice == i)
            {
                LastSelected = true;
                return true;
            }

            if (!optionMapping.ContainsKey(choice))
                throw new IndexOutOfRangeException("Item choice is not valid.");

            Target = optionMapping[choice].Serial;
            return true;
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

        /// <summary>
        ///     Sets the item to be used, if any.
        /// </summary>
        /// <returns>True if an item is chosen.</returns>
        public bool SetItem()
        {
            if (Items == null)
                throw new ArgumentNullException(nameof(Items), "There are no items to display.");

            var optionMapping = new Dictionary<int, BaseItem>(); // <pos, item>
            var totalCount = Items.Count;

            Console.WriteLine($"[{0,-2}] none");
            ++totalCount;

            var i = 1;
            foreach (var (item, cost) in Items)
            {
                optionMapping.Add(i, item);
                Console.WriteLine($"[{i,-2}] {cost + "gp",-8} - {item.Name}");
                ++i;
            }

            if (AllowAll)
            {
                Console.WriteLine($"[{i,-2}] all");
                ++totalCount;
            }

            // Get the choice from the user.
            var choice = Utility.ReadInt(totalCount, true);

            // Return an empty item, "none" was selected.
            if (choice == 0)
                return false;

            if (choice == i)
            {
                AllSelected = true;
                return true;
            }

            if (!optionMapping.ContainsKey(choice))
                throw new IndexOutOfRangeException("Item choice is not valid.");

            Item = optionMapping[choice];
            return true;
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

        public Dictionary<int, NpcTypes> LocalVendors { get; set; } =
            new Dictionary<int, NpcTypes>(); // < Position, Type >

        public Dictionary<BaseItem, int> Items { get; set; } = new Dictionary<BaseItem, int>(); // <BaseItem, cost>

        public NpcTypes LocalNpc { get; set; }

        public BaseItem Item { get; set; }

        public int Transaction { get; set; }

        public Choices PerformAction { get; set; }

        public bool AllowAll { get; set; }

        public bool AllSelected { get; set; }

        #endregion
    }
}