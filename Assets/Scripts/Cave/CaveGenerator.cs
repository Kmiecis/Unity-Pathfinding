using Common.Mathematics;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Custom.CaveGeneration
{
    public static class CaveGenerator
    {
        private const bool kRoom = true;
        private const bool kWall = false;

        private struct Line
        {
            public Vector2Int a;
            public Vector2Int b;
        }

        [Serializable]
        public struct Input
        {
            [Header("Map generation")]
            public int width;
            public int height;
            public int smooths;
            [Range(0.45f, 0.55f)]
            public float fill;
            public string seed;

            [Header("Map processing")]
            public int wallSizeThreshold;
            public int roomSizeThreshold;
            public int passageWidth;
            public int borderWidth;

            public static readonly Input Default = new Input
            {
                width = 64,
                height = 64,
                smooths = 4,
                fill = 0.5f,
                seed = "",
                wallSizeThreshold = 5,
                roomSizeThreshold = 5,
                passageWidth = 2,
                borderWidth = 2
            };
        }

        public static void Generate(bool[] map, in Input input)
        {
            var random = new Random(input.seed.GetHashCode());
            Noisex.GetRandomMap(map, input.width, input.height, input.fill, random);
            ApplyBorder(map, input.width, input.height, input.borderWidth);
            Noisex.SmoothRandomMap(map, input.width, input.height, input.smooths);

            var roomRegions = GetRegionsByType(map, input.width, input.height, kRoom);
            var removedRoomRegions = RemoveRegionsUnderThreshold(roomRegions, input.roomSizeThreshold);
            FlipRegions(removedRoomRegions, map, input.width);

            var wallRegions = GetRegionsByType(map, input.width, input.height, kWall);
            var removedWallRegions = RemoveRegionsUnderThreshold(wallRegions, input.wallSizeThreshold);
            FlipRegions(removedWallRegions, map, input.width);

            var rooms = CreateRooms(roomRegions, map, input.width, input.height);
            var passages = FindPassages(rooms);
            ClearPassages(passages, map, input.width, input.height, input.passageWidth, input.borderWidth);
        }

        private static void ApplyBorder(bool[] map, int width, int height, int border)
        {
            if (border > 0)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < border; x++)
                    {
                        int i = Mathx.ToIndex(x, y, width);
                        map[i] = kWall;
                    }

                    for (int x = width - border; x < width; x++)
                    {
                        int i = Mathx.ToIndex(x, y, width);
                        map[i] = kWall;
                    }
                }

                for (int x = border; x < width - border; x++)
                {
                    for (int y = 0; y < border; y++)
                    {
                        int i = Mathx.ToIndex(x, y, width);
                        map[i] = kWall;
                    }

                    for (int y = height - border; y < height; y++)
                    {
                        int i = Mathx.ToIndex(x, y, width);
                        map[i] = kWall;
                    }
                }
            }
        }

        private static List<List<Vector2Int>> GetRegionsByType(bool[] map, int width, int height, bool type)
        {
            var result = new List<List<Vector2Int>>();

            var isChecked = new bool[width * height];
            var mapRange = new Range2Int(0, 0, width - 1, height - 1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Mathx.ToIndex(x, y, width);
                    if (map[i] == type && !isChecked[i])
                    {
                        var region = new List<Vector2Int>();
                        var toCheck = new Queue<Vector2Int>();

                        toCheck.Enqueue(new Vector2Int(x, y));
                        isChecked[i] = true;

                        while (toCheck.Count > 0)
                        {
                            var current = toCheck.Dequeue();
                            region.Add(current);

                            foreach (var direction in Axes.All2D)
                            {
                                var neighbour = current + direction;
                                var ni = Mathx.ToIndex(neighbour.x, neighbour.y, width);

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

            return result;
        }

        private static List<List<Vector2Int>> RemoveRegionsUnderThreshold(List<List<Vector2Int>> regions, int threshold)
        {
            var removed = new List<List<Vector2Int>>();

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

        private static void FlipRegions(List<List<Vector2Int>> regions, bool[] map, int width)
        {
            foreach (var region in regions)
            {
                foreach (var tile in region)
                {
                    int i = Mathx.ToIndex(tile.x, tile.y, width);
                    map[i] = !map[i];
                }
            }
        }

        private static List<List<Vector2Int>> CreateRooms(List<List<Vector2Int>> regions, bool[] map, int width, int height)
        {
            var result = new List<List<Vector2Int>>();

            var isChecked = new bool[width * height];
            var mapRange = new Range2Int(0, 0, width - 1, height - 1);

            foreach (var region in regions)
            {
                var room = new List<Vector2Int>();

                foreach (var tile in region)
                {
                    foreach (var direction in Axes.All2D)
                    {
                        var neighbour = tile + direction;
                        int ni = Mathx.ToIndex(neighbour.x, neighbour.y, width);

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
        private static List<Line> FindPassages(List<List<Vector2Int>> rooms)
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

                            var currentDistance = dx * dx + dy * dy;

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

        private static List<Vector2Int> GetTilesOnLine(Vector2Int a, Vector2Int b)
        {
            var result = new List<Vector2Int>();

            int x = a.x;
            int y = a.y;

            int dx = b.x - a.x;
            int dy = b.y - a.y;

            int abs_dx = Math.Abs(dx);
            int abs_dy = Math.Abs(dy);

            int min = Math.Min(abs_dx, abs_dy);
            int max = Math.Max(abs_dx, abs_dy);

            var step = Vector2Int.zero;
            var acc_step = Vector2Int.zero;

            if (abs_dy > abs_dx)
            {
                step.y = Math.Sign(dy);
                acc_step.x = Math.Sign(dx);
            }
            else
            {
                step.x = Math.Sign(dx);
                acc_step.y = Math.Sign(dy);
            }

            int acc = max / 2;

            for (int i = 0; i < max; i++)
            {
                result.Add(new Vector2Int(x, y));

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
            result.Add(b);

            return result;
        }

        private static void ClearPassages(List<Line> passages, bool[] map, int width, int height, int passageWidth, int borderWidth)
        {
            foreach (var passage in passages)
            {
                var tiles = GetTilesOnLine(passage.a, passage.b);

                foreach (var tile in tiles)
                {
                    ClearCircle(tile, passageWidth, map, width, height, borderWidth);
                }
            }
        }

        private static void ClearCircle(Vector2Int tile, int r, bool[] map, int width, int height, int borderWidth)
        {
            var mapRange = new Range2Int(borderWidth, borderWidth, width - borderWidth - 1, height - borderWidth - 1);

            for (int y = -r; y <= r; y++)
            {
                for (int x = -r; x <= r; x++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        var clearTile = new Vector2Int(tile.x + x, tile.y + y);

                        if (mapRange.Contains(clearTile))
                        {
                            var i = Mathx.ToIndex(clearTile.x, clearTile.y, width);
                            map[i] = kRoom;
                        }
                    }
                }
            }
        }
    }
}
