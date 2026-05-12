using System.Collections.Generic;
using UnityEngine;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Generates a branching overworld map for a run, inspired by the Dicey Dungeons
    /// overland map: left-to-right layers with nodes of type Fight, Treasure, and Shop.
    /// </summary>
    public static class WorldMapGenerator
    {
        // Horizontal margin as a fraction of the map container width.
        private const float MarginX = 0.06f;
        // Vertical range nodes are distributed across.
        private const float MinY = 0.15f;
        private const float MaxY = 0.85f;

        /// <summary>
        /// Generates a run map.
        /// </summary>
        /// <param name="combatLayers">
        /// Corresponds to <c>progressionCombatCount</c> – controls how many layers
        /// (and therefore how long) the run is.
        /// </param>
        /// <param name="minCombatEncounters">
        /// Minimum number of Fight nodes guaranteed on every possible path through the map.
        /// Middle layers are locked to all-Fight as needed to satisfy this constraint.
        /// </param>
        public static WorldMap Generate(int combatLayers, int minCombatEncounters = 3)
        {
            int totalLayers = Mathf.Max(2, combatLayers + 1);
            var map = new WorldMap { TotalLayers = totalLayers };

            // First and last layers are always fights. Calculate how many additional
            // fight-guaranteed layers are required in the middle so that every path
            // visits at least minCombatEncounters Fight nodes.
            int guaranteedFights  = 2; // layer 0 and last layer
            int extraFightsNeeded = Mathf.Max(0, minCombatEncounters - guaranteedFights);
            int middleLayers      = totalLayers - 2; // layers 1 .. totalLayers-2
            int forcedCount       = Mathf.Min(extraFightsNeeded, middleLayers);

            // Distribute forced-fight layers evenly across the middle section.
            // Using floor(i * middleLayers / forcedCount) guarantees unique, in-range indices
            // for any forcedCount <= middleLayers.
            var forcedFightLayers = new HashSet<int>();
            for (int i = 0; i < forcedCount; i++)
            {
                int layerIdx = 1 + (int)(i * middleLayers / (float)forcedCount);
                forcedFightLayers.Add(layerIdx);
            }

            int nodeId = 0;
            var layerNodes = new List<List<MapNode>>();

            // ── Build nodes ────────────────────────────────────────────────────
            for (int layer = 0; layer < totalLayers; layer++)
            {
                bool isFirst = layer == 0;
                bool isLast  = layer == totalLayers - 1;

                int nodeCount = (isFirst || isLast) ? 1 : BoBRandom.Range(3, 5); // exclusive upper bound → 3 or 4 middle nodes

                float xFrac = totalLayers == 1
                    ? 0.5f
                    : Mathf.Lerp(MarginX, 1f - MarginX, (float)layer / (totalLayers - 1));

                var nodes = new List<MapNode>();
                for (int slot = 0; slot < nodeCount; slot++)
                {
                    float yFrac = nodeCount == 1
                        ? 0.5f
                        : Mathf.Lerp(MinY, MaxY, (float)slot / (nodeCount - 1));

                    var node = new MapNode
                    {
                        Id           = nodeId++,
                        Type         = DetermineNodeType(layer, isFirst, isLast, forcedFightLayers.Contains(layer)),
                        Layer        = layer,
                        SlotInLayer  = slot,
                        IsBoss       = isLast,
                        NormX        = xFrac,
                        NormY        = yFrac,
                    };
                    nodes.Add(node);
                    map.Nodes.Add(node);
                }
                layerNodes.Add(nodes);
            }

            // ── Connect nodes between adjacent layers ──────────────────────────
            for (int layer = 0; layer < totalLayers - 1; layer++)
            {
                var thisLayer = layerNodes[layer];
                var nextLayer = layerNodes[layer + 1];

                var coveredNext = new HashSet<int>();

                foreach (var fromNode in thisLayer)
                {
                    // Always connect to the nearest node in the next layer.
                    int nearestSlot = Mathf.Clamp(
                        Mathf.RoundToInt(fromNode.SlotInLayer
                            * (nextLayer.Count - 1f)
                            / Mathf.Max(1f, thisLayer.Count - 1f)),
                        0, nextLayer.Count - 1);

                    AddConnection(fromNode, nextLayer[nearestSlot], coveredNext);

                    // Optionally connect to an adjacent slot for branching.
                    bool addExtra = nextLayer.Count > 1 && BoBRandom.Range(0, 2) == 0;
                    if (addExtra)
                    {
                        int extraSlot = nearestSlot < nextLayer.Count - 1
                            ? nearestSlot + 1
                            : nearestSlot - 1;
                        AddConnection(fromNode, nextLayer[extraSlot], coveredNext);
                    }
                }

                // Guarantee every next-layer node is reachable.
                foreach (var nextNode in nextLayer)
                {
                    if (coveredNext.Contains(nextNode.Id))
                        continue;

                    int nearestSlot = Mathf.Clamp(
                        Mathf.RoundToInt(nextNode.SlotInLayer
                            * (thisLayer.Count - 1f)
                            / Mathf.Max(1f, nextLayer.Count - 1f)),
                        0, thisLayer.Count - 1);

                    AddConnection(thisLayer[nearestSlot], nextNode, coveredNext);
                }
            }

            map.StartNodeId  = layerNodes[0][0].Id;
            map.CurrentNodeId = -1;
            return map;
        }

        private static void AddConnection(MapNode from, MapNode to, HashSet<int> coveredNext)
        {
            if (!from.NextNodeIds.Contains(to.Id))
                from.NextNodeIds.Add(to.Id);
            coveredNext.Add(to.Id);
        }

        private static EMapNodeType DetermineNodeType(int layer, bool isFirst, bool isLast, bool forceFight = false)
        {
            if (isFirst || isLast || forceFight)
                return EMapNodeType.Fight;

            // Middle nodes: ~40% fight, ~35% treasure, ~25% shop
            int roll = BoBRandom.Range(0, 20);
            if (roll < 8)  return EMapNodeType.Fight;
            if (roll < 15) return EMapNodeType.Treasure;
            return EMapNodeType.Shop;
        }
    }
}
