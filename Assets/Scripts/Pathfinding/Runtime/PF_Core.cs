using Common.Extensions;
using Common.Mathematics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
    public static class PF_Core
    {
        private static readonly Vector2Int[] kDirections = new Vector2Int[]
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

        public static bool TryFindPath(bool[,] map, Vector2Int start, Vector2Int target, out List<Vector2Int> path)
        {
            path = default;

            if (!map[start.x, start.y] || !map[target.x, target.y])
                return false;

            var width = map.GetWidth();
            var height = map.GetHeight();
            
            var grid = new PF_Node[width * height];
            var gridRange = new Range2Int(0, 0, width - 1, height - 1);

            var startNodeIndex = start.x + start.y * width;
            var startNode = grid[startNodeIndex] = new PF_Node(start.x, start.y) { gScore = 0 };

            var targetNodeIndex = target.x + target.y * width;
            var targetNode = grid[targetNodeIndex] = new PF_Node(target.x, target.y);

            const byte NODE_ADDED = 1;
            const byte NODE_CHECKED = 2;
            var checkedArray = new byte[width * height];

            var remaining = new List<PF_Node>() { startNode };
            while (remaining.Count > 0)
            {
                var currentIndex = GetLowestTotalCostNodeIndex(remaining);
                var currentNode = remaining[currentIndex];

                if (currentIndex == targetNodeIndex)
                    break;

                remaining.SwapLast(currentIndex);
                remaining.RemoveLast();

                var currentNodeIndex = currentNode.x + currentNode.y * width;
                checkedArray[currentNodeIndex] = NODE_CHECKED;

                for (int i = 0; i < kDirections.Length; i++)
                {
                    var direction = kDirections[i];

                    var neighbourX = currentNode.x + direction.x;
                    var neighbourY = currentNode.y + direction.y;

                    if (!gridRange.Contains(neighbourX, neighbourY))
                        continue;

                    if (!map[neighbourX, neighbourY])
                        continue;

                    var neighbourNodeIndex = neighbourX + neighbourY * width;
                    if (checkedArray[neighbourNodeIndex] == NODE_CHECKED)
                        continue;

                    var neighbourNode = grid[neighbourNodeIndex];
                    if (neighbourNode == null)
                        neighbourNode = grid[neighbourNodeIndex] = new PF_Node(neighbourX, neighbourY);

                    var totalCumulativeCost = currentNode.gScore + GetDistanceCost(currentNode, neighbourNode);
                    if (totalCumulativeCost < neighbourNode.gScore)
                    {
                        neighbourNode.link = currentNode;
                        neighbourNode.gScore = totalCumulativeCost;
                        neighbourNode.fScore = totalCumulativeCost + GetDistanceCost(neighbourNode, targetNode);

                        if (checkedArray[neighbourNodeIndex] < NODE_ADDED)
                        {
                            remaining.Add(neighbourNode);
                            checkedArray[neighbourNodeIndex] = NODE_ADDED;
                        }
                    }
                }
            }

            path = GetPathFromNode(targetNode);
            return path.Count > 1;
        }

        private static int GetLowestTotalCostNodeIndex(List<PF_Node> nodes)
        {   // Switching this with priority queue could reduce running time but add complexity. To consider.
            var result = 0;
            var nodeA = nodes[result];

            for (int i = 1; i < nodes.Count; i++)
            {
                var nodeB = nodes[i];

                if (nodeA.fScore > nodeB.fScore)
                {
                    result = i;
                    nodeA = nodeB;
                }
            }

            return result;
        }

        private static int GetDistanceCost(PF_Node a, PF_Node b)
        {
            int dx = Math.Abs(b.x - a.x);
            int dy = Math.Abs(b.y - a.y);
            if (dx > dy)
                return 14 * dy + 10 * (dx - dy);
            return 14 * dx + 10 * (dy - dx);
        }

        private static List<Vector2Int> GetPathFromNode(PF_Node node)
        {
            var nodes = new List<Vector2Int>();
            while (node != null)
            {
                nodes.Add(new Vector2Int(node.x, node.y));
                node = node.link;
            }

            var result = new List<Vector2Int>();
            result.Add(nodes.Last());
            for (int i = nodes.Count - 2; i > 0; --i)
            {
                var prev = nodes[i + 1];
                var current = nodes[i];
                var next = nodes[i - 1];

                if (
                    next.x - current.x != current.x - prev.x ||
                    next.y - current.y != current.y - prev.y
                )
                {
                    result.Add(current);
                }
            }
            result.Add(nodes.First());

            return result;
        }
    }
}
