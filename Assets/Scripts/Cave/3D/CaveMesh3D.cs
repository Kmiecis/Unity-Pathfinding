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

        private bool[][][] _map;
        private Mesh _mesh;

        public bool[][][] Map
        {
            set
            {
                _map = value;
                CheckMesh(_map);
                RegenerateMesh(_map, _mesh);
            }
        }

        private void CheckMesh(bool[][][] map)
        {
            if (map == null)
            {
                _mesh?.Destroy();
                _mesh = null;
                ApplyMeshToFilter(null);
            }
            else
            {
                if (_mesh == null)
                {
                    _mesh = new Mesh();
                    ApplyMeshToFilter(_mesh);
                }
            }
        }

        private void ApplyMeshToFilter(Mesh mesh)
        {
            if (_filter != null)
            {
                _filter.sharedMesh = mesh;
            }
        }

        private static void RegenerateMesh(bool[][][] map, Mesh mesh)
        {
            if (map != null && mesh != null)
            {
                var builder = GenerateMeshBuilder(map);
                builder.Overwrite(mesh);
            }
        }

        private static MeshBuilder GenerateMeshBuilder(bool[][][] map)
        {
            var result = new FlatMeshBuilder() { Options = EMeshBuildingOptions.NONE };

            var width = map.GetWidth();
            var height = map.GetHeight();
            var depth = map.GetDepth();

            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    for (int z = 0; z < depth - 1; z++)
                    {
                        var v = new Vector3(x, y, z);

                        var c = MarchingCubes.GetConfiguration(
                            !map[x][y][z], !map[x][y + 1][z], !map[x + 1][y + 1][z], !map[x + 1][y][z],
                            !map[x][y][z + 1], !map[x][y + 1][z + 1], !map[x + 1][y + 1][z + 1], !map[x + 1][y][z + 1]
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

            for (int y = 0; y < height - 1; y++)
            {
                for (int z = 0; z < depth - 1; z++)
                {
                    int x = 0;
                    {
                        var v = new Vector3(x, y, z);

                        var c = MarchingSquares.GetConfiguration(
                            !map[x][y][z], !map[x][y + 1][z], !map[x][y + 1][z + 1], !map[x][y][z + 1]
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
                            !map[x][y][z], !map[x][y + 1][z], !map[x][y + 1][z + 1], !map[x][y][z + 1]
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

            for (int x = 0; x < width - 1; x++)
            {
                for (int z = 0; z < depth - 1; z++)
                {
                    int y = 0;
                    {
                        var v = new Vector3(x, y, z);

                        var c = MarchingSquares.GetConfiguration(
                            !map[x][y][z], !map[x][y][z + 1], !map[x + 1][y][z + 1], !map[x + 1][y][z]
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
                            !map[x][y][z], !map[x][y][z + 1], !map[x + 1][y][z + 1], !map[x + 1][y][z]
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

            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    int z = 0;
                    {
                        var v = new Vector3(x, y, z);

                        var c = MarchingSquares.GetConfiguration(
                            !map[x][y][z], !map[x][y + 1][z], !map[x + 1][y + 1][z], !map[x + 1][y][z]
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
                            !map[x][y][z], !map[x][y + 1][z], !map[x + 1][y + 1][z], !map[x + 1][y][z]
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
