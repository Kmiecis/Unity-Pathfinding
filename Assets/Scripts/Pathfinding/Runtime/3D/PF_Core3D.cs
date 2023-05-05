using Common.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
    public static class PF_Core3D
    {
        private static readonly Vector3Int[] kDirections = new Vector3Int[]
        {
            new Vector3Int(-1, -1, -1),
            new Vector3Int(-1,  0, -1),
            new Vector3Int(-1,  1, -1),
            new Vector3Int( 0,  1, -1),
            new Vector3Int( 1,  1, -1),
            new Vector3Int( 1,  0, -1),
            new Vector3Int( 1, -1, -1),
            new Vector3Int( 0, -1, -1),
            new Vector3Int( 0,  0, -1),

            new Vector3Int(-1, -1,  0),
            new Vector3Int(-1,  0,  0),
            new Vector3Int(-1,  1,  0),
            new Vector3Int( 0,  1,  0),
            new Vector3Int( 1,  1,  0),
            new Vector3Int( 1,  0,  0),
            new Vector3Int( 1, -1,  0),
            new Vector3Int( 0, -1,  0),

            new Vector3Int(-1, -1,  1),
            new Vector3Int(-1,  0,  1),
            new Vector3Int(-1,  1,  1),
            new Vector3Int( 0,  1,  1),
            new Vector3Int( 1,  1,  1),
            new Vector3Int( 1,  0,  1),
            new Vector3Int( 1, -1,  1),
            new Vector3Int( 0, -1,  1),
            new Vector3Int( 0,  0,  1),
        };

        public static bool TryFindPath(PF_IMapper3D mapper, Vector3Int start, Vector3Int target, out List<Vector3Int> path)
        {
            var nodes = new Dictionary<Vector3Int, PF_Node3D>();

            var startNode = new PF_Node3D(start.x, start.y, start.z) { cost = 0 };
            var targetNode = new PF_Node3D(target.x, target.y, target.z);

            nodes[start] = startNode;
            nodes[target] = targetNode;

            var remaining = new List<PF_Node3D>() { startNode };
            while (remaining.Count > 0)
            {
                var currentIndex = GetLowestTotalCostNodeIndex(remaining);
                var currentNode = remaining[currentIndex];

                if (currentNode == targetNode)
                    break;

                remaining[currentIndex] = remaining[remaining.Count - 1];
                remaining.RemoveAt(remaining.Count - 1);

                currentNode.state = PF_ENodeState.Checked;

                for (int i = 0; i < kDirections.Length; i++)
                {
                    var direction = kDirections[i];

                    var neighbourPos = new Vector3Int(
                        currentNode.x + direction.x,
                        currentNode.y + direction.y,
                        currentNode.z + direction.z
                    );

                    if (!mapper.IsWalkable(neighbourPos))
                        continue;

                    if (!nodes.TryGetValue(neighbourPos, out var neighbourNode))
                        nodes[neighbourPos] = neighbourNode = new PF_Node3D(neighbourPos.x, neighbourPos.y, neighbourPos.z);

                    if (neighbourNode.state == PF_ENodeState.Checked)
                        continue;

                    var totalCost = currentNode.cost + GetDistanceCost(currentNode, neighbourNode) + mapper.GetWalkCost(neighbourPos);
                    if (totalCost < neighbourNode.cost)
                    {
                        neighbourNode.link = currentNode;
                        neighbourNode.cost = totalCost;
                        neighbourNode.totalcost = totalCost + GetDistanceCost(neighbourNode, targetNode);

                        if (neighbourNode.state < PF_ENodeState.Added)
                        {
                            remaining.Add(neighbourNode);
                            neighbourNode.state = PF_ENodeState.Added;
                        }
                    }
                }
            }

            path = GetPathFromNode(targetNode);
            if (path.Count < 2)
                return false;

            path.RemoveAt(path.Count - 1);
            return true;
        }

        private static int GetLowestTotalCostNodeIndex(List<PF_Node3D> nodes)
        {   // Switching this with priority queue could reduce running time but add complexity. To consider.
            var result = 0;
            var nodeA = nodes[result];

            for (int i = 1; i < nodes.Count; i++)
            {
                var nodeB = nodes[i];

                if (nodeA.totalcost > nodeB.totalcost)
                {
                    result = i;
                    nodeA = nodeB;
                }
            }

            return result;
        }

        private static int GetDistanceCost(PF_Node3D a, PF_Node3D b)
        {
            const int H_MUL = 20;
            const int M_MUL = 14;
            const int L_MUL = 10;

            int dx = Math.Abs(b.x - a.x);
            int dy = Math.Abs(b.y - a.y);
            int dz = Math.Abs(b.z - a.z);
            
            if (dx > dy)
            {
                if (dz > dx)
                {   // dz > dx > dy
                    return H_MUL * dy + M_MUL * (dx - dy) + L_MUL * (dz - dx);
                }
                // dx >= dz
                if (dz > dy)
                {   // dx >= dz > dy
                    return H_MUL * dy + M_MUL * (dz - dy) + L_MUL * (dx - dz);
                }
                // dy >= dz
                // dx > dy >= dz
                return H_MUL * dz + M_MUL * (dy - dz) + L_MUL * (dx - dy);
            }
            // dy >= dx
            if (dz > dy)
            {   // dz > dy >= dx
                return H_MUL * dx + M_MUL * (dy - dx) + L_MUL * (dz - dy);
            }
            // dy >= dz
            if (dz > dx)
            {   // dy >= dz > dx
                return H_MUL * dx + M_MUL * (dz - dx) + L_MUL * (dy - dz);
            }
            // dx >= dz
            // dy >= dx >= dz
            return H_MUL * dz + M_MUL * (dx - dz) + L_MUL * (dy - dx);
        }

        private static List<Vector3Int> GetPathFromNode(PF_Node3D node)
        {
            var result = new List<Vector3Int>();
            while (node != null)
            {
                result.Add(new Vector3Int(node.x, node.y, node.z));
                node = node.link;
            }
            return result;
        }

        public static List<Vector3Int> GetTrimmedPath(List<Vector3Int> path)
        {
            var result = new List<Vector3Int>();

            if (path.First() != path.Last())
            {
                result.Add(path.Last());
                for (int i = path.Count - 2; i > 0; --i)
                {
                    var prev = path[i + 1];
                    var current = path[i];
                    var next = path[i - 1];

                    if (
                        next.x - current.x != current.x - prev.x ||
                        next.y - current.y != current.y - prev.y ||
                        next.z - current.z != current.z - prev.z
                    )
                    {
                        result.Add(current);
                    }
                }
                result.Add(path.First());
            }

            return result;
        }
    }
}
