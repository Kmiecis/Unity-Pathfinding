using System;

namespace Custom.Pathfinding
{
    [Serializable]
    public class PF_Node3D
    {
        public readonly int x;
        public readonly int y;
        public readonly int z;

        public int cost = int.MaxValue;
        public int totalcost;

        public PF_ENodeState state = PF_ENodeState.Idle;
        public PF_Node3D link = null;

        public PF_Node3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
