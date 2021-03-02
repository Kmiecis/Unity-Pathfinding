using Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
	public static class PathGenerator
	{
		private const int TRAVERSE_COST = 10;
		private const int DISTANCE_COST = 10;
		private const int DIAGONAL_COST = 14;
		
		public static List<Vector2Int> Generate(bool[,] map, Vector2Int start, Vector2Int target)
		{
			var result = new List<Vector2Int>();

			var width = map.GetLength(0);
			var height = map.GetLength(1);

			var gridRange = new Range2Int(0, 0, width - 1, height - 1);
			var grid = new PathNode[width * height];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (!map[x, y])
					{
						grid[x + y * width] = new PathNode(x, y, TRAVERSE_COST);
					}
				}
			}

			var startIndex = start.x + start.y * width;
			var targetIndex = target.x + target.y * width;

			var startNode = grid[startIndex];
			if (startNode == null)
				return result;

			var targetNode = grid[targetIndex];
			if (targetNode == null)
				return result;

			startNode.cumulativeCost = 0;

			var remainingList = new List<int>();
			var checkedArray = new bool[width * height];
			var directions = GetDirections();

			remainingList.Add(startIndex);
			while (remainingList.Count > 0)
			{
				int currentNodeIndex = GetLowestTotalCostNodeIndex(remainingList, grid);

				if (currentNodeIndex == targetIndex)
					break;
				
				if (remainingList.TryIndexOf(currentNodeIndex, out int index))
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
						neighbourNode.totalCost = neighbourNode.cumulativeCost + neighbourNode.distanceCost;

						if (!remainingList.Contains(neighbourNodeIndex))
							remainingList.Add(neighbourNodeIndex);
					}
				}
			}

			var node = targetNode;
			while (node.prev != null)
			{
				if (result.Count > 1)
				{
					var last = result[result.Count - 1];
					if (
						Math.Sign(node.x - last.x) != Math.Sign(node.prev.x - node.x) ||
						Math.Sign(node.y - last.y) != Math.Sign(node.prev.y - node.y)
					)
					{
						result.Add(new Vector2Int(node.x, node.y));
					}
				}
				else
				{
					result.Add(new Vector2Int(node.x, node.y));
				}

				node = node.prev;
			}
			if (result.Count > 0)
			{
				result.Add(new Vector2Int(node.x, node.y));
			}

			return result;
		}
		
		private static int GetLowestTotalCostNodeIndex(List<int> remaining, PathNode[] nodes)
		{
			var result = remaining[0];
			var nodeA = nodes[result];

			for (int i = 1; i < remaining.Count; i++)
			{
				var current = remaining[i];
				var nodeB = nodes[current];

				if (nodeA.totalCost > nodeB.totalCost)
				{
					result = current;
					nodeA = nodeB;
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