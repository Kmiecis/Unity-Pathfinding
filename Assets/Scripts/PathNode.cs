using System;

namespace Custom.Pathfinding
{
    [Serializable]
    public class PathNode
    {
        public readonly int x;
        public readonly int y;

        /// <summary> Cumulative score </summary>
        public int gScore = int.MaxValue;
        /// <summary> Distance score </summary>
        public int hScore;
        /// <summary> Total score </summary>
        public int fScore;

        public PathNode link = null;

        public PathNode(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
