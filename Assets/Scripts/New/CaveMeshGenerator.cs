using System;
using Common;
using UnityEngine;

namespace Custom.CaveGeneration
{
	public static class CaveMeshGenerator
	{
		[Serializable]
		public struct Input
		{
			public float squareSize;
			public float wallHeight;

			public static readonly Input Default = new Input
			{
				squareSize = 1.0f,
				wallHeight = 1.0f
			};
		}

		public static IMeshBuilder Generate(bool[,] cave, in Input input)
		{
			var meshBuilder = new FlatMeshBuilder();

			var width = cave.GetLength(0);
			var height = cave.GetLength(1);

			var wallOffset = new Vector3(0.0f, 0.0f, input.wallHeight);

			var vertices = MarchingSquares.Vertices;

			for (int y = 0; y < height - 1; y++)
			{
				for (int x = 0; x < width - 1; x++)
				{
					var active0 = cave[x, y];
					var active1 = cave[x, y + 1];
					var active2 = cave[x + 1, y + 1];
					var active3 = cave[x + 1, y];

					var configuration = MarchingSquares.GetConfiguration(active0, active1, active2, active3);
					var triangles = MarchingSquares.Triangles[configuration];

					var offset = new Vector3(x, y) * input.squareSize - wallOffset;

					int i = 0;
					for (; i < triangles.Length && triangles[i] != -1; i += 3)
					{
						var t0 = triangles[i + 0];
						var t1 = triangles[i + 1];
						var t2 = triangles[i + 2];

						var v0 = (Vector3)vertices[t0] * input.squareSize + offset;
						var v1 = (Vector3)vertices[t1] * input.squareSize + offset;
						var v2 = (Vector3)vertices[t2] * input.squareSize + offset;

						meshBuilder.AddTriangle(v0, v1, v2);
					}
					
					if (configuration > 0 && configuration < MarchingSquares.Triangles.Length - 1)
					{
						var wt0 = triangles[i - 1];
						var wt1 = triangles[i - 2];

						var wv0 = (Vector3)vertices[wt0] * input.squareSize + offset;
						var wv1 = (Vector3)vertices[wt1] * input.squareSize + offset;
						var wv2 = wv1 + wallOffset;
						var wv3 = wv0 + wallOffset;

						meshBuilder.AddTriangle(wv0, wv1, wv2);
						meshBuilder.AddTriangle(wv0, wv2, wv3);
					}
				}
			}

			return meshBuilder;
		}
	}
}