using Common.Collections;
using Common.Extensions;
using Common.Mathematics;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
    public class PF_Core
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

        public static bool TryFindPath(PF_IMapper map, Vector2Int start, Vector2Int target, int size, out List<Vector2Int> path)
        {
            path = new List<Vector2Int>();

            var reached = TryFindLinkNode(map, start, target, size, out var node);
            GetPathFromNode(node, path);

            return reached;
        }

        public static void SmoothPath(PF_IMapper map, int size, List<Vector2Int> refPath)
        {
            for (int i = 0; i < refPath.Count - 2; ++i)
            {
                var start = refPath[i];
                var target = refPath[i + 2];

                if (IsLineWalkable(start, target, size, map))
                {
                    refPath.RemoveAt(i + 1);
                    i -= 1;
                }
            }
        }

        public static void TrimPath(List<Vector2Int> refPath)
        {
            var temp = new List<Vector2Int>();

            temp.Add(refPath.First());
            for (int i = 1; i < refPath.Count - 1; ++i)
            {
                var prev = refPath[i - 1];
                var current = refPath[i];
                var next = refPath[i + 1];

                var ndx = next.x - current.x;
                var pdx = current.x - prev.x;
                var ndy = next.y - current.y;
                var pdy = current.y - prev.y;

                if (ndx != pdx || ndy != pdy)
                {
                    temp.Add(current);
                }
            }
            temp.Add(refPath.Last());

            refPath.Clear();
            refPath.AddRange(temp);
        }

        private static bool TryFindLinkNode(PF_IMapper map, Vector2Int start, Vector2Int target, int size, out PF_Node node)
        {
            var startNode = new PF_Node(start.x, start.y) { cost = 0 };
            var targetNode = new PF_Node(target.x, target.y);
            var closestNode = targetNode;

            if (start != target)
            {
                var nodes = new Dictionary<Vector2Int, PF_Node>();
                nodes[start] = startNode;
                nodes[target] = targetNode;

                var remaining = new PriorityQueue<PF_Node>();
                remaining.Enqueue(startNode);

                while (remaining.Count > 0)
                {
                    var currentNode = remaining.Dequeue();

                    currentNode.state = PF_ENodeState.Checked;
                    var currentPos = new Vector2Int(currentNode.x, currentNode.y);

                    for (int i = 0; i < kDirections.Length; i++)
                    {
                        var direction = kDirections[i];

                        var neighbour = new Vector2Int(currentNode.x + direction.x, currentNode.y + direction.y);

                        if (!nodes.TryGetValue(neighbour, out var neighbourNode))
                        {
                            nodes[neighbour] = neighbourNode = new PF_Node(neighbour.x, neighbour.y);
                        }

                        if (neighbourNode.state == PF_ENodeState.Checked)
                        {
                            continue;
                        }

                        if (!IsPathable(currentPos, direction, size, map))
                        {
                            continue;
                        }

                        if (IsTargetReached(neighbour, target, size))
                        {
                            neighbourNode.link = currentNode;
                            targetNode = neighbourNode;
                            closestNode = targetNode;
                            remaining.Clear();
                            break;
                        }

                        var walkMultiplier = GetWalkMultiplier(neighbour, size, map);
                        var totalCost = currentNode.cost + GetDistanceCost(currentNode, neighbourNode) * walkMultiplier;
                        if (totalCost < neighbourNode.cost)
                        {
                            neighbourNode.link = currentNode;
                            neighbourNode.cost = totalCost;
                            neighbourNode.distanceCost = GetDistanceCost(neighbourNode, targetNode);
                            neighbourNode.totalCost = totalCost + neighbourNode.distanceCost;

                            if (neighbourNode.state < PF_ENodeState.Added)
                            {
                                remaining.Enqueue(neighbourNode);
                                neighbourNode.state = PF_ENodeState.Added;
                            }

                            if (closestNode.distanceCost > neighbourNode.distanceCost)
                            {
                                closestNode = neighbourNode;
                            }
                        }
                    }
                }
            }

            node = closestNode;
            return node == targetNode;
        }

        private static float GetDistanceCost(PF_Node a, PF_Node b)
        {
            const float H_MUL = 14.1f;
            const float L_MUL = 10.0f;

            int dx = Mathf.Abs(b.x - a.x);
            int dy = Mathf.Abs(b.y - a.y);

            if (dx > dy)
            {
                return H_MUL * dy + L_MUL * (dx - dy);
            }
            return H_MUL * dx + L_MUL * (dy - dx);
        }

        private static bool IsTargetReached(Vector2Int point, Vector2Int target, int size)
        {
            var minx = point.x;
            var maxx = minx + size;

            var miny = point.y;
            var maxy = miny + size;

            return (
                minx <= target.x && target.x < maxx &&
                miny <= target.y && target.y < maxy
            );
        }

        private static bool IsLineWalkable(Vector2Int a, Vector2Int b, int size, PF_IMapper map)
        {
            foreach (var linePoint in GetCoveredPoints(a, b))
            {
                foreach (var sizePoint in GetSizePoints(linePoint, size))
                {
                    if (!map.IsPathable(sizePoint))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IsPathable(Vector2Int position, Vector2Int direction, int size, PF_IMapper map)
        {
            if (size == 1)
            {
                if (direction.x != 0 && direction.y != 0)
                {
                    var dpx = new Vector2Int(position.x + direction.x, position.y);
                    var dpy = new Vector2Int(position.x, position.y + direction.y);
                    if (!map.IsPathable(dpx) ||
                        !map.IsPathable(dpy))
                    {
                        return false;
                    }
                }

                var dp = new Vector2Int(position.x + direction.x, position.y + direction.y);
                return map.IsPathable(dp);
            }
            else
            {
                foreach (var offset in Geometry.GetOffsets(direction, size))
                {
                    var dp = new Vector2Int(position.x + offset.x, position.y + offset.y);
                    if (!map.IsPathable(dp))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static float GetWalkMultiplier(Vector2Int position, int size, PF_IMapper map)
        {
            if (size == 1)
            {
                return map.GetWalkMultiplier(position);
            }

            var result = 0.0f;
            foreach (var point in GetSizePoints(position, size))
            {
                result += map.GetWalkMultiplier(point);
            }
            return result / (size * size);
        }

        private static void GetPathFromNode(PF_Node node, List<Vector2Int> outPath)
        {
            outPath.Clear();

            while (node != null)
            {
                outPath.Add(new Vector2Int(node.x, node.y));
                node = node.link;
            }

            outPath.RemoveLast();
        }

        private static IEnumerable<Vector2Int> GetSizePoints(Vector2Int position, int size)
        {
            for (int dy = 0; dy < size; ++dy)
            {
                for (int dx = 0; dx < size; ++dx)
                {
                    yield return new Vector2Int(position.x + dx, position.y + dy);
                }
            }
        }

        private static IEnumerable<Vector2Int> GetCoveredPoints(Vector2Int a, Vector2Int b)
        {
            int abs_dx = Mathf.Abs(b.x - a.x);
            int abs_dy = Mathf.Abs(b.y - a.y);

            var min = Mathx.Min(a, b);
            var max = Mathx.Max(a, b);

            if (abs_dx > abs_dy)
            {
                foreach (var l in GetWalkableLine(a, b))
                {
                    var v = l;
                    yield return v;

                    v.y += 1;
                    if (v.y <= max.y)
                    {
                        yield return v;
                    }

                    v.y -= 2;
                    if (v.y >= min.y)
                    {
                        yield return v;
                    }
                }
            }
            else
            {
                foreach (var l in GetWalkableLine(a, b))
                {
                    var v = l;
                    yield return v;

                    v.x += 1;
                    if (v.x <= max.x)
                    {
                        yield return v;
                    }

                    v.x -= 2;
                    if (v.x >= min.x)
                    {
                        yield return v;
                    }
                }
            }
        }

        private static IEnumerable<Vector2Int> GetWalkableLine(Vector2Int a, Vector2Int b)
        {   // Geometry ThinLine with a twist
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
                step.y = Mathx.Sign(dy);
                acc_step.x = Mathx.Sign(dx);
            }
            else
            {
                step.x = Mathx.Sign(dx);
                acc_step.y = Mathx.Sign(dy);
            }

            int acc = max / 2;

            for (int i = 0; i < max; i++)
            {
                yield return new Vector2Int(x, y);

                x += step.x;
                y += step.y;

                acc += min;
                if (acc == max)
                {
                    yield return new Vector2Int(x, y);
                }
                if (acc >= max)
                {
                    x += acc_step.x;
                    y += acc_step.y;

                    acc -= max;
                }
            }

            yield return b;
        }
    }
}