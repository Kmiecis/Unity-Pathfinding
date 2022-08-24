using System;

namespace Custom.Pathfinding
{
    [Serializable]
    public class PF_Node3D
    {
        public readonly int x;
        public readonly int y;
        public readonly int z;

        /// <summary> Cumulative score </summary>
        public int gScore = int.MaxValue;
        /// <summary> Distance score </summary>
        public int hScore;
        /// <summary> Total score </summary>
        public int fScore;

        public PF_Node3D link = null;

        public PF_Node3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
