using System;
using Common;

namespace Custom.CaveGeneration
{
	public static class CaveMeshGenerator
	{
		[Serializable]
		public struct Input
		{
			public float squareSize;

			public static readonly Input Default = new Input
			{
				squareSize = 1.0f
			};
		}

		public static IMeshBuilder Generate(bool[,] cave, in Input input)
		{
			var meshBuilder = new FlatMeshBuilder();

			var width = cave.GetLength(0);
			var height = cave.GetLength(1);

			for (int y = 0; y < height - 1; y++)
			{
				for (int x = 0; x < width - 1; x++)
				{
					var active0 = cave[x, y];
					var active1 = cave[x, y + 1];
					var active2 = cave[x + 1, y + 1];
					var active3 = cave[x + 1, y];


				}
			}

			return meshBuilder;
		}

		private static int GetConfiguration(bool v0, bool v1, bool v2, bool v3)
		{
			var result = 0;
			if (v0) result += 8;
			if (v1) result += 4;
			if (v2) result += 2;
			if (v3) result += 1;
			return result;
		}

		/*
		 __ __ 5 __ __
		1             2
		|             |
		4             6
		|             |
		0__ __ 7 __ __3
		*/

		private static readonly int[,] TRIANGLES = new int[16, 13]
		{
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 0
			{  3,  7,  6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 1
			{  2,  6,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 2
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 3
			{  1,  5,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 4
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 5
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 6
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 7
			{  0,  4,  7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 8
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 9
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 10
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 11
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 12
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 13
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 14
			{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }  // 15
		};
	}
}