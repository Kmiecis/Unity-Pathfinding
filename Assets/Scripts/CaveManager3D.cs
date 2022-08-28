using Common.Mathematics;
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

        private bool[] _caveMap;

        private bool[] BuildCaveMap()
        {
            var result = new bool[caveInput.width * caveInput.height * caveInput.depth];
            CaveGenerator3D.Generate(result, in caveInput);
            return result;
        }

        public void Build()
        {
            _caveMap = BuildCaveMap();
            if (_caveMesh != null)
                _caveMesh.SetMap(_caveMap, caveInput.width, caveInput.height, caveInput.depth);
            if (_caveColliders != null)
                _caveColliders.SetMap(_caveMap, caveInput.width, caveInput.height, caveInput.depth);
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
                var width = caveInput.width;
                var height = caveInput.height;
                var depth = caveInput.depth;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int z = 0; z < depth; z++)
                        {
                            var v = new Vector3(x, y, z);
                            int i = Mathx.ToIndex(x, y, z, width, height);

                            bool isRoom = _caveMap[i];
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
