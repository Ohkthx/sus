using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SUS.Server.Map;
using SUS.Server.Map.Zones;
using SUS.Server.Objects;
using SUS.Shared;
using SUS.Shared.Actions;

namespace SUS.Server
{
    public static class World
    {
        #region Getters / Setters

        public static Regions StartingZone => GetStartingZone().Id;

        #endregion

        #region Dictionaries and Variables.

        // Discord ID => GameState
        private static readonly ConcurrentDictionary<ulong, Gamestate> Gamestates =
            new ConcurrentDictionary<ulong, Gamestate>();

        // Location.Type => Node
        private static readonly ConcurrentDictionary<Regions, Region> Nodes =
            new ConcurrentDictionary<Regions, Region>();

        // Contains all currently active/alive mobiles.
        private static readonly ConcurrentDictionary<Serial, Mobile> Mobiles =
            new ConcurrentDictionary<Serial, Mobile>();

        // Contains all items.
        private static readonly ConcurrentDictionary<Serial, Item> Items = new ConcurrentDictionary<Serial, Item>();

        #endregion

        #region Map Data

        /// <summary>
        ///     Creates the map, links the nodes, adds it to GameObject.
        /// </summary>
        private static void CreateMap()
        {
            // Make sure we do not accidental recreate nodes...
            if (Nodes.Any())
                return;

            // Add the nodes to be held by the GameObject.
            AddRegion(new Britain());
            AddRegion(new Sewers());
            AddRegion(new Wilderness());
            AddRegion(new Graveyard());
            AddRegion(new Despise());
        }

        /// <summary>
        ///     Gets the current starting zone for new players.
        /// </summary>
        /// <returns>A Node of the starting zone.</returns>
        private static Region GetStartingZone()
        {
            // If the nodes have not been added, create the map first.
            if (!Nodes.Any())
                CreateMap();

            // Returns the designated starting Node.
            return Nodes[Regions.Britain];
        }

        #endregion

        #region Gamestate Actions

        public static void AddGamestate(Gamestate gs)
        {
            if (gs == null)
                return;

            Gamestates[gs.PlayerId] = gs;
        }

        public static void RemoveGamestate(Gamestate gs)
        {
            if (gs == null)
                return;

            Gamestates.TryRemove(gs.PlayerId, out _);
        }

        /// <summary>
        ///     GameState to locate in GameObject.
        /// </summary>
        /// <returns>GameState provided by GameObject.</returns>
        public static Gamestate FindGamestate(ulong discordId)
        {
            return Gamestates.TryGetValue(discordId, out var gs) ? gs : null;
        }

        #endregion

        #region Items

        public static void AddItem(Item i)
        {
            if (i == null)
                return;

            Items[i.Serial] = i;
        }

        public static void RemoveItem(Item i)
        {
            if (i == null)
                return;

            Items.TryRemove(i.Serial, out _);
        }

        public static bool FindItem(Serial s, out Item item)
        {
            return Items.TryGetValue(s, out item);
        }

        #endregion

        #region Mobile Actions

        public static void AddMobile(Mobile m)
        {
            if (m == null)
                return;

            Mobiles[m.Serial] = m;
        }

        public static void RemoveMobile(Mobile m)
        {
            if (m == null)
                return;

            Mobiles.TryRemove(m.Serial, out _);
        }

        public static bool FindMobile(Serial s, out Mobile foundMobile)
        {
            return Mobiles.TryGetValue(s, out foundMobile);
        }

        public static HashSet<BaseMobile> FindNearbyMobiles(Mobile mobile, Regions region, int range)
        {
            if (mobile == null)
                throw new ArgumentNullException(nameof(mobile));

            if (region == Regions.None)
                throw new ArgumentOutOfRangeException(nameof(region),
                    "Attempted to find nearby mobiles in the non-existent region.");

            var localMobiles = new HashSet<BaseMobile>();
            foreach (var m in Mobiles.Values)
            {
                if (m.Region != region || m.IsPlayer && m.Serial == mobile.Serial)
                    continue;

                // Calculate the distance between the two coordinates.
                var distance = Point2D.Distance(mobile, m);
                if (distance <= range)
                    localMobiles.Add(m.Base());
            }

            return localMobiles;
        }

        private static bool FindNearestMobile(Mobile mobile, out Mobile closestMobile)
        {
            var v = mobile.Vision;
            var mobiles = new HashSet<BaseMobile>();
            var i = 0;
            while (mobiles.Count == 0 && ++i * v < 120)
                mobiles = FindNearbyMobiles(mobile, mobile.Region, v * i) ?? new HashSet<BaseMobile>();

            closestMobile = null;
            if (mobiles.Count == 0)
                return false; // Just return null since we found no nearby.

            foreach (var baseMobile in mobiles)
            {
                if (!FindMobile(baseMobile.Serial, out var m))
                    continue;

                if (closestMobile == null)
                    closestMobile = m;
                else if (Point2D.Distance(mobile, m) < Point2D.Distance(mobile, closestMobile))
                    closestMobile = m;
            }

            return closestMobile != null;
        }

