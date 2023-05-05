using Common.Mathematics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.CaveGeneration
{
    public static class CaveGenerator3D
    {
        private const bool kRoom = true;
        private const bool kWall = false;

        private struct Line
        {
            public Vector3Int a;
            public Vector3Int b;
        }

        [Serializable]
        public struct Input
        {
            [Header("Map generation")]
            public int width;
            public int height;
            public int depth;
            public int smooths;
            [Range(0.45f, 0.55f)]
            public float fill;
            public string seed;

            [Header("Map processing")]
            public int wallThreshold;
            public int roomThreshold;
            public int passageWidth;
            public int borderWidth;

            public static readonly Input Default = new Input
            {
                width = 32,
                height = 32,
                depth = 32,
                smooths = 2,
                fill = 0.5f,
                seed = "",
                wallThreshold = 5,
                roomThreshold = 5,
                passageWidth = 2,
                borderWidth = 0
            };
        }

        public static void Generate(bool[] map, in Input input)
        {
            var random = new System.Random(input.seed.GetHashCode());
            Noisex.GetRandomMap(map, input.width, input.height, input.depth, input.fill, random);
            ApplyBorder(map, input.width, input.height, input.depth, input.borderWidth);
            Noisex.SmoothRandomMap(map, input.width, input.height, input.depth, input.smooths);

            var roomRegions = GetRegionsByType(map, input.width, input.height, input.depth, kRoom);
            var removedRoomRegions = RemoveRegionsUnderThreshold(roomRegions, input.roomThreshold);
            FlipRegions(removedRoomRegions, map, input.width, input.height);

            var wallRegions = GetRegionsByType(map, input.width, input.height, input.depth, kWall);
            var removedWallRegions = RemoveRegionsUnderThreshold(wallRegions, input.wallThreshold);
            FlipRegions(removedWallRegions, map, input.width, input.height);

            var rooms = CreateRooms(roomRegions, map, input.width, input.height, input.depth);
            var passages = FindPassages(rooms);
            ClearPassages(passages, map, input.width, input.height, input.depth, input.passageWidth, input.borderWidth);
        }

        private static void ApplyBorder(bool[] map, int width, int height, int depth, int border)
        {
            if (border > 0)
            {
                for (int z = 0; z < depth; ++z)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < border; x++)
                        {
                            int i = Mathx.ToIndex(x, y, z, width, height);
                            map[i] = kWall;
                        }

                        for (int x = width - border; x < width; x++)
                        {
                            int i = Mathx.ToIndex(x, y, z, width, height);
                            map[i] = kWall;
                        }
                    }
                }

                for (int x = border; x < width - border; x++)
                {
                    for (int y = 0; y < border; y++)
                    {
                        for (int z = 0; z < border; z++)
                        {
                            int i = Mathx.ToIndex(x, y, z, width, height);
                            map[i] = kWall;
                        }

                        for (int z = depth - border; z < depth; z++)
                        {
                            int i = Mathx.ToIndex(x, y, z, width, height);
                            map[i] = kWall;
                        }
                    }

                    for (int y = height - border - 1; y < height; y++)
                    {
                        for (int z = 0; z < border; z++)
                        {
                            int i = Mathx.ToIndex(x, y, z, width, height);
                            map[i] = kWall;
                        }

                        for (int z = depth - border; z < depth; z++)
                        {
                            int i = Mathx.ToIndex(x, y, z, width, height);
                            map[i] = kWall;
                        }
                    }
                }
            }
        }

        private static List<List<Vector3Int>> GetRegionsByType(bool[] map, int width, int height, int depth, bool type)
        {
            var result = new List<List<Vector3Int>>();

            var isChecked = new bool[width * height * depth];
            var mapRange = new Range3Int(0, 0, 0, width - 1, height - 1, depth - 1);

            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = Mathx.ToIndex(x, y, z, width, height);
                        if (map[i] == type && !isChecked[i])
                        {
                            var region = new List<Vector3Int>();
                            var toCheck = new Queue<Vector3Int>();

                            toCheck.Enqueue(new Vector3Int(x, y, z));
                            isChecked[i] = true;

                            while (toCheck.Count > 0)
                            {
                                var current = toCheck.Dequeue();
                                region.Add(current);

                                foreach (var direction in Axes.All3D)
                                {
                                    var neighbour = current + direction;
                                    var ni = Mathx.ToIndex(neighbour.x, neighbour.y, neighbour.z, width, height);

                                    if (
                                        mapRange.Contains(neighbour) &&
                                        map[ni] == type && !isChecked[ni]
                                    )
                                    {
                                        toCheck.Enqueue(neighbour);
                                        isChecked[ni] = true;
                                    }
                                }
                            }

                            result.Add(region);
                        }
                    }
                }
            }

            return result;
        }

        private static List<List<Vector3Int>> RemoveRegionsUnderThreshold(List<List<Vector3Int>> regions, int threshold)
        {
            var removed = new List<List<Vector3Int>>();

            for (int i = regions.Count - 1; i > -1; i--)
            {
                var region = regions[i];

                if (region.Count < threshold)
                {
                    regions.RemoveAt(i);
                    removed.Add(region);
                }
            }

            return removed;
        }

        private static void FlipRegions(List<List<Vector3Int>> regions, bool[] map, int width, int height)
        {
            foreach (var region in regions)
            {
                foreach (var tile in region)
                {
                    var i = Mathx.ToIndex(tile.x, tile.y, tile.z, width, height);
                    map[i] = !map[i];
                }
            }
        }

        private static List<List<Vector3Int>> CreateRooms(List<List<Vector3Int>> regions, bool[] map, int width, int height, int depth)
        {
            var result = new List<List<Vector3Int>>();

            var isChecked = new bool[width * height * depth];
            var mapRange = new Range3Int(0, 0, 0, width - 1, height - 1, depth - 1);

            foreach (var region in regions)
            {
                var room = new List<Vector3Int>();

                foreach (var tile in region)
                {
                    foreach (var direction in Axes.All3D)
                    {
                        var neighbour = tile + direction;
                        var ni = Mathx.ToIndex(neighbour.x, neighbour.y, neighbour.z, width, height);

                        if (
                            mapRange.Contains(neighbour) &&
                            map[ni] == kWall && !isChecked[ni]
                        )
                        {
                            room.Add(neighbour);
                            isChecked[ni] = true;
                        }
                    }
                }

                result.Add(room);
            }

            return result;
        }

        // TODO: Bottleneck. Find a more optimal way.
        private static List<Line> FindPassages(List<List<Vector3Int>> rooms)
        {
            var result = new List<Line>();

            if (rooms.Count < 1)
                return result;

            var roomA = rooms[0];

            while (rooms.Count > 1)
            {
                var line = new Line();
                var distance = int.MaxValue;
                var index = 0;

                for (int i = 1; i < rooms.Count; ++i)
                {
                    var roomB = rooms[i];

                    for (int ta = 0; ta < roomA.Count; ta++)
                    {
                        var tileA = roomA[ta];

                        for (int tb = 0; tb < roomB.Count; tb++)
                        {
                            var tileB = roomB[tb];

                            var dx = tileB.x - tileA.x;
                            var dy = tileB.y - tileA.y;
                            var dz = tileB.z - tileA.z;

                            var currentDistance = dx * dx + dy * dy + dz * dz;

                            if (currentDistance < distance)
                            {
                                line.a = tileA;
                                line.b = tileB;

                                distance = currentDistance;

                                index = i;
                            }
                        }
                    }
                }

                roomA.AddRange(rooms[index]);
                rooms.RemoveAt(index);

                result.Add(line);
            }

            return result;
        }

        private static List<Vector3Int> GetTilesOnLine(Vector3Int a, Vector3Int b)
        {
            var result = new List<Vector3Int>();
            result.Add(a);

            var d = Mathx.Abs(b - a);
            var s = Mathx.Select(-Vector3Int.one, Vector3Int.one, Mathx.AreGreater(b, a));

            if (d.x >= d.y && d.x >= d.z)
            {   // Driving axis is X-axis"
                var p1 = 2 * d.y - d.x;
                var p2 = 2 * d.z - d.x;
                while (a.x != b.x)
                {
                    a.x += s.x;
                    if (p1 >= 0)
                    {
                        a.y += s.y;
                        p1 -= 2 * d.x;
                    }
                    if (p2 >= 0)
                    {
                        a.z += s.z;
                        p2 -= 2 * d.x;
                    }
                    p1 += 2 * d.y;
                    p2 += 2 * d.z;

                    result.Add(a);
                }
            }
            else if (d.y >= d.x && d.y >= d.z)
            {   // Driving axis is Y-axis"
                var p1 = 2 * d.x - d.y;
                var p2 = 2 * d.z - d.y;
                while (a.y != b.y)
                {
                    a.y += s.y;
                    if (p1 >= 0)
                    {
                        a.x += s.x;
                        p1 -= 2 * d.y;
                    }
                    if (p2 >= 0)
                    {
                        a.z += s.z;
                        p2 -= 2 * d.y;
                    }
                    p1 += 2 * d.x;
                    p2 += 2 * d.z;

                    result.Add(a);
                }
            }
            else
            {   // Driving axis is Z-axis"
                var p1 = 2 * d.y - d.z;
                var p2 = 2 * d.x - d.z;
                while (a.z != b.z)
                {
                    a.z += s.z;
                    if (p1 >= 0)
                    {
                        a.y += s.y;
                        p1 -= 2 * d.z;
                    }
                    if (p2 >= 0)
                    {
                        a.x += s.x;
                        p2 -= 2 * d.z;
                    }
                    p1 += 2 * d.y;
                    p2 += 2 * d.x;

                    result.Add(a);
                }
            }

            return result;
        }

        private static void ClearPassages(List<Line> passages, bool[] map, int width, int height, int depth, int passageWidth, int borderWidth)
        {
            foreach (var passage in passages)
            {
                var tiles = GetTilesOnLine(passage.a, passage.b);

                foreach (var tile in tiles)
                {
                    ClearCircle(tile, passageWidth, map, width, height, depth, borderWidth);
                }
            }
        }

        private static void ClearCircle(Vector3Int tile, int r, bool[] map, int width, int height, int depth, int borderWidth)
        {
            var mapRange = new Range3Int(
                borderWidth, borderWidth, borderWidth,
                width - borderWidth - 1, height - borderWidth - 1, depth - borderWidth - 1
            );

            for (int z = -r; z <= r; z++)
            {
                for (int y = -r; y <= r; y++)
                {
                    for (int x = -r; x <= r; x++)
                    {
                        if (x * x + y * y <= r * r)
                        {
                            var clearTile = new Vector3Int(tile.x + x, tile.y + y, tile.z + z);

                            if (mapRange.Contains(clearTile))
                            {
                                var i = Mathx.ToIndex(clearTile.x, clearTile.y, clearTile.z, width, height);
                                map[i] = kRoom;
                            }
                        }
                    }
                }
            }
        }
    }
}
