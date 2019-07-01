using System;
using System.Collections.Generic;

namespace SUS.Shared.Packets
{
    [Serializable]
    public class GetInfoPacket : Packet
    {
        public enum RequestReason
        {
            Paperdoll,
            Location,
            Equipment,
            Items,
            Vendors,
            Npcs
        }

        private string _info;
        private List<string> _infoList;

        private RequestReason _reason;

        #region Constructors

        public GetInfoPacket(RequestReason reason)
        {
            Reason = reason;
        }

        #endregion

        public void AddInfo(string name)
        {
            InfoList.Add(name);
        }

        #region Getters / Setters

        public RequestReason Reason
        {
            get => _reason;
            private set
            {
                if (value == Reason)
                    return;

                _reason = value;
            }
        }

        public string Info
        {
            get => _info ?? (_info = string.Empty);
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                _info = value;
            }
        }

        public List<string> InfoList => _infoList ?? (_infoList = new List<string>());

        #endregion
    }

    [Serializable]
    public class GetNodePacket : Packet
    {
        #region Constructors

        public GetNodePacket(Regions region)
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