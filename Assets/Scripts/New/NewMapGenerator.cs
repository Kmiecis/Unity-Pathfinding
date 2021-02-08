using Common;
using UnityEngine;

namespace Pathfinding
{
	public class NewMapGenerator : MonoBehaviour
	{
		public int width;
		public int height;
		public string seed;
		[Range(0.45f, 0.55f)] public float fill;

		private bool[,] m_Map;

		private void Start()
		{
			m_Map = Noisex.RandomMap(width, height, fill, 1, seed.GetHashCode());
		}
	}
}