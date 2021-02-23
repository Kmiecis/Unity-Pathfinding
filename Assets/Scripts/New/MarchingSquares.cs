using Common;
using System;
using UnityEngine;

namespace Custom
{
	public class MarchingSquares
	{
		public const float SIDE_LENGTH = SquareUtility.SIDE_LENGTH;
		public const float CENTER_TO_SIDE = SquareUtility.CENTER_TO_SIDE;

		public static readonly Vector2[] EdgeVertices = new Vector2[]
		{
			new Vector2(-CENTER_TO_SIDE, 0.0f),
			new Vector2(0.0f, +CENTER_TO_SIDE),
			new Vector2(+CENTER_TO_SIDE, 0.0f),
			new Vector2(0.0f, -CENTER_TO_SIDE)
		};

		/*
		 __ __ 5 __ __
		1             2
		|  2       4  |
		4             6
		|  1       8  |
		0__ __ 7 __ __3
		*/

		public static readonly int[][] Triangles = new int[][]
		{
			new int[]{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 0
			new int[]{  0,  4,  7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 1
			new int[]{  1,  5,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 2
			new int[]{  0,  1,  5,  0,  5,  7, -1, -1, -1, -1, -1, -1, -1 }, // 3 = 1 + 2
			new int[]{  2,  6,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 4
			new int[]{  0,  4,  5,  0,  5,  2,  0,  2,  6,  0,  6,  7, -1 }, // 5 = 1 + 4
			new int[]{  1,  2,  6,  1,  6,  4, -1, -1, -1, -1, -1, -1, -1 }, // 6 = 2 + 4
			new int[]{  0,  1,  2,  0,  2,  6,  0,  6,  7, -1, -1, -1, -1 }, // 7 = 1 + 2 + 4
			new int[]{  3,  7,  6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, // 8
			new int[]{  0,  4,  6,  0,  6,  3, -1, -1, -1, -1, -1, -1, -1 }, // 9 = 1 + 8
			new int[]{  1,  5,  6,  1,  6,  3,  1,  3,  7,  1,  7,  4, -1 }, // 10 = 2 + 8
			new int[]{  3,  0,  1,  3,  1,  5,  3,  5,  6, -1, -1, -1, -1 }, // 11 = 1 + 2 + 8
			new int[]{  2,  3,  7,  2,  7,  5, -1, -1, -1, -1, -1, -1, -1 }, // 12 = 4 + 8
			new int[]{  2,  3,  0,  2,  0,  4,  2,  4,  5, -1, -1, -1, -1 }, // 13 = 1 + 4 + 8
			new int[]{  1,  2,  3,  1,  3,  7,  1,  7,  4, -1, -1, -1, -1 }, // 14 = 2 + 4 + 8
			new int[]{  0,  1,  2,  0,  2,  3, -1, -1, -1, -1, -1, -1, -1 }  // 15 = 1 + 2 + 4 + 8
		};

		public static int GetConfiguration(int active0, int active1, int active2, int active3)
		{
			return (
				active0 * 1 +
				active1 * 2 +
				active2 * 4 +
				active3 * 8
			);
		}

		public static int GetConfiguration(bool active0, bool active1, bool active2, bool active3)
		{
			return GetConfiguration(Convert.ToInt32(active0), Convert.ToInt32(active1), Convert.ToInt32(active2), Convert.ToInt32(active3));
		}
	}
}