using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SUS.Server.Map;
using SUS.Server.Map.Regions;
using SUS.Server.Objects;
using SUS.Server.Objects.Mobiles;
using SUS.Shared;

namespace SUS.Server
{
    public static class World
    {
        #region Getters / Setters

        public static Regions StartingZone => GetStartingZone().Region;

        #endregion

        #region Dictionaries and Variables.

        // Discord ID => GameState
        private static readonly ConcurrentDictionary<ulong, Gamestate> Gamestates =
            new ConcurrentDictionary<ulong, Gamestate>();

        // Location.Type => Node
        private static readonly ConcurrentDictionary<Regions, Node> Nodes = new ConcurrentDictionary<Regions, Node>();

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
            AddNode(new Britain());
            AddNode(new Sewers());
            AddNode(new Wilderness());
            AddNode(new Graveyard());
            AddNode(new Despise());
        }

        /// <summary>
        ///     Gets the current starting zone for new players.
        /// </summary>
        /// <returns>A Node of the starting zone.</returns>
        private static Node GetStartingZone()
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

        public static Item FindItem(Serial s)
        {
            if (!Items.ContainsKey(s)) return null;
            return Items.TryGetValue(s, out var i) ? i : null;
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

        public static Mobile FindMobile(Serial s)
        {
            if (!Mobiles.ContainsKey(s)) return null;
            return Mobiles.TryGetValue(s, out var m) ? m : null;
        }

        public static HashSet<BaseMobile> FindNearbyMobiles(Regions region, Mobile mobile, int range)
        {
            if (mobile == null)
                return null;

            var lm = new HashSet<BaseMobile>();
            foreach (var m in Mobiles.Values)
            {
                if (m.Region != region || m.IsPlayer && m.Serial == mobile.Serial)
                    continue;

                // Calculate the distance between the two coordinates.
                var distance = Point2D.Distance(mobile, m);

                if (distance <= range)
                    lm.Add(m.Base());
            }

            return lm;
        }

        private static Mobile FindNearestMobile(Mobile mobile)
        {
            if (mobile == null)
                return null;

            var v = mobile.Vision;
            var mobiles = new HashSet<BaseMobile>();
            var i = 0;
            while (mobiles.Count == 0 && ++i * v < 120)
                mobiles = FindNearbyMobiles(mobile.Region, mobile, v * i) ?? new HashSet<BaseMobile>();

            if (mobiles.Count == 0)
                return null; // Just return null since we found no nearby.

            Mobile closestMobile = null;
            foreach (var m in mobiles)
            {
                var mm = FindMobile(m.Serial);
                if (mm == null)
                    continue;

                if (closestMobile == null)
                    closestMobile = mm;
                else if (Point2D.Distance(mobile, mm) < Point2D.Distance(mobile, closestMobile))
                    closestMobile = mm;
            }

            return closestMobile;
        }

        /// <summary>
        ///     Updates a Node with a mobile.
        /// </summary>
        /// <param name="toRegion">Node to move the mobile to.</param>
        /// <param name="mobile">Mobile to update.</param>
        /// <param name="direction">Direction moving in.</param>
        /// <param name="forceMove">Overrides requirements if an admin is performing the action.</param>
        public static Node MoveMobile(Regions toRegion, Mobile mobile,
            MobileDirections direction = MobileDirections.None, bool forceMove = false)
        {
            if (mobile == null)
                return null;

            if (!forceMove) // If it is not an admin move...
                if (!IsConnectedLocation(mobile.Region, toRegion)
                    && toRegion != mobile.Region) //  And it is not a connected location...
                    return null; //   Return failure.

            // This validates that the mobile is not trying to access what they cannot.
            if (!forceMove)
                if (mobile is Player player)
                    if ((player.UnlockedRegions & toRegion) != toRegion)
                        return null;

            var node = FindNode(toRegion);
            if (node is Spawnable spawnable)
            {
                if (toRegion != mobile.Region)
                    mobile.Location = spawnable.StartingLocation(); // Resets or assigns the new coordinate.

                if (toRegion == mobile.Region)
                {
                    // Move our mobile into the targeted direction.
                    if (direction == MobileDirections.Nearby)
                    {
                        // Move to nearby NPC.
                        var newM = FindNearestMobile(mobile);
                        if (newM != null)
                            for (var distance = Point2D.Distance(mobile, newM);
                                distance > mobile.Vision;
                                distance = Point2D.Distance(mobile, newM))
                                mobile.Location = Point2D.MoveTowards(mobile, newM, mobile.Speed);
                    }
                    else
                    {
                        // Move in a specific direction.
                        mobile.MoveInDirection(direction, spawnable.MaxX, spawnable.MaxY);

                        // Checked if the mobile is in an unlockable area. If so, add it to unlocked areas.
                        if (mobile is Player player)
                            player.AddUnlockedRegion(spawnable.InUnlockedArea(mobile.Location));
                    }
                }
            }
            else
            {
                mobile.Location.Invalidate();
            }

            mobile.Region = toRegion; // Update the local mobile to the new location.
            return node;
        }

        public static void Resurrect(Regions region, Mobile mobile)
        {
            // Validate we're not working with a null value.
            if (mobile == null)
                return;

            MoveMobile(region, mobile, forceMove: true);

            mobile.Resurrect(); // Perform the resurrection.
        }

        /// <summary>
        ///     Kills off a mobile from the GameObject.
        /// </summary>
        /// <param name="mobile">Mobile to kill.</param>
        public static void Kill(Mobile mobile)
        {
            if (mobile == null)
                return;

            mobile.Kill(); // Kill the mobile.

            if (!mobile.IsPlayer) mobile.Delete(); // Remove the mobile from the list of mobiles.
        }

        #endregion

        #region Nodes / Locations Actions

        private static void AddNode(Node n)
        {
            if (n == null || !Node.IsValidRegion(n.Region))
                return;

            Nodes[n.Region] = n;
        }

        /// <summary>
        ///     Locates a Node based on it's ID.
        /// </summary>
        /// <param name="region">Region to find.</param>
        /// <returns>Discovered Node from the GameObject.</returns>
        public static Node FindNode(Regions region)
        {
            return !Nodes.TryGetValue(region, out var n) ? null : n;
        }

        /// <summary>
        ///     Validates that an originating connection (from) as a desired connection (to).
        /// </summary>
        /// <param name="from">Originating connection.</param>
        /// <param name="to">Connection to validate if exists.</param>
        /// <returns>True - Connection exists. False - Connection is faulty.</returns>
        private static bool IsConnectedLocation(Regions from, Regions to)
        {
            if (!Node.IsValidRegion(from) || !Node.IsValidRegion(to))
                return false; // One of them are not valid locations.

            // Get our originating location
            var fromN = FindNode(from);
            return fromN != null && fromN.HasConnection(to);
        }

        #endregion
    }
}