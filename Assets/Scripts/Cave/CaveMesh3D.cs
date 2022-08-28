using Common;
using Common.Extensions;
using Common.Mathematics;
using UnityEngine;

namespace Custom.CaveGeneration
{
    public class CaveMesh3D : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        protected MeshFilter _filter;

        private Mesh _mesh;

        public void SetMap(bool[] map, int width, int height, int depth)
        {
            _mesh = GetSharedMesh();
            if (_mesh != null && map != null)
            {
                var builder = GenerateMeshBuilder(map, width, height, depth);
                builder.Overwrite(_mesh);
            }
        }

        private Mesh GetSharedMesh()
        {
            if (_filter != null)
            {
                var mesh = _filter.sharedMesh;
                if (mesh == null)
                {
                    _filter.sharedMesh = mesh = new Mesh();
                }
                return mesh;
            }
            return null;
        }

        private static MeshBuilder GenerateMeshBuilder(bool[] map, int width, int height, int depth)
        {
            var result = new FlatMeshBuilder() { Options = EMeshBuildingOptions.NONE };

            for (int z = 0; z < depth - 1; z++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    for (int x = 0; x < width - 1; x++)
                    {
                        var v = new Vector3(x, y, z);

                        var c = MarchingCubes.GetConfiguration(
                            !map[Mathx.ToIndex(x, y, z, width, height)],
                            !map[Mathx.ToIndex(x, y + 1, z, width, height)],
                            !map[Mathx.ToIndex(x + 1, y + 1, z, width, height)],
                            !map[Mathx.ToIndex(x + 1, y, z, width, height)],
                            !map[Mathx.ToIndex(x, y, z + 1, width, height)],
                            !map[Mathx.ToIndex(x, y + 1, z + 1, width, height)],
                            !map[Mathx.ToIndex(x + 1, y + 1, z + 1, width, height)],
                            !map[Mathx.ToIndex(x + 1, y, z + 1, width, height)]
                        );

                        var ts = MarchingCubes.Triangles[c];
                        for (int t = 0; t < ts.Length; t += 3)
                        {
                            var t0 = ts[t + 0];
                            var t1 = ts[t + 1];
                            var t2 = ts[t + 2];

                            var v0 = v + MarchingCubes.Vertices[t0];
                            var v1 = v + MarchingCubes.Vertices[t1];
                            var v2 = v + MarchingCubes.Vertices[t2];

                            result.AddTriangle(v0, v1, v2);
                        }
                    }
                }
            }

