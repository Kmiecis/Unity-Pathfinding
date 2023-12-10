using System;

namespace Custom.Pathfinding
{
    [Serializable]
    public class PF_Node : IComparable<PF_Node>
    {
        public readonly int x;
        public readonly int y;

        public int cost = int.MaxValue;
        public int totalcost;

        public PF_ENodeState state = PF_ENodeState.Idle;
        public PF_Node link = null;

        public PF_Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int CompareTo(PF_Node other)
        {
            if (totalcost == other.totalcost)
            {
                return cost - other.cost;
            }
            return totalcost - other.totalcost;
        }
    }
}
