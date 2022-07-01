using Common;
using Common.Extensions;
using Common.Mathematics;
using UnityEngine;

namespace Custom.CaveGeneration
{
    public class CaveMesh : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        protected MeshFilter _filter;
        [Header("Input")]
        [SerializeField]
        protected float _wallHeight = 1.0f;

        private bool[][] _map;
        private Mesh _mesh;

        public bool[][] Map
        {
            set
            {
                _map = value;
                CheckMesh(value);
                RegenerateMesh(value);
            }
        }

        private void CheckMesh(bool[][] map)
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
        
        private void RegenerateMesh(bool[][] map)
        {
            if (map != null && _mesh != null)
            {
                var builder = GenerateMeshBuilder(map, _wallHeight);
                builder.Overwrite(_mesh);
            }
        }

        private static MeshBuilder GenerateMeshBuilder(bool[][] map, float wallHeight)
        {
            var builder = new FlatMeshBuilder() { Options = EMeshBuildingOptions.NONE };

            var width = map.GetWidth();
            var height = map.GetHeight();

            var wallOffset = new Vector3(0.0f, 0.0f, wallHeight);

            var vertices = MarchingSquares.Vertices;

            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    var active0 = !map[x][y];
                    var active1 = !map[x][y + 1];
                    var active2 = !map[x + 1][y + 1];
                    var active3 = !map[x + 1][y];

                    var configuration = MarchingSquares.GetConfiguration(active0, active1, active2, active3);
                    var triangles = MarchingSquares.Triangles[configuration];

                    var offset = new Vector3(x, y) - wallOffset;

                    int i = 0;
                    for (; i < triangles.Length; i += 3)
                    {
                        var t0 = triangles[i + 0];
                        var t1 = triangles[i + 1];
                        var t2 = triangles[i + 2];

                        var v0 = (Vector3)vertices[t0] + offset;
                        var v1 = (Vector3)vertices[t1] + offset;
                        var v2 = (Vector3)vertices[t2] + offset;

                        builder.AddTriangle(v0, v1, v2);
                    }

                    if (configuration > 0 && configuration < MarchingSquares.Triangles.Length - 1)
                    {
                        var wt0 = triangles[i - 1];
                        var wt1 = triangles[i - 2];

                        var wv0 = (Vector3)vertices[wt0] + offset;
                        var wv1 = (Vector3)vertices[wt1] + offset;
                        var wv2 = wv1 + wallOffset;
                        var wv3 = wv0 + wallOffset;

                        builder.AddTriangle(wv0, wv1, wv2);
                        builder.AddTriangle(wv0, wv2, wv3);
                    }
                }
            }

            return builder;
        }
        
        private void OnDestroy()
        {
            _mesh?.Destroy();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RegenerateMesh(_map);
        }
#endif
    }
}