        /// <summary>
        ///     Moves a mobile to the desired location or in a direction.
        /// </summary>
        /// <param name="moveAction">Action to perform, holds basic information.</param>
        /// <param name="newRegion">The new region the mobile is in, if any.</param>
        /// <param name="forceMove">Server or GM forced action.</param>
        /// <returns>Success or failure.</returns>
        public static bool MoveMobile(Move moveAction, out Region newRegion, bool forceMove = false)
        {
            if (!FindMobile(moveAction.MobileId, out var mobile))
                throw new ArgumentException("Mobile is non-existent when moving.", nameof(moveAction.MobileId));

            if (!FindRegion(moveAction.Destination, out newRegion))
                throw new ArgumentException("Region provided is non-existent when moving.",
                    nameof(moveAction.Destination));

            // Tells us if it is a move locally or to a new region.
            var localMove = mobile.Region == moveAction.Destination;
            if (localMove && moveAction.Direction == Directions.None)
                return false; // Player is not moving, return false;

            // Checks if it is a valid non-forced move. Regions have to be connected.
            if (!forceMove && !localMove && !IsConnectedRegion(mobile.Region, moveAction.Destination))
                return false; //   Return failure.

            // This validates that the mobile is not trying to access what they cannot.
            if (!forceMove && !mobile.AccessibleRegions.HasFlag(moveAction.Destination))
                return false;

            mobile.Region = moveAction.Destination;


            if (!localMove)
            {
                if (newRegion.IsSpawnable && newRegion is Spawnable spawn)
                {
                    mobile.Location = spawn.StartingLocation(); // Resets or assigns the new coordinate.
                    return true;
                }

                mobile.Location.Invalidate();
                return true;
            }

            // Move our mobile into the targeted direction.
            if (moveAction.Direction == Directions.Nearby)
            {
                // Move to nearby NPC.
                if (!FindNearestMobile(mobile, out var closestMobile))
                    return false;

                var vision = mobile.Vision > closestMobile.Vision ? mobile.Vision : closestMobile.Vision;

                while (Point2D.Distance(mobile, closestMobile) > vision)
                    mobile.Location = Point2D.MoveTowards(mobile, closestMobile, mobile.Speed);

                return true;
            }

            if (!(newRegion is Spawnable spawnable))
                throw new Exception("Cannot move in a direction in this region.");


            // Move in a specific direction.
            mobile.MoveInDirection(moveAction.Direction, spawnable.MaxX, spawnable.MaxY);

            // Checked if the mobile is in an unlockable area. If so, add it to unlocked areas.
            mobile.AddRegionAccess(spawnable.InUnlockedArea(mobile.Location));

            return true;
        }

        /// <summary>
        ///     Brings a mobile back to life.
        /// </summary>
        /// <param name="mobile">Mobile to be resurrected.</param>
        /// <param name="region">Region to relocate the mobile to.</param>
        /// <returns>Success or failure.</returns>
        public static bool Resurrect(Mobile mobile, Regions region)
        {
            // Validate we're not working with a null value.
            if (mobile == null)
                throw new ArgumentNullException(nameof(mobile));

            if (region == Regions.None)
                throw new ArgumentOutOfRangeException(nameof(region), "Attempted to move to a non-existent region.");

            var success = MoveMobile(new Move(mobile.Serial, region), out _, true);
            if (success)
                mobile.Resurrect(); // Perform the resurrection.

            return success;
        }

        /// <summary>
        ///     Kills off a mobile from the GameObject.
        /// </summary>
        /// <param name="mobile">Mobile to kill.</param>
        public static void Kill(Mobile mobile)
        {
            if (mobile == null)
                throw new ArgumentNullException(nameof(mobile));

            mobile.Kill(); // Kill the mobile.

            if (!mobile.IsPlayer)
                mobile.Delete(); // Remove the mobile from the list of mobiles.
        }

        #endregion

        #region Nodes / Locations Actions

        private static void AddRegion(Region n)
        {
            if (!Region.IsValidRegion(n.Id))
                return;

            Nodes[n.Id] = n;
        }

        public static bool FindRegion(Regions region, out Region foundRegion)
        {
            return Nodes.TryGetValue(region, out foundRegion);
        }

        /// <summary>
        ///     Validates that an originating connection (from) as a desired connection (to).
        /// </summary>
        /// <param name="from">Originating connection.</param>
        /// <param name="to">Connection to validate if exists.</param>
        /// <returns>True - Connection exists. False - Connection is faulty.</returns>
        private static bool IsConnectedRegion(Regions from, Regions to)
        {
            if (!Region.IsValidRegion(from) || !Region.IsValidRegion(to))
                return false; // One of them are not valid locations.

            // Get our originating location
            return FindRegion(from, out var region) && region.HasConnection(to);
        }

        #endregion
    }
}