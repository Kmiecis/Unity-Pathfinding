using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid representation of walkable space.
/// </summary>
public class Grid
{
    Node[,] m_Nodes;

    public class SizeInfo
    {
        Grid grid;
        public int X { get { return grid.m_Nodes.GetLength(0); } }
        public int Y { get { return grid.m_Nodes.GetLength(1); } }
        public SizeInfo(Grid grid) { this.grid = grid; }
    }
    /// <summary>
    /// Size of the Grid.
    /// </summary>
    public SizeInfo Size;

    /// <summary>
    /// Total Nodes count within Grid.
    /// </summary>
    public int Count { get { return Size.X * Size.Y; } }

    public class NodeInfo
    {
        public float Radius;
        public float Diameter;
        public NodeInfo(float radius) { Radius = radius; Diameter = Radius * 2; }
    }
    /// <summary>
    /// Information about each Node within Grid.
    /// </summary>
    public NodeInfo Node;

    public Grid(float plane_size_x, float plane_size_y, float node_radius)
    {
        Size = new SizeInfo(this);
        Node = new NodeInfo(node_radius);

        m_Nodes = new Node[
            Mathf.RoundToInt(plane_size_x / Node.Diameter),
            Mathf.RoundToInt(plane_size_y / Node.Diameter)
        ];
    }

    public Node this[int x, int y]
    {
        get
        {
            return m_Nodes[x, y];
        }
        set
        {
            m_Nodes[x, y] = value;
        }
    }

    /// <summary>
    /// Returns Node based on passed position.
    /// </summary>
    public Node GetNodeFromPosition(Vector3 position)
    {
        // PercentX is value [0, 1] depending on where in the world is our target. 0 - left, .5 - middle, 1 - right.
        float percentX = (position.x + Size.X * Node.Diameter / 2) / Size.X * Node.Diameter;
        float percentY = (position.z + Size.Y * Node.Diameter / 2) / Size.Y * Node.Diameter;

        // Clamping value prevents from a situation, where target is actually outside of the plane.
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((Size.X - 1) * percentX);
        int y = Mathf.RoundToInt((Size.Y - 1) * percentY);

        return this[x, y];
    }

    /// <summary>
    /// Returns neightbours of passed Node.
    /// </summary>
    public List<Node> GetNodeNeightbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; ++x)
        {
            for (int y = -1; y <= 1; ++y)
            {
                if (x == 0 && y == 0)
                    continue;
                int checkX = node.x + x;
                int checkY = node.y + y;

                // Check if is inside the grid
                if (checkX >= 0 && checkX < Size.X && checkY >= 0 && checkY < Size.Y)
                {
                    neighbours.Add(this[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public bool IsWithin(int x, int y)
    {
        return (x >= 0 && x < Size.X && y >= 0 && y < Size.Y);
    }
}
