using Common;
using Common.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
    public static class PathGenerator
    {
        private static readonly Vector2Int[] _directions = new Vector2Int[]
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

        public static bool TryFindPath(bool[,] map, int startX, int startY, int targetX, int targetY, out PathNode node)
        {
            node = default;

            if (!map[startX, startY] || !map[targetX, targetY])
                return false;

            var width = map.GetWidth();
            var height = map.GetHeight();

            var grid = new PathNode[width, height];
            var gridRange = new Range2Int(0, 0, width - 1, height - 1);

            var startNode = grid[startX, startY] = new PathNode(startX, startY) { gScore = 0 };
            var targetNode = grid[targetX, targetY] = node = new PathNode(targetX, targetY);

            const byte NODE_ADDED = 1;
            const byte NODE_CHECKED = 2;
            var checkedArray = new byte[width, height];

            var remaining = new List<PathNode>() { startNode };
            while (remaining.Count > 0)
            {
                var currentNodeIndex = GetLowestTotalCostNodeIndex(remaining);
                var currentNode = remaining[currentNodeIndex];

                if (currentNode == targetNode)
                    break;

                remaining.SwapLast(currentNodeIndex);
                remaining.RemoveLast();

                checkedArray[currentNode.x, currentNode.y] = NODE_CHECKED;

                for (int i = 0; i < _directions.Length; i++)
                {
                    var direction = _directions[i];

                    var neighbourX = currentNode.x + direction.x;
                    var neighbourY = currentNode.y + direction.y;

                    if (!gridRange.Contains(neighbourX, neighbourY))
                        continue;

                    if (checkedArray[neighbourX, neighbourY] == NODE_CHECKED)
                        continue;

                    if (!map[neighbourX, neighbourY])
                        continue;

                    var neighbourNode = grid[neighbourX, neighbourY];
                    if (neighbourNode == null)
                        neighbourNode = grid[neighbourX, neighbourY] = new PathNode(neighbourX, neighbourY);

                    var totalCumulativeCost = currentNode.gScore + GetDistanceCost(currentNode, neighbourNode);
                    if (totalCumulativeCost < neighbourNode.gScore)
                    {
                        neighbourNode.link = currentNode;
                        neighbourNode.gScore = totalCumulativeCost;
                        neighbourNode.fScore = totalCumulativeCost + GetDistanceCost(neighbourNode, targetNode);

                        if (checkedArray[neighbourX, neighbourY] < NODE_ADDED)
                        {
                            remaining.Add(neighbourNode);
                            checkedArray[neighbourX, neighbourY] = NODE_ADDED;
                        }
                    }
                }
            }

            return node.link != null;
        }

        private static int GetLowestTotalCostNodeIndex(List<PathNode> nodes)
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

        private static int GetDistanceCost(PathNode a, PathNode b)
        {
            int dx = Math.Abs(b.x - a.x);
            int dy = Math.Abs(b.y - a.y);
            if (dx > dy)
                return 14 * dy + 10 * (dx - dy);
            return 14 * dx + 10 * (dy - dx);
        }

        public static List<Vector2Int> ToPath(PathNode node)
        {
            var result = new List<Vector2Int>();
            while (node != null)
            {
                result.Add(new Vector2Int(node.x, node.y));
                node = node.link;
            }
            return result;
        }

        public static List<Vector2Int> ToLeanPath(PathNode node)
        {
            var result = new List<Vector2Int>();

            while (node != null)
            {
                if (result.Count > 1 && node.link != null)
                {
                    var last = result.Last();
                    if (
                        Math.Sign(node.x - last.x) != Math.Sign(node.link.x - node.x) ||
                        Math.Sign(node.y - last.y) != Math.Sign(node.link.y - node.y)
                    )
                    {
                        result.Add(new Vector2Int(node.x, node.y));
                    }
                }
                else
                {
                    result.Add(new Vector2Int(node.x, node.y));
                }

                node = node.link;
            }

            return result;
        }
    }
}