            for (int z = 0; z < depth - 1; z++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    int x = 0;
                    {
                        var v = new Vector3(x, y, z);

                        var c = MarchingSquares.GetConfiguration(
                            !map[Mathx.ToIndex(x, y, z, width, height)],
                            !map[Mathx.ToIndex(x, y + 1, z, width, height)],
                            !map[Mathx.ToIndex(x, y + 1, z + 1, width, height)],
                            !map[Mathx.ToIndex(x, y, z + 1, width, height)]
                        );

                        var ts = MarchingSquares.Triangles[c];
                        for (int t = 0; t < ts.Length; t += 3)
                        {
                            var t0 = ts[t + 0];
                            var t1 = ts[t + 1];
                            var t2 = ts[t + 2];

                            var v0 = v + MarchingSquares.Vertices[t0]._YX();
                            var v1 = v + MarchingSquares.Vertices[t1]._YX();
                            var v2 = v + MarchingSquares.Vertices[t2]._YX();

                            result.AddTriangle(v2, v1, v0);
                        }
                    }

                    x = width - 1;
                    {
                        var v = new Vector3(x, y, z);

                        var c = MarchingSquares.GetConfiguration(
                            !map[Mathx.ToIndex(x, y, z, width, height)],
                            !map[Mathx.ToIndex(x, y + 1, z, width, height)],
                            !map[Mathx.ToIndex(x, y + 1, z + 1, width, height)],
                            !map[Mathx.ToIndex(x, y, z + 1, width, height)]
                        );

                        var ts = MarchingSquares.Triangles[c];
                        for (int t = 0; t < ts.Length; t += 3)
                        {
                            var t0 = ts[t + 0];
                            var t1 = ts[t + 1];
                            var t2 = ts[t + 2];

                            var v0 = v + MarchingSquares.Vertices[t0]._YX();
                            var v1 = v + MarchingSquares.Vertices[t1]._YX();
                            var v2 = v + MarchingSquares.Vertices[t2]._YX();

                            result.AddTriangle(v0, v1, v2);
                        }
                    }
                }
            }

            for (int z = 0; z < depth - 1; z++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int y = 0;
                    {
                        var v = new Vector3(x, y, z);

                        var c = MarchingSquares.GetConfiguration(
                            !map[Mathx.ToIndex(x, y, z, width, height)],
                            !map[Mathx.ToIndex(x, y, z + 1, width, height)],
                            !map[Mathx.ToIndex(x + 1, y, z + 1, width, height)],
                            !map[Mathx.ToIndex(x + 1, y, z, width, height)]
                        );

                        var ts = MarchingSquares.Triangles[c];
                        for (int t = 0; t < ts.Length; t += 3)
                        {
                            var t0 = ts[t + 0];
                            var t1 = ts[t + 1];
                            var t2 = ts[t + 2];

                            var v0 = v + MarchingSquares.Vertices[t0].X_Y();
                            var v1 = v + MarchingSquares.Vertices[t1].X_Y();
                            var v2 = v + MarchingSquares.Vertices[t2].X_Y();

                            result.AddTriangle(v2, v1, v0);
                        }
                    }

                    y = height - 1;
                    {
                        var v = new Vector3(x, y, z);

                        var c = MarchingSquares.GetConfiguration(
                            !map[Mathx.ToIndex(x, y, z, width, height)],
                            !map[Mathx.ToIndex(x, y, z + 1, width, height)],
                            !map[Mathx.ToIndex(x + 1, y, z + 1, width, height)],
                            !map[Mathx.ToIndex(x + 1, y, z, width, height)]
                        );

                        var ts = MarchingSquares.Triangles[c];
                        for (int t = 0; t < ts.Length; t += 3)
                        {
                            var t0 = ts[t + 0];
                            var t1 = ts[t + 1];
                            var t2 = ts[t + 2];

                            var v0 = v + MarchingSquares.Vertices[t0].X_Y();
                            var v1 = v + MarchingSquares.Vertices[t1].X_Y();
                            var v2 = v + MarchingSquares.Vertices[t2].X_Y();

                            result.AddTriangle(v0, v1, v2);
                        }
                    }
                }
            }

            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int z = 0;
                    {
                        var v = new Vector3(x, y, z);

                        var c = MarchingSquares.GetConfiguration(
                            !map[Mathx.ToIndex(x, y, z, width, height)],
                            !map[Mathx.ToIndex(x, y + 1, z, width, height)],
                            !map[Mathx.ToIndex(x + 1, y + 1, z, width, height)],
                            !map[Mathx.ToIndex(x + 1, y, z, width, height)]
                        );

                        var ts = MarchingSquares.Triangles[c];
                        for (int t = 0; t < ts.Length; t += 3)
                        {
                            var t0 = ts[t + 0];
                            var t1 = ts[t + 1];
                            var t2 = ts[t + 2];

                            var v0 = v + MarchingSquares.Vertices[t0].XY_();
                            var v1 = v + MarchingSquares.Vertices[t1].XY_();
                            var v2 = v + MarchingSquares.Vertices[t2].XY_();

                            result.AddTriangle(v0, v1, v2);
                        }
                    }

                    z = depth - 1;
                    {
                        var v = new Vector3(x, y, z);

                        var c = MarchingSquares.GetConfiguration(
                            !map[Mathx.ToIndex(x, y, z, width, height)],
                            !map[Mathx.ToIndex(x, y + 1, z, width, height)],
                            !map[Mathx.ToIndex(x + 1, y + 1, z, width, height)],
                            !map[Mathx.ToIndex(x + 1, y, z, width, height)]
                        );

                        var ts = MarchingSquares.Triangles[c];
                        for (int t = 0; t < ts.Length; t += 3)
                        {
                            var t0 = ts[t + 0];
                            var t1 = ts[t + 1];
                            var t2 = ts[t + 2];

                            var v0 = v + MarchingSquares.Vertices[t0].XY_();
                            var v1 = v + MarchingSquares.Vertices[t1].XY_();
                            var v2 = v + MarchingSquares.Vertices[t2].XY_();

                            result.AddTriangle(v2, v1, v0);
                        }
                    }
                }
            }

            return result;
        }
    }
}
