using System;

namespace Custom.Pathfinding
{
    [Serializable]
    public class PF_Node : IComparable<PF_Node>
    {
        public readonly int x;
        public readonly int y;

        public float cost = float.MaxValue; // g cost
        public float distanceCost = float.MaxValue; // h cost
        public float totalCost; // f cost

        public PF_ENodeState state = PF_ENodeState.Idle;
        public PF_Node link;

        public PF_Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int CompareTo(PF_Node other)
        {
            var result = totalCost.CompareTo(other.totalCost);
            if (result == 0)
                result = cost.CompareTo(other.cost);
            return result;
        }
    }
}
