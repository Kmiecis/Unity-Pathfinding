using Common.Mathematics;
using Custom.CaveGeneration;
using System.Collections;
using UnityEngine;

namespace Common
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

        private bool[] _caveMap;

        private bool[] BuildCaveMap()
        {
            var result = new bool[caveInput.width * caveInput.height];
            CaveGenerator.Generate(result, in caveInput);
            return result;
        }
        
        private void PositionGround()
        {
            if (_ground != null)
            {
                var width = (caveInput.width - 1);
                var height = (caveInput.height - 1);

                var localPosition = _ground.localPosition;
                var localScale = _ground.localScale;

                localPosition.x = width * 0.5f;
                localPosition.y = height * 0.5f;
                localScale.x = width;
                localScale.y = height;

                _ground.localPosition = localPosition;
                _ground.localScale = localScale;
            }
        }

        public void Build()
        {
            _caveMap = BuildCaveMap();
            if (_caveMesh != null)
                _caveMesh.SetMap(_caveMap, caveInput.width, caveInput.height);
            if (_caveCollider != null)
                _caveCollider.SetMap(_caveMap, caveInput.width, caveInput.height);
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
                var width = caveInput.width;
                var height = caveInput.height;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = Mathx.ToIndex(x, y, width);
                        bool isWall = _caveMap[i];
                        Gizmos.color = isWall ? Color.black : Color.white;
                        Gizmos.DrawWireSphere(new Vector3(x, y, 0.0f), 0.2f);
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
