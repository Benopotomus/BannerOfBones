using System.Collections.Generic;

namespace BannerOfBones.CardGame
{
    /// <summary>
    /// A single node on the overworld run map.
    /// </summary>
    public class MapNode
    {
        public int Id;
        public EMapNodeType Type;
        public int Layer;
        public int SlotInLayer;
        public bool IsBoss;
        public bool IsVisited;
        public List<int> NextNodeIds = new List<int>();

        /// <summary>Normalised X position [0,1] within the map container.</summary>
        public float NormX;
        /// <summary>Normalised Y position [0,1] within the map container.</summary>
        public float NormY;
    }
}
