using Common.Extensions;
using Common.Mathematics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.CaveGeneration
{
    public class CaveCollider : MonoBehaviour
    {
        private class VertexPair
        {
            public Vector2 v0;
            public Vector2 v1;
        }

        [Header("Components")]
        [SerializeField]
        protected PolygonCollider2D _collider;

        public void SetMap(bool[] map, int width, int height)
        {
            if (_collider != null && map != null)
            {
                var paths = GetColliderPaths(map, width, height);
                _collider.pathCount = paths.Count + 1;

                for (int i = 0; i < paths.Count; ++i)
                    _collider.SetPath(i, paths[i]);

                var border = GetBorderPath(map, width, height);
                _collider.SetPath(paths.Count, border);
            }
        }

        private static List<List<Vector2>> GetColliderPaths(bool[] map, int width, int height)
        {
            var result = new List<List<Vector2>>();

            var edges = new Dictionary<Vector2Int, VertexPair>();

            const float kScale = 10.0f;

            Vector2Int VertexToKey(Vector2 v)
            {
                return Mathx.RoundToInt(v * kScale);
            }

            Vector2 KeyToVertex(Vector2Int key)
            {
                return Mathx.Div(key, kScale);
            }

            void AddEdge(Vector2 v0, Vector2 v1)
            {
                var key = VertexToKey(v0);
                if (!edges.TryGetValue(key, out var pair))
                {
                    edges.Add(key, new VertexPair { v0 = v1 });
                }
                else
                {
                    pair.v1 = v1;
                }
            }

            Vector2 GetNextVertex(Vector2 v0, Vector2 v1)
            {
                var key = VertexToKey(v1);
                var pair = edges.Revoke(key);
                return Mathx.IsEqual(v0, pair.v0) ? pair.v1 : pair.v0;
            }

            // Find edge pairs
            var vertices = MarchingSquares.Vertices;

            for (int y = 0; y < height - 1; ++y)
            {
                for (int x = 0; x < width - 1; ++x)
                {
                    var c = MarchingSquares.GetConfiguration(
                        !map[Mathx.ToIndex(x, y, width)],
                        !map[Mathx.ToIndex(x, y + 1, width)],
                        !map[Mathx.ToIndex(x + 1, y + 1, width)],
                        !map[Mathx.ToIndex(x + 1, y, width)]
                    );

                    var offset = new Vector2(x, y);

                    var triangles = MarchingSquares.Triangles[c];
                    for (int t = 0; t < triangles.Length; t += 3)
                    {
                        for (int e0 = 2, e1 = 0; e1 < 3; e0 = e1, ++e1)
                        {
                            var t0 = triangles[e0 + t];
                            var t1 = triangles[e1 + t];

                            if (t0 > 3 && t1 > 3) // 4 || 5 || 6 || 7
                            {
                                var v0 = vertices[t0] + offset;
                                var v1 = vertices[t1] + offset;

                                AddEdge(v0, v1);
                                AddEdge(v1, v0);
                            }
                        }
                    }
                }
            }
            
            while (edges.Count > 0)
            {
                var firstKey = edges.First().Key;
                var v0 = KeyToVertex(firstKey);
                var v1 = GetNextVertex(v0, v0);

                var path = new List<Vector2>();

                // Build path
                path.Add(v0);
                path.Add(v1);

                while (!Mathx.IsEqual(path.Last(), path.First()))
                {
                    var vn = GetNextVertex(v0, v1);
                    path.Add(vn);

                    v0 = v1;
                    v1 = vn;
                }

                // Remove redundant nodes
                for (int i = path.Count - 2; i > 0; --i)
                {
                    var current = path[i];
                    var next = path[i - 1];
                    var prev = path[i + 1];

                    if (
                        Math.Sign(next.x - current.x) == Math.Sign(current.x - prev.x) &&
                        Math.Sign(next.y - current.y) == Math.Sign(current.y - prev.y)
                    )
                    {
                        path.RemoveAt(i);
                    }
                }

                result.Add(path);
            }
            
            return result;
        }

        private static List<Vector2> GetBorderPath(bool[] map, int width, int height)
        {
            var result = new List<Vector2>();

            result.Add(new Vector2(0.0f, 0.0f));
            result.Add(new Vector2(0.0f, height - 1));
            result.Add(new Vector2(width - 1, height - 1));
            result.Add(new Vector2(width - 1, 0.0f));

            return result;
        }
    }
}
