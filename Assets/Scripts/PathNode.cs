using System;

namespace Custom.Pathfinding
{
	[Serializable]
	public class PathNode
	{
		public int x;
		public int y;

		public int traverseCost;
		public int cumulativeCost; // G cost
		public int distanceCost; // H cost

		public PathNode prev = null;

		public int TotalCost => cumulativeCost + distanceCost; // F cost

		public PathNode(int x, int y, int traverseCost)
		{
			this.x = x;
			this.y = y;
			this.traverseCost = traverseCost;

			this.cumulativeCost = int.MaxValue;
		}
	}
}