namespace Custom.Pathfinding
{
	public class PathNode
	{
		public int x;
		public int y;

		public int traverseCost;
		public int cumulativeCost; // G cost
		public int distanceCost; // H cost

		public PathNode prev = null;

		public int TotalCost => cumulativeCost + distanceCost;

		public PathNode(int x, int y, int traverseCost)
		{
			this.x = x;
			this.y = y;
			this.traverseCost = traverseCost;
		}
	}
}