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

            var vs = MarchingSquares.Vertices;

            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    var v = new Vector3(x, y);

                    var c = MarchingSquares.GetConfiguration(
                        !map[x][y], !map[x][y + 1], !map[x + 1][y + 1], !map[x + 1][y]
                    );

                    int i = 0;
                    var ts = MarchingSquares.Triangles[c];
                    for (; i < ts.Length; i += 3)
                    {
                        var t0 = ts[i + 0];
                        var t1 = ts[i + 1];
                        var t2 = ts[i + 2];

                        var v0 = (Vector3)vs[t0] + v - wallOffset;
                        var v1 = (Vector3)vs[t1] + v - wallOffset;
                        var v2 = (Vector3)vs[t2] + v - wallOffset;

                        builder.AddTriangle(v0, v1, v2);
                    }

                    if (c > 0 && c < MarchingSquares.Triangles.Length - 1)
                    {
                        var wt0 = ts[i - 1];
                        var wt1 = ts[i - 2];

                        var wv0 = (Vector3)vs[wt0] + v - wallOffset;
                        var wv1 = (Vector3)vs[wt1] + v - wallOffset;
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
