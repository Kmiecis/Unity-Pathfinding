using Common.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
    public static class PF_Core
    {
        private static readonly Vector2Int[] kDirections = new Vector2Int[]
        {
            new Vector2Int(-1,  0),
            new Vector2Int( 0,  1),
            new Vector2Int( 1,  0),
            new Vector2Int( 0, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-1,  1),
            new Vector2Int( 1,  1),
            new Vector2Int( 1, -1)
        };

        public static bool TryFindPath(PF_IMapper mapper, Vector2Int start, Vector2Int target, out List<Vector2Int> path)
        {
            var nodes = new Dictionary<Vector2Int, PF_Node>();

            var startNode = new PF_Node(start.x, start.y) { cost = 0 };
            var targetNode = new PF_Node(target.x, target.y);

            nodes[start] = startNode;
            nodes[target] = targetNode;

            var remaining = new List<PF_Node>() { startNode };
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

                    var neighbourXY = new Vector2Int(currentNode.x + direction.x, currentNode.y + direction.y);
                    if (!mapper.IsWalkable(neighbourXY))
                        continue;

                    if (!nodes.TryGetValue(neighbourXY, out var neighbourNode))
                        nodes[neighbourXY] = neighbourNode = new PF_Node(neighbourXY.x, neighbourXY.y);

                    if (neighbourNode.state == PF_ENodeState.Checked)
                        continue;

                    var totalCost = currentNode.cost + GetDistanceCost(currentNode, neighbourNode) + mapper.GetWalkCost(neighbourXY);
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

        private static int GetLowestTotalCostNodeIndex(List<PF_Node> nodes)
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

        private static int GetDistanceCost(PF_Node a, PF_Node b)
        {
            const int H_MUL = 14;
            const int L_MUL = 10;

            int dx = Math.Abs(b.x - a.x);
            int dy = Math.Abs(b.y - a.y);

            if (dx > dy)
            {
                return H_MUL * dy + L_MUL * (dx - dy);
            }
            return H_MUL * dx + L_MUL * (dy - dx);
        }

        private static List<Vector2Int> GetPathFromNode(PF_Node node)
        {
            var result = new List<Vector2Int>();

            while (node != null)
            {
                result.Add(new Vector2Int(node.x, node.y));
                node = node.link;
            }

            return result;
        }

        public static List<Vector2Int> GetTrimmedPath(List<Vector2Int> path)
        {
            var result = new List<Vector2Int>();

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
                        next.y - current.y != current.y - prev.y
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
