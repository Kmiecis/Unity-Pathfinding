using Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
	public class CaveGenerator : MonoBehaviour
	{
		private const bool ROOM = false;
		private const bool WALL = true;

		class Region
		{
			public List<Vector2Int> tiles = new List<Vector2Int>();
		}

		class Room
		{
			public List<Vector2Int> tiles = new List<Vector2Int>();
			public List<Vector2Int> edge = new List<Vector2Int>();
		}

		struct Line
		{
			public Vector2Int a;
			public Vector2Int b;
		}

		[Header("Map generation")]
		public int width = 64;
		public int height = 64;
		public string seed = "";
		[Range(0.45f, 0.55f)] public float fill;

		[Header("Map processing")]
		public int wallThreshold = 5;
		public int roomThreshold = 5;
		public int passageWidth = 3;
		public int borderWidth = 2;
		
		private void Start()
		{
			var map = Noisex.RandomMap(width, height, fill, 1, seed.GetHashCode());

			var wallRegions = GetRegionsByType(map, WALL);
			var removedWallRegions = RemoveRegionsUnderThreshold(wallRegions, wallThreshold);
			FlipRegions(removedWallRegions, map);

			var roomRegions = GetRegionsByType(map, ROOM);
			var removedRoomRegions = RemoveRegionsUnderThreshold(roomRegions, roomThreshold);
			FlipRegions(removedRoomRegions, map);

			var rooms = CreateRooms(roomRegions, map);
			var passages = FindPassages(rooms);
		}

		private List<Region> GetRegionsByType(bool[,] target, bool type)
		{
			var result = new List<Region>();

			var width = target.GetLength(0);
			var height = target.GetLength(1);

			var isChecked = new bool[width, height];
			var targetRange = new Range2Int(0, 0, width - 1, height - 1);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (
						!isChecked[x, y] &&
						target[x, y] == type
					)
					{
						var region = new Region();

						var toCheck = new Queue<Vector2Int>();
						toCheck.Enqueue(new Vector2Int(x, y));

						while (toCheck.Count > 0)
						{
							var current = toCheck.Dequeue();
							region.tiles.Add(current);

							foreach (var direction in CartesianUtility.Directions2D)
							{
								var neighbour = current + direction;

								if (
									targetRange.Contains(neighbour) &&
									target[neighbour.x, neighbour.y] == type &&
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

		private List<Region> RemoveRegionsUnderThreshold(List<Region> regions, int threshold)
		{
			var removed = new List<Region>();

			for (int i = regions.Count - 1; i > -1; i--)
			{
				var region = regions[i];

				if (region.tiles.Count < threshold)
				{
					regions.RemoveAt(i);
					removed.Add(region);
				}
			}

			return removed;
		}

		private void FlipRegions(List<Region> regions, bool[,] target)
		{
			foreach (var region in regions)
			{
				foreach (var tile in region.tiles)
				{
					target[tile.x, tile.y] = !target[tile.x, tile.y];
				}
			}
		}

		private List<Room> CreateRooms(List<Region> regions, bool[,] target)
		{
			var result = new List<Room>();

			var width = target.GetLength(0);
			var height = target.GetLength(1);
			var targetRange = new Range2Int(0, 0, width - 1, height - 1);

			foreach (var region in regions)
			{
				var room = new Room();
				room.tiles = region.tiles;

				foreach (var tile in room.tiles)
				{
					foreach (var direction in CartesianUtility.Directions2D)
					{
						var neighbour = tile + direction;
						if (
							targetRange.Contains(neighbour) &&
							target[neighbour.x, neighbour.y] == WALL
						)
						{
							room.edge.Add(neighbour);
						}
					}
				}
			}

			return result;
		}

		List<Line> FindPassages(List<Room> rooms)
		{
			var result = new List<Line>();

			for (int i = 0; i < rooms.Count; i++)
			{
				var roomA = rooms[i];
				var line = new Line();

				var distance = int.MaxValue;

				for (int j = i + 1; j < rooms.Count; j++)
				{
					var roomB = rooms[j];

					for (int ta = 0; ta < roomA.edge.Count; ta++)
					{
						var tileA = roomA.edge[ta];

						for (int tb = 0; tb < roomB.edge.Count; tb++)
						{
							var tileB = roomB.edge[tb];

							var dx = tileB.x - tileA.x;
							var dy = tileB.y - tileA.y;

							var currentDistance = dx * dx + dy * dy;

							if (distance < currentDistance)
							{
								line.a = tileA;
								line.b = tileB;
							}
						}
					}
				}

				result.Add(line);
			}

			return result;
		}
	}
}