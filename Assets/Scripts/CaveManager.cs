using Common;
using Common.Extensions;
using Custom.CaveGeneration;
using System.Collections;
using UnityEngine;

namespace Custom
{
    public class CaveManager : MonoBehaviour
    {
        [SerializeField]
        protected CaveMesh _caveMesh;
        [SerializeField]
        protected CaveCollider _caveCollider;
        [SerializeField]
        protected Transform _ground;

        public CaveGenerator.Input caveInput = CaveGenerator.Input.Default;

        private bool[][] _caveMap;

        private void BuildCaveMap()
        {
            _caveMap = Arrays.New<bool>(caveInput.width, caveInput.height);
            CaveGenerator.Generate(_caveMap, in caveInput);
        }
        
        private void PositionGround()
        {
            if (_ground != null)
            {
                var width = (_caveMap.GetWidth() - 1);
                var height = (_caveMap.GetHeight() - 1);
                _ground.localPosition = new Vector3(width * 0.5f, height * 0.5f, 0.0f);
                _ground.localScale = new Vector3(width, height, 1.0f);
            }
        }

        public void Build()
        {
            BuildCaveMap();
            if (_caveMesh != null)
                _caveMesh.Map = _caveMap;
            if (_caveCollider != null)
                _caveCollider.Map = _caveMap;
            PositionGround();
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
            if (_drawCaveMap)
            {
                var caveMapWidth = _caveMap.GetWidth();
                var caveMapHeight = _caveMap.GetHeight();

                for (int y = 0; y < caveMapHeight; y++)
                {
                    for (int x = 0; x < caveMapWidth; x++)
                    {
                        bool isWall = _caveMap[x][y];
                        Gizmos.color = isWall ? Color.black : Color.white;
                        Gizmos.DrawWireSphere(new Vector3(x, y, -1.0f), 0.2f);
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
