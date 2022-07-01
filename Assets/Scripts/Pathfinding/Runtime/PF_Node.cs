using System;

namespace Custom.Pathfinding
{
    [Serializable]
    public class PF_Node
    {
        public readonly int x;
        public readonly int y;

        /// <summary> Cumulative score </summary>
        public int gScore = int.MaxValue;
        /// <summary> Distance score </summary>
        public int hScore;
        /// <summary> Total score </summary>
        public int fScore;

        public PF_Node link = null;

        public PF_Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
