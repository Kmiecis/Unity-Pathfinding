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

		Node startNode = gridManager.grid.GetNodeFromPosition(request.pathStart);   // Set some starting node.
		Node targetNode = gridManager.grid.GetNodeFromPosition(request.pathEnd);    // Set some ending node.

        // Only begin calculation if those are walkable.
		if (startNode.walkable && targetNode.walkable)
		{
			Heap<Node> openSet = new Heap<Node>(gridManager.grid.Count);    // Contains every node to currently look at, sorted by lowest costs as highest priority.
			HashSet<Node> closedSet = new HashSet<Node>();                  // Contains every node which at the time had lowest cost.
			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				Node currentNode = openSet.RemoveFirst();   // Remove node with the lowest cost.
				closedSet.Add(currentNode);                 // Add node with the lowest cost.

                // End searching if we have reached target node.
                if (currentNode == targetNode)
				{
					sw.Stop();                                              // Stop Timer
					print("Path found: " + sw.ElapsedMilliseconds + " ms"); // Show Timer time
					pathSuccess = true;
					break;
				}

                // Search through every neighbour of currently looked at node.
				foreach (Node neighbour in gridManager.grid.GetNodeNeightbours(currentNode))
				{
                    // Skip non walkable nodes and those that we already contain in resulting set.
					if (!neighbour.walkable || closedSet.Contains(neighbour))
						continue;

                    // Calculate cost of moving to selected node from the start node.
					int newMovementCostToNeighbour = currentNode.cost.g + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;

                    // In case calculated cost is cheaper than currently looked at neighbour OR we haven't added neighbour to open set.
                    if (newMovementCostToNeighbour < neighbour.cost.g || !openSet.Contains(neighbour))
					{
						neighbour.cost.g = newMovementCostToNeighbour;          // Cost from the start node to current.
                        neighbour.cost.h = GetDistance(neighbour, targetNode);  // Cost from the current to end node, estimated.
                        neighbour.parent = currentNode;                     // Set current node parent from which it originates.

                        // In case open set doesn't already contain, add.
						if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
                        // Otherwise update it with new costs.
						else
							openSet.UpdateItem(neighbour);
					}
				}
			}
		}

        // Once we have every node which could lead us to the destination, extract only those that will truly do.
		if (pathSuccess)
		{
			waypoints = RetracePath(startNode, targetNode);
			pathSuccess = waypoints.Length > 0;	// For when moving target a small range
		}

        // Set callback with success.
		callback(new PathResult(waypoints, pathSuccess, request.callback));
	}

    /// <summary>
    /// Calculates waypoints array from 'end node' to 'start node' based on it's parents.
    /// </summary>
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

    /// <summary>
    /// Extract waypoints from the path nodes only those in which direction changes.
    /// </summary>
	Vector3[] SimplifyPath(List<Node> path)
	{
		List<Vector3> waypoints = new List<Vector3>();

		Vector2 directionOld = Vector2.zero;
        // For each node in path, add to result only those in which direction changes.
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

    /// <summary>
    /// Returns distance cost between nodes.
    /// </summary>
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
