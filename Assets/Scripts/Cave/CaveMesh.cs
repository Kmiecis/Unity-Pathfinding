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

        private Mesh _mesh;

        public void SetMap(bool[] map, int width, int height)
        {
            _mesh = GetSharedMesh();
            if (_mesh != null && map != null)
            {
                var builder = GenerateMeshBuilder(map, width, height, _wallHeight);
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

        private static MeshBuilder GenerateMeshBuilder(bool[] map, int width, int height, float wallHeight)
        {
            var builder = new FlatMeshBuilder() { Options = EMeshBuildingOptions.NONE };

            var wallOffset = new Vector3(0.0f, 0.0f, wallHeight);

            var vs = MarchingSquares.Vertices;

            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    var v = new Vector3(x, y);

                    var c = MarchingSquares.GetConfiguration(
                        !map[Mathx.ToIndex(x, y, width)],
                        !map[Mathx.ToIndex(x, y + 1, width)],
                        !map[Mathx.ToIndex(x + 1, y + 1, width)],
                        !map[Mathx.ToIndex(x + 1, y, width)]
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

                    if (c > 0)
                    {
                        var wt0 = ts[i - 1];
                        var wt1 = ts[i - 2];

                        var wv0 = (Vector3)vs[wt0] + v - wallOffset;
                        var wv1 = (Vector3)vs[wt1] + v - wallOffset;
                        var wv2 = wv1 + wallOffset;
                        var wv3 = wv0 + wallOffset;

                        builder.AddTriangle(wv0, wv1, wv2);
                        builder.AddTriangle(wv0, wv2, wv3);

                        // Special cases of two walls
                        if (c == 5 || c == 10)
                        {
                            wt0 = ts[1];
                            wt1 = ts[2];

                            wv0 = (Vector3)vs[wt0] + v - wallOffset;
                            wv1 = (Vector3)vs[wt1] + v - wallOffset;
                            wv2 = wv1 + wallOffset;
                            wv3 = wv0 + wallOffset;

                            builder.AddTriangle(wv2, wv1, wv0);
                            builder.AddTriangle(wv2, wv0, wv3);
                        }
                    }
                }
            }

            return builder;
        }
        
        private void OnDestroy()
        {
            _mesh?.Destroy();
        }
    }
}
