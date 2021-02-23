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

			var vertices = SquareUtility.Vertices;
			var edgeVertices = MarchingSquares.EdgeVertices;

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

					var offset = new Vector2(x, y);
					
					for (int i = 0; i < triangles.Length && triangles[i] != -1; i += 3)
					{
						var t0 = triangles[i + 0];
						var t1 = triangles[i + 1];
						var t2 = triangles[i + 2];

						var v0 = t0 < vertices.Length ? vertices[t0] : edgeVertices[t0 - vertices.Length];
						var v1 = t1 < vertices.Length ? vertices[t1] : edgeVertices[t1 - vertices.Length];
						var v2 = t2 < vertices.Length ? vertices[t2] : edgeVertices[t2 - vertices.Length];

						meshBuilder.AddTriangle(v0 + offset, v1 + offset, v2 + offset);
					}
				}
			}

			return meshBuilder;
		}
	}
}