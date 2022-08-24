using Common;
using Common.Extensions;
using System.Collections;
using UnityEngine;

namespace Custom.CaveGeneration
{
    public class CaveManager3D : MonoBehaviour
    {
        [SerializeField]
        protected CaveMesh3D _caveMesh;

        public CaveGenerator3D.Input caveInput = CaveGenerator3D.Input.Default;

        private bool[][][] _caveMap;

        private void BuildCaveMap()
        {
            _caveMap = Arrays.New<bool>(caveInput.width, caveInput.height, caveInput.depth);
            CaveGenerator3D.Generate(_caveMap, in caveInput);
        }

        public void Build()
        {
            BuildCaveMap();
            if (_caveMesh != null)
                _caveMesh.Map = _caveMap;
        }

        private void Start()
        {
            Build();
        }

#if UNITY_EDITOR
        [Header("Gizmos")]
        [SerializeField]
        protected bool _drawCaveMap;

        private void OnDrawGizmos()
        {
            if (_drawCaveMap && _caveMap != null)
            {
                var width = _caveMap.GetWidth();
                var height = _caveMap.GetHeight();
                var depth = _caveMap.GetDepth();

                Gizmos.color = Color.cyan;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int z = 0; z < depth; z++)
                        {
                            bool isWall = _caveMap[x][y][z];
                            if (!isWall)
                            {
                                Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one * 0.9f);
                            }
                        }
                    }
                }
            }
        }

        private void OnValidate()
        {
            StartCoroutine(BuildNextFrame());
        }

        IEnumerator BuildNextFrame()
        {
            yield return null;
            Build();
        }
#endif
    }
}
