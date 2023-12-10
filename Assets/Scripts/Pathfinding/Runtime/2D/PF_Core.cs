using Common;
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

        public static bool TryFindPath(PF_IMapper mapper, Vector2Int start, Vector2Int target, int size, out List<Vector2Int> path)
        {
            var nodes = new Dictionary<Vector2Int, PF_Node>();

            var startNode = new PF_Node(start.x, start.y) { cost = 0 };
            var targetNode = new PF_Node(target.x, target.y);

            nodes[start] = startNode;
            nodes[target] = targetNode;

            var remaining = new PriorityQueue<PF_Node>();
            remaining.Enqueue(startNode);

            while (remaining.Count > 0)
            {
                var currentNode = remaining.Dequeue();
                var currentXY = new Vector2Int(currentNode.x, currentNode.y);

                if (IsTargetReached(currentXY, target, size))
                    break;

                currentNode.state = PF_ENodeState.Checked;

                for (int i = 0; i < kDirections.Length; i++)
                {
                    var direction = kDirections[i];

                    var neighbourXY = new Vector2Int(currentNode.x + direction.x, currentNode.y + direction.y);

                    if (!mapper.IsWalkable(neighbourXY, size))
                        continue;

                    if (!nodes.TryGetValue(neighbourXY, out var neighbourNode))
                        nodes[neighbourXY] = neighbourNode = new PF_Node(neighbourXY.x, neighbourXY.y);

                    if (neighbourNode.state == PF_ENodeState.Checked)
                        continue;

                    var totalCost = currentNode.cost + GetDistanceCost(currentNode, neighbourNode) + mapper.GetWalkCost(neighbourXY, size);
                    if (totalCost < neighbourNode.cost)
                    {
                        neighbourNode.link = currentNode;
                        neighbourNode.cost = totalCost;
                        neighbourNode.totalcost = totalCost + GetDistanceCost(neighbourNode, targetNode);

                        if (neighbourNode.state < PF_ENodeState.Added)
                        {
                            remaining.Enqueue(neighbourNode);
                            neighbourNode.state = PF_ENodeState.Added;
                        }
                    }
                }
            }

            SmoothNodes(targetNode, size, mapper);

            path = GetPathFromNode(targetNode);
            if (path.Count < 2)
                return false;

            path.RemoveAt(path.Count - 1);
            return true;
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

        private static bool IsTargetReached(Vector2Int point, Vector2Int target, int size)
        {
            var min = point;
            var max = point + new Vector2Int(size, size);
            return (
                min.x <= target.x && target.x <= max.x &&
                min.y <= target.y && target.y <= max.y
            );
        }

        private static bool IsLineStraight(Vector2Int a, Vector2Int b, Vector2Int c)
        {
            return (
                c.x - b.x == b.x - a.x &&
                c.y - b.y == b.y - a.y
            );
        }

        private static bool IsLineWalkable(Vector2Int a, Vector2Int b, PF_IMapper mapper, int size)
        {
            foreach (var point in GetThinLine(a, b))
            {
                if (!mapper.IsWalkable(point, size))
                {
                    return false;
                }
            }
            return true;
        }

        private static IEnumerable<Vector2Int> GetThinLine(Vector2Int a, Vector2Int b)
        {
            int x = a.x;
            int y = a.y;

            int dx = b.x - a.x;
            int dy = b.y - a.y;

            int abs_dx = Mathf.Abs(dx);
            int abs_dy = Mathf.Abs(dy);

            int min = Mathf.Min(abs_dx, abs_dy);
            int max = Mathf.Max(abs_dx, abs_dy);

            var step = Vector2Int.zero;
            var acc_step = Vector2Int.zero;

            if (abs_dy > abs_dx)
            {
                step.y = Mathf.RoundToInt(Mathf.Sign(dy));
                acc_step.x = Mathf.RoundToInt(Mathf.Sign(dx));
            }
            else
            {
                step.x = Mathf.RoundToInt(Mathf.Sign(dx));
                acc_step.y = Mathf.RoundToInt(Mathf.Sign(dy));
            }

            int acc = max / 2;

            for (int i = 0; i < max; i++)
            {
                yield return new Vector2Int(x, y);

                x += step.x;
                y += step.y;

                acc += min;
                if (acc >= max)
                {
                    x += acc_step.x;
                    y += acc_step.y;

                    acc -= max;
                }
            }

            yield return b;
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

        private static void SmoothNodes(PF_Node node, int size, PF_IMapper mapper)
        {
            while (node.link != null && node.link.link != null)
            {
                var target = node.link.link;

                var nodeXY = new Vector2Int(node.x, node.y);
                var targetXY = new Vector2Int(target.x, target.y);

                if (IsLineWalkable(nodeXY, targetXY, mapper, size))
                {
                    node.link = target;
                }
                else
                {
                    node = node.link;
                }
            }
        }

        private static void TrimNodes(PF_Node node)
        {
            while (node.link != null && node.link.link != null)
            {
                var prev = node;
                var current = node.link;
                var next = node.link.link;

                var prevXY = new Vector2Int(prev.x, prev.y);
                var currentXY = new Vector2Int(current.x, current.y);
                var nextXY = new Vector2Int(next.x, next.y);

                if (IsLineStraight(prevXY, currentXY, nextXY))
                {
                    node.link = next;
                }
                else
                {
                    node = node.link;
                }
            }
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
    }
}
