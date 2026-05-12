using System.Collections.Generic;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// Holds all nodes and current navigation state for a single run's overworld map.
    /// </summary>
    public class WorldMap
    {
        public List<MapNode> Nodes = new List<MapNode>();
        public int StartNodeId;
        /// <summary>-1 means the player has not yet entered any node.</summary>
        public int CurrentNodeId = -1;
        public int TotalLayers;

        public MapNode GetNode(int id)
        {
            foreach (var node in Nodes)
                if (node.Id == id) return node;
            return null;
        }

        /// <summary>Returns the node IDs the player can travel to from the current position.</summary>
        public List<int> GetReachableNodeIds()
        {
            if (CurrentNodeId < 0)
                return new List<int> { StartNodeId };

            var current = GetNode(CurrentNodeId);
            return current?.NextNodeIds ?? new List<int>();
        }
    }
}
