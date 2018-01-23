using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour
{
	GridManager gridManager;

	private void Awake()
	{
		gridManager = GetComponent<GridManager>();
	}
	
	// Path Coroutine
	public void FindPath(PathRequest request, Action<PathResult> callback)
	{
		Stopwatch sw = new Stopwatch(); // Timer
		sw.Start();                     // Start Timer

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		Node startNode = gridManager.grid.GetNodeFromPosition(request.pathStart);
		Node targetNode = gridManager.grid.GetNodeFromPosition(request.pathEnd);

		if (startNode.walkable && targetNode.walkable)
		{
			Heap<Node> openSet = new Heap<Node>(gridManager.grid.Count);
			HashSet<Node> closedSet = new HashSet<Node>();
			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);

				if (currentNode == targetNode)
				{
					sw.Stop();                                              // Stop Timer
					print("Path found: " + sw.ElapsedMilliseconds + " ms"); // Show Timer time
					pathSuccess = true;
					break;
				}

				foreach (Node neighbour in gridManager.grid.GetNodeNeightbours(currentNode))
				{
					if (!neighbour.walkable || closedSet.Contains(neighbour))
						continue;
					int newMovementCostToNeighbour = currentNode.cost.g + GetDistance(currentNode, neighbour)/* + neighbour.movementPenalty*/;
					if (newMovementCostToNeighbour < neighbour.cost.g || !openSet.Contains(neighbour))
					{
						neighbour.cost.g = newMovementCostToNeighbour;
						neighbour.cost.h = GetDistance(neighbour, targetNode);
						neighbour.parent = currentNode;

						if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
						else
							openSet.UpdateItem(neighbour);
					}
				}
			}
		}
		if (pathSuccess)
		{
			waypoints = RetracePath(startNode, targetNode);
			pathSuccess = waypoints.Length > 0;	// For when moving target a small range
		}
		callback(new PathResult(waypoints, pathSuccess, request.callback));
	}

	Vector3[] RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		Vector3[] waypoints = SimplifyPath(path);
		Array.Reverse(waypoints);
		return waypoints; 
	}

	Vector3[] SimplifyPath(List<Node> path)
	{
		List<Vector3> waypoints = new List<Vector3>();

		Vector2 directionOld = Vector2.zero;
		for (int i = 1; i < path.Count; ++i)
		{
			Vector2 directionNew = new Vector2(path[i - 1].x - path[i].x, path[i - 1].y - path[i].y);
			if (directionNew != directionOld)
			{
				waypoints.Add(path[i].position);
			}
			directionOld = directionNew;
		}

		return waypoints.ToArray();
	}

	int GetDistance(Node nodeA, Node nodeB)
	{
		int dstX = Mathf.Abs(nodeA.x - nodeB.x);
		int dstY = Mathf.Abs(nodeA.y - nodeB.y);
		// 14 for every diagonal move, 10 for every horizontal / vertical move
		if (dstX > dstY)
			return 14 * dstY + 10 * (dstX - dstY);
		return 14 * dstX + 10 * (dstY - dstX);
	}
}
