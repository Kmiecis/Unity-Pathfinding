using Common;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
	public class NewMapGenerator : MonoBehaviour
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

		[Header("Map generation")]
		public int width = 64;
		public int height = 64;
		public string seed = "";
		[Range(0.45f, 0.55f)] public float fill;

		[Header("Map processing")]
		public int wallThreshold = 5;
		public int roomThreshold = 5;

		private bool[,] m_Map;

		private void Start()
		{
			m_Map = Noisex.RandomMap(width, height, fill, 1, seed.GetHashCode());

			var wallRegions = GetRegionsByType(m_Map, WALL);
			var removedWallRegions = RemoveRegionsUnderThreshold(wallRegions, wallThreshold);
			FlipRegions(removedWallRegions, m_Map);

			var roomRegions = GetRegionsByType(m_Map, ROOM);
			var removedRoomRegions = RemoveRegionsUnderThreshold(roomRegions, roomThreshold);
			FlipRegions(removedRoomRegions, m_Map);

			// TODO
		}

		private List<List<Vector2Int>> GetRegionsByType(bool[,] target, bool type)
		{
			var result = new List<List<Vector2Int>>();

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

		private List<List<Vector2Int>> RemoveRegionsUnderThreshold(List<List<Vector2Int>> regions, int threshold)
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

		private void FlipRegions(List<List<Vector2Int>> regions, bool[,] target)
		{
			foreach (var region in regions)
			{
				foreach (var tile in region)
				{
					target[tile.x, tile.y] = !target[tile.x, tile.y];
				}
			}
		}
	}
}