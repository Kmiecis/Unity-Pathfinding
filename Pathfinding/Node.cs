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
        // Cost from the start node to current.
        public int g;
        // Cost from the current to end node, estimated.
        public int h;
        // Cost g + h.
        public int f { get { return g + h; } }
    }
    public Cost cost;

    public int HeapIndex { get; set; }

    public Node parent; // No parent by default.

	public Node(bool walkable, Vector3 position, int x, int y, int movementPenalty)
	{
		this.walkable = walkable;
		this.position = position;
        this.x = x;
        this.y = y;
		this.movementPenalty = movementPenalty;
	}

    /// <summary>
    /// Node specification of CompareTo method. Compares it's f costs or h costs.
    /// Returns flipped value: 1 when lower and -1 when higher, because it's more valuable to us if it has lower value.
    /// </summary>
	public int CompareTo(Node node)
	{
		int compare = cost.f.CompareTo(node.cost.f);

		if (compare == 0) compare = cost.h.CompareTo(node.cost.h);
		
		return -compare;    // CompareTo returns 1 if this value is higher, but we want the opposite, thus -
    }
}
