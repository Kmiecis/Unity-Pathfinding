using Common;
using Common.Extensions;
using Common.Mathematics;
using UnityEngine;

namespace Custom.CaveGeneration
{
    public class CaveColliders3D : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        protected Transform _parent;

        private bool[][][] _map;

        public bool[][][] Map
        {
            get => _map;
            set
            {
                _map = value;
                CheckColliders();
                RegenerateColliders(_map, _parent);
            }
        }

        private void CheckColliders()
        {
            _parent.DestroyChildren();
        }

        private static void RegenerateColliders(bool[][][] map, Transform parent)
        {
            var width = map.GetWidth();
            var height = map.GetHeight();
            var depth = map.GetDepth();

            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    for (int z = 0; z < depth - 1; z++)
                    {
                        var v = new Vector3Int(x, y, z);

                        var c = MarchingCubes.GetConfiguration(
                            !map[x][y][z], !map[x][y + 1][z], !map[x + 1][y + 1][z], !map[x + 1][y][z],
                            !map[x][y][z + 1], !map[x][y + 1][z + 1], !map[x + 1][y + 1][z + 1], !map[x + 1][y][z + 1]
                        );

                        if (c == 255)
                        {   // TODO
                            var collider = new GameObject("BoxCollider (Clone)").AddComponent<BoxCollider>();
                            collider.center = Vector3.one * 0.5f;
                            collider.size = Vector3.one;
                            collider.transform.parent = parent;
                            collider.transform.localPosition = v;
                        }
                    }
                }
            }
        }
    }
}
