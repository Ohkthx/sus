using System;
using System.Collections.Generic;

namespace SUS.Shared
{
    [Serializable]
    public class ClientState
    {
        private BaseMobile _account;
        private BaseRegion _currentRegion;
        private BaseRegion _lastRegion;

        // Objects that need to be requested from the server.
        private HashSet<BaseMobile> _localMobiles; // Local / Nearby creatures.

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

        public void Resurrect()
        {
            IsAlive = true;
        }

        public void Kill()
        {
            IsAlive = false;
        }

        #region Getters / Setters

        public ulong PlayerId { get; }

        public BaseMobile Account
        {
            get => _account;
            private set
            {
                if (!value.IsPlayer)
                    return;

                _account = value;
            }
        }

        /// <summary>
        ///     Gets all of the current nearby regions, excluding the one that we are currently in.
        /// </summary>
        public Regions NearbyAccessibleRegions => CurrentRegion.Connections & UnlockedRegions & ~CurrentRegion.Id;

        public BaseRegion CurrentRegion
        {
            get => _currentRegion;
            set
            {
                if (!BaseRegion.IsValidRegionId(value.Id) || value.Id == CurrentRegion.Id)
                    return;

                LastRegion = CurrentRegion; // Swap the Node.
                _currentRegion = value; // Assign the new
            }
        }

        public BaseRegion LastRegion
        {
            get => _lastRegion;
            private set
            {
                if (!BaseRegion.IsValidRegionId(value.Id) || value.Id == LastRegion.Id)
                    return;

                _lastRegion = value; // Updates our Last Node accessed.
            }
        }

        public HashSet<BaseMobile> LocalMobiles
        {
            get => _localMobiles ?? (_localMobiles = new HashSet<BaseMobile>());
            set
            {
                if (value == null)
                    return;

                _localMobiles = value;
            }
        }

        public bool IsAlive { get; private set; }

        public Regions UnlockedRegions { get; private set; }

        #endregion
    }
}