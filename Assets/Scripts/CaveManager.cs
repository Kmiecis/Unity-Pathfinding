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
        protected CaveTexture _caveTexture;
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
            if (_caveTexture != null)
                _caveTexture.Map = _caveMap;
            if (_caveCollider != null)
                _caveCollider.Map = _caveMap;
            PositionGround();
        }

        /*private void SetUnitPath()
        {
            var startPosition = Vector2Int.RoundToInt(_unit.transform.position);

            int caveWidth = _caveMap.GetWidth();
            int caveHeight = _caveMap.GetHeight();
            int x, y;
            do
            {
                x = Random.Range(0, caveWidth);
                y = Random.Range(0, caveHeight);
            }
            while (!_caveMap[x][y]);

            if (PF_Core.TryFindPath(_caveMap, startPosition.x, startPosition.y, x, y, out var node))
            {
                var path = PF_Core.ToPath(node);
                _unit.SetPath(path, Vector2.one);
                _unit.StartFollowPath();
            }
        }*/

        private void Start()
        {
            Build();
        }

        /*private void Update()
        {
            if (_unit != null && _unit.HasReachedDestination)
            {
                SetUnitPath();
            }
        }*/

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
