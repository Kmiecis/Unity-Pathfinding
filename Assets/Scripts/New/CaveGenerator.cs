using Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
	public class CaveGenerator
	{
		public const bool ROOM = false;
		public const bool WALL = true;
		
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
			public string seed;
			[Range(0.45f, 0.55f)] public float fill;

			[Header("Map processing")]
			public int wallThreshold;
			public int roomThreshold;
			public int passageWidth;
			public int borderWidth;

			public static readonly Input Default = new Input
			{
				width = 64,
				height = 64,
				smooths = 4,
				seed = "",
				fill = 0.5f,
				wallThreshold = 5,
				roomThreshold = 5,
				passageWidth = 2,
				borderWidth = 2
			};
		}

		public static bool[,] Generate(in Input input)
		{
			var map = Noisex.GenerateRandomMap(input.width, input.height, input.fill, input.seed.GetHashCode());
			ApplyBorder(map, input.borderWidth);
			Noisex.SmoothRandomMap(map, input.smooths);

			var wallRegions = GetRegionsByType(map, WALL);
			var removedWallRegions = RemoveRegionsUnderThreshold(wallRegions, input.wallThreshold);
			FlipRegions(removedWallRegions, map);

			var roomRegions = GetRegionsByType(map, ROOM);
			var removedRoomRegions = RemoveRegionsUnderThreshold(roomRegions, input.roomThreshold);
			FlipRegions(removedRoomRegions, map);

			var rooms = CreateRooms(roomRegions, map);
			var passages = FindPassages(rooms);
			ClearPassages(passages, map, input.passageWidth, input.borderWidth);

			return map;
		}
		
		private static void ApplyBorder(bool[,] map, int border)
		{
			var width = map.GetLength(0);
			var height = map.GetLength(1);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < border; x++)
				{
					map[x, y] = WALL;
				}

				for (int x = width - border - 1; x < width; x++)
				{
					map[x, y] = WALL;
				}
			}

			for (int x = border; x < width - border; x++)
			{
				for (int y = 0; y < border; y++)
				{
					map[x, y] = WALL;
				}

				for (int y = height - border - 1; y < height; y++)
				{
					map[x, y] = WALL;
				}
			}
		}

		private static List<List<Vector2Int>> GetRegionsByType(bool[,] map, bool type)
		{
			var result = new List<List<Vector2Int>>();

			var width = map.GetLength(0);
			var height = map.GetLength(1);

			var isChecked = new bool[width, height];
			var mapRange = new Range2Int(0, 0, width - 1, height - 1);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (
						!isChecked[x, y] &&
						map[x, y] == type
					)
					{
						var region = new List<Vector2Int>();

						var toCheck = new Queue<Vector2Int>();
						toCheck.Enqueue(new Vector2Int(x, y));

						while (toCheck.Count > 0)
						{
							var current = toCheck.Dequeue();
							region.Add(current);

							foreach (var direction in CartesianUtility.Directions2D)
							{
								var neighbour = current + direction;

								if (
									mapRange.Contains(neighbour) &&
									map[neighbour.x, neighbour.y] == type &&
									!isChecked[neighbour.x, neighbour.y]
								)
								{
									toCheck.Enqueue(neighbour);
									isChecked[neighbour.x, neighbour.y] = true;
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

		private static void FlipRegions(List<List<Vector2Int>> regions, bool[,] map)
		{
			foreach (var region in regions)
			{
				foreach (var tile in region)
				{
					map[tile.x, tile.y] = !map[tile.x, tile.y];
				}
			}
		}

		private static List<List<Vector2Int>> CreateRooms(List<List<Vector2Int>> regions, bool[,] map)
		{
			var result = new List<List<Vector2Int>>();

			var width = map.GetLength(0);
			var height = map.GetLength(1);
			var mapRange = new Range2Int(0, 0, width - 1, height - 1);

			foreach (var region in regions)
			{
				var room = new List<Vector2Int>();

				foreach (var tile in region)
				{
					foreach (var direction in CartesianUtility.Directions2D)
					{
						var neighbour = tile + direction;
						if (
							mapRange.Contains(neighbour) &&
							map[neighbour.x, neighbour.y] == WALL
						)
						{
							room.Add(neighbour);
						}
					}
				}

				result.Add(room);
			}

			return result;
		}

		private static List<Line> FindPassages(List<List<Vector2Int>> rooms)
		{
			var result = new List<Line>();

			while (rooms.Count > 1)
			{
				var roomA = rooms[0];

				var line = new Line();
				var distance = int.MaxValue;
				var index = -1;

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

				result.Add(line);
				rooms.RemoveAt(index);
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

			return result;
		}

		private static void ClearPassages(List<Line> passages, bool[,] map, int passageWidth, int borderWidth)
		{
			foreach (var passage in passages)
			{
				var tiles = GetTilesOnLine(passage.a, passage.b);

				foreach (var tile in tiles)
				{
					ClearCircle(tile, passageWidth, map, borderWidth);
				}
			}
		}

		private static void ClearCircle(Vector2Int tile, int r, bool[,] map, int borderWidth)
		{
			var width = map.GetLength(0);
			var height = map.GetLength(1);
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
							map[clearTile.x, clearTile.y] = ROOM;
						}
					}
				}
			}
		}
	}
}