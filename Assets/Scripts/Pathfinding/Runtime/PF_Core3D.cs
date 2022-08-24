using Common.Extensions;
using Common.Mathematics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
    public static class PF_Core3D
    {
        [Flags]
        private enum ENodeState : byte
        {
            Idle = 0,
            Added = 1,
            Checked = 2
        }

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

        public static bool TryFindPath(bool[][][] map, Vector3Int start, Vector3Int target, out List<Vector3Int> path)
        {
            path = default;

            if (!map[start.x][start.y][start.z] || !map[target.x][target.y][target.z])
                return false;

            var width = map.GetWidth();
            var height = map.GetHeight();
            var depth = map.GetDepth();

            var grid = new PF_Node3D[width * height * depth];
            var gridRange = new Range3Int(0, 0, 0, width - 1, height - 1, depth - 1);

            var startNodeIndex = start.x + start.y * width + start.z * width * height;
            var startNode = grid[startNodeIndex] = new PF_Node3D(start.x, start.y, start.z) { gScore = 0 };

            var targetNodeIndex = target.x + target.y * width + start.z * width * height;
            var targetNode = grid[targetNodeIndex] = new PF_Node3D(target.x, target.y, target.z);

            var checkedArray = new ENodeState[width * height];

            var remaining = new List<PF_Node3D>() { startNode };
            while (remaining.Count > 0)
            {
                var currentIndex = GetLowestTotalCostNodeIndex(remaining);
                var currentNode = remaining[currentIndex];

                if (currentIndex == targetNodeIndex)
                    break;

                remaining.SwapLast(currentIndex);
                remaining.RemoveLast();

                var currentNodeIndex = currentNode.x + currentNode.y * width;
                checkedArray[currentNodeIndex] = ENodeState.Checked;

                for (int i = 0; i < kDirections.Length; i++)
                {
                    var direction = kDirections[i];

                    var neighbourX = currentNode.x + direction.x;
                    var neighbourY = currentNode.y + direction.y;
                    var neighbourZ = currentNode.z + direction.z;

                    if (!gridRange.Contains(neighbourX, neighbourY, neighbourZ))
                        continue;

                    if (!map[neighbourX][neighbourY][neighbourZ])
                        continue;

                    var neighbourNodeIndex = neighbourX + neighbourY * width + neighbourZ * width * height;
                    if (checkedArray[neighbourNodeIndex] == ENodeState.Checked)
                        continue;

                    var neighbourNode = grid[neighbourNodeIndex];
                    if (neighbourNode == null)
                        neighbourNode = grid[neighbourNodeIndex] = new PF_Node3D(neighbourX, neighbourY, neighbourZ);

                    var totalCumulativeCost = currentNode.gScore + GetDistanceCost(currentNode, neighbourNode);
                    if (totalCumulativeCost < neighbourNode.gScore)
                    {
                        neighbourNode.link = currentNode;
                        neighbourNode.gScore = totalCumulativeCost;
                        neighbourNode.fScore = totalCumulativeCost + GetDistanceCost(neighbourNode, targetNode);

                        if (checkedArray[neighbourNodeIndex] < ENodeState.Added)
                        {
                            remaining.Add(neighbourNode);
                            checkedArray[neighbourNodeIndex] = ENodeState.Added;
                        }
                    }
                }
            }

            path = GetPathFromNode(targetNode);
            return path.Count > 1;
        }

        private static int GetLowestTotalCostNodeIndex(List<PF_Node3D> nodes)
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
                {
                    return H_MUL * dz + M_MUL * (dz - dx) + L_MUL * (dx - dy);
                }
                // dx >= dz
                if (dz > dy)
                {
                    return H_MUL * dx + M_MUL * (dx - dz) + L_MUL * (dz - dy);
                }
                // dy >= dz
                return H_MUL * dx + M_MUL * (dx - dy) + L_MUL * (dy - dz);
            }
            // dy >= dx
            if (dz > dy)
            {
                return H_MUL * dz + M_MUL * (dz - dy) + L_MUL * (dy - dx);
            }
            // dy >= dz
            if (dz > dx)
            {
                return H_MUL * dy + M_MUL * (dy - dz) + L_MUL * (dz - dx);
            }
            // dx >= dz
            return H_MUL * dy + M_MUL * (dy - dx) + L_MUL * (dx - dz);
        }

        private static List<Vector3Int> GetPathFromNode(PF_Node3D node)
        {
            var nodes = new List<Vector3Int>();
            while (node != null)
            {
                nodes.Add(new Vector3Int(node.x, node.y, node.z));
                node = node.link;
            }

            var result = new List<Vector3Int>();
            if (nodes.First() != nodes.Last())
            {
                result.Add(nodes.Last());
                for (int i = nodes.Count - 2; i > 0; --i)
                {
                    var prev = nodes[i + 1];
                    var current = nodes[i];
                    var next = nodes[i - 1];

                    if (
                        next.x - current.x != current.x - prev.x ||
                        next.y - current.y != current.y - prev.y ||
                        next.z - current.z != current.z - prev.z
                    )
                    {
                        result.Add(current);
                    }
                }
                result.Add(nodes.First());
            }

            return result;
        }
    }
}
