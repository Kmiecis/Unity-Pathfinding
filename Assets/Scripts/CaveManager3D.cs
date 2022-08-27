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
        [SerializeField]
        protected CaveColliders3D _caveColliders;

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
            if (_caveColliders != null)
                _caveColliders.Map = _caveMap;
        }

        private void Start()
        {
            Build();
        }

#if UNITY_EDITOR
        [Header("Gizmos")]
        [SerializeField]
        protected Color _wallColor;
        [SerializeField]
        protected Color _roomColor;

        private void OnDrawGizmos()
        {
            if (_caveMap != null && (_wallColor.a > 0.0f || _roomColor.a > 0.0f))
            {
                var width = _caveMap.GetWidth();
                var height = _caveMap.GetHeight();
                var depth = _caveMap.GetDepth();

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int z = 0; z < depth; z++)
                        {
                            var v = new Vector3(x, y, z);

                            bool isRoom = _caveMap[x][y][z];
                            if (!isRoom && _wallColor.a > 0.0f)
                            {
                                Gizmos.color = _wallColor;
                                Gizmos.DrawWireSphere(v, 0.5f);
                            }
                            if (isRoom && _roomColor.a > 0.0f)
                            {
                                Gizmos.color = _roomColor;
                                Gizmos.DrawWireSphere(v, 0.5f);
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
