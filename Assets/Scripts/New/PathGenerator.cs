using System.Collections.Generic;
using UnityEngine;
using Common;

namespace Custom.Pathfinding
{
	public static class PathGenerator
	{
		private const int TRAVERSE_COST = 10;
		private const int DISTANCE_COST = 10;
		private const int DIAGONAL_COST = 14;

		public static List<PathNode> Generate(bool[,] map, Vector2Int current, Vector2Int target)
		{
			var result = new List<PathNode>();

			var width = map.GetLength(0);
			var height = map.GetLength(1);

			var gridRange = new Range2Int(0, 0, width - 1, height - 1);
			var grid = new PathNode[width * height];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (map[x, y])
					{
						var node = new PathNode(x, y, TRAVERSE_COST);

						grid[x + y * width] = node;
					}
				}
			}

			var currentIndex = current.x + current.y * width;
			var targetIndex = target.x + target.y * width;
			var targetNode = grid[targetIndex];

			var remainingList = new List<int>();
			var checkedArray = new bool[width * height];
			var directions = GetDirections();

			remainingList.Add(currentIndex);
			while (remainingList.Count > 0)
			{
				int currentNodeIndex = GetLowestTotalCostNodeIndex(remainingList, grid);

				if (currentNodeIndex == targetIndex)
					break; // Reached destination
				
				if (remainingList.TryGetIndexOf(currentNodeIndex, out int index))
				{
					remainingList.SwapLast(index);
					remainingList.RemoveLast();
				}

				checkedArray[currentNodeIndex] = true;

				var currentNode = grid[currentNodeIndex];

				for (int i = 0; i < directions.Length; i++)
				{
					var direction = directions[i];

					var neighbourX = currentNode.x + direction.x;
					var neighbourY = currentNode.y + direction.y;

					if (!gridRange.Contains(neighbourX, neighbourY))
						continue;

					var neighbourNodeIndex = neighbourX + neighbourY * width;
					if (checkedArray[neighbourNodeIndex])
						continue;

					var neighbourNode = grid[neighbourNodeIndex];
					if (neighbourNode == null)
						continue;

					var totalCumulativeCost = currentNode.cumulativeCost + GetDistanceCost(currentNode, neighbourNode) + neighbourNode.traverseCost;
					if (totalCumulativeCost < neighbourNode.cumulativeCost)
					{
						neighbourNode.prev = currentNode;
						neighbourNode.cumulativeCost = totalCumulativeCost;
						neighbourNode.distanceCost = GetDistanceCost(neighbourNode, targetNode);

						if (!remainingList.Contains(neighbourNodeIndex))
							remainingList.Add(neighbourNodeIndex);
					}
				}
			}

			if (targetNode.prev != null)
			{
				var node = targetNode;
				while (node.prev != null)
				{
					result.Add(node);
					node = node.prev;
				}

				result.Add(node);
			}

			return result;
		}
		
		private static int GetLowestTotalCostNodeIndex(List<int> remaining, PathNode[] nodes)
		{
			var result = 0;

			var nodeA = nodes[result];
			for (int i = 1; i < remaining.Count; i++)
			{
				var nodeB = nodes[remaining[i]];
				if (nodeA.TotalCost > nodeB.TotalCost)
				{
					nodeA = nodeB;
					result = i;
				}
			}

			return result;
		}

		private static int GetDistanceCost(PathNode a, PathNode b)
		{	// Can be abstracted
			int dx = Mathf.Abs(b.x - a.x);
			int dy = Mathf.Abs(b.y - a.y);
			if (dx > dy)
				return DIAGONAL_COST * dy + DISTANCE_COST * (dx - dy);
			return DIAGONAL_COST * dx + DISTANCE_COST * (dy - dx);
		}

		private static readonly Vector2Int[] Directions = new Vector2Int[]
		{
			new Vector2Int(-1, +0),
			new Vector2Int(+0, +1),
			new Vector2Int(+1, +0),
			new Vector2Int(+0, -1),
			new Vector2Int(-1, -1),
			new Vector2Int(-1, +1),
			new Vector2Int(+1, +1),
			new Vector2Int(+1, -1)
		};

		private static Vector2Int[] GetDirections()
		{   // Can be abstracted
			return Directions;
		}
	}
}