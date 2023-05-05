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

        public void SetMap(bool[] map, int width, int height, int depth)
        {
            _parent.DestroyChildren();
            RegenerateColliders(map, width, height, depth, _parent);
        }

        private static void RegenerateColliders(bool[] map, int width, int height, int depth, Transform parent)
        {
            for (int z = 0; z < depth - 1; z++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    for (int x = 0; x < width - 1; x++)
                    {
                        var v = new Vector3Int(x, y, z);

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
