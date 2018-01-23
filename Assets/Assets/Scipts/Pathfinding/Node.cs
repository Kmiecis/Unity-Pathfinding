using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single Node to represent the grid to walk on.
/// </summary>
public class Node : IHeapItem<Node>
{
    /// <summary>
    /// Determinant if this node can be walked on.
    /// </summary>
	public bool walkable;

    /// <summary>
    /// World position of node in 3D space.
    /// </summary>
    public Vector3 position;   

    /// <summary>
    /// Grid position of node in 2D space.
    /// </summary>
    public int x, y;

	public int movementPenalty;

    public struct Cost
    {
        public int g, h;
        public int f { get { return g + h; } }
    }
    public Cost cost;
	//public int gCost;
	//public int hCost;
	//public int fCost { get { return gCost + hCost; } }

    //int heapindex;
    public int HeapIndex { get; set; }

    public Node parent;

	public Node(bool walkable, Vector3 position, int x, int y, int movementPenalty)
	{
		this.walkable = walkable;
		this.position = position;
        this.x = x;
        this.y = y;
		this.movementPenalty = movementPenalty;
	}

	public int CompareTo(Node node)
	{
		int compare = cost.f.CompareTo(node.cost.f);

		if (compare == 0) compare = cost.h.CompareTo(node.cost.h);
		
		return -compare;    // CompareTo returns 1 if int is higher, but we want the opposite, thus -
    }
}
