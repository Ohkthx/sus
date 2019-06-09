using System;
using System.Collections.Generic;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class GetMobilesPacket : Packet
    {
        private HashSet<BaseMobile> _mobiles;
        private Regions _region;

        #region Constructors

        public GetMobilesPacket(Regions region, ulong playerId)
            : base(PacketTypes.GetLocalMobiles, playerId)
        {
            Region = region;
        }

        #endregion

        #region Getters / Setters

        public Regions Region
        {
            get => _region;
            private set => _region = value;
        }

        public HashSet<BaseMobile> Mobiles
        {
            get => _mobiles ?? (_mobiles = new HashSet<BaseMobile>());
            set
            {
                if (value == null) return;

                _mobiles = value;
            }
        }

        #endregion
    }

    [Serializable]
    public class GetMobilePacket : Packet
    {
        [Flags]
        public enum RequestReason
        {
            None = 0,
            Paperdoll = 1,
            Location = 2,
            IsDead = 4,
            Items = 8,
            Equipment = 16
        }

        private Dictionary<int, string> _equipment;
        private bool _isAlive = true;
        private Dictionary<int, string> _items;

        // Requested information to return.
        private string _paperdoll;

        private RequestReason _reason;
        private Regions _region;

        #region Constructors

        public GetMobilePacket(RequestReason reason, ulong playerId)
            : base(PacketTypes.GetMobile, playerId)
        {
            Reason = reason;
        }

        #endregion

        public void AddItem(int serial, string name)
        {
            Items.Add(serial, name);
        }

        public void AddEquipment(int serial, string name)
        {
            Equipment.Add(serial, name);
        }

        #region Getters / Setters

        public RequestReason Reason
        {
            get => _reason;
            private set
            {
                if (value == RequestReason.None || value == Reason) return;

                _reason = value;
            }
        }

        public string Paperdoll
        {
            get => _paperdoll ?? (_paperdoll = string.Empty);
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                _paperdoll = value;
            }
        }

        public Regions Region
        {
            get => _region;
            set
            {
                if (value == Regions.None || value == Region) return;

                _region = value;
            }
        }

        public bool IsAlive
        {
            get => _isAlive;
            set
            {
                if (value != IsAlive) _isAlive = value;
            }
        }

        public Dictionary<int, string> Items => _items ?? (_items = new Dictionary<int, string>());

        public Dictionary<int, string> Equipment => _equipment ?? (_equipment = new Dictionary<int, string>());

        #endregion
    }

    [Serializable]
    public class GetNodePacket : Packet
    {
        #region Constructors

        public GetNodePacket(Regions region, ulong playerId)
            : base(PacketTypes.GetNode, playerId)
        {
            Region = region;
        }

        #endregion

        #region Getters / Setters

        public Regions Region { get; }

        public BaseRegion NewRegion { get; set; }

        #endregion
    }
}