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
		public int totalCost; // F cost

		public PathNode prev = null;

		public PathNode(int x, int y, int traverseCost)
		{
			this.x = x;
			this.y = y;
			this.traverseCost = traverseCost;

			this.cumulativeCost = int.MaxValue;
		}
	}
}