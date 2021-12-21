using Common.Extensions;
using Custom.CaveGeneration;
using Custom.Pathfinding;
using System.Collections;
using UnityEngine;

namespace Custom
{
    public class ManagerBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected CaveTexture _caveTexture;
        [SerializeField]
        protected CaveMesh _caveMesh;
        [SerializeField]
        protected Transform[] _scalables;

        public UnitBehaviour unitBehaviour;

        public float squareSize = 1.0f;
        public CaveGenerator.Input caveInput = CaveGenerator.Input.Default;
        [Space(10)]
        public bool auto = true;
        public Vector2Int targetPosition;

        private bool[,] _caveMap;

        private void BuildCaveMap()
        {
            _caveMap = new bool[caveInput.width, caveInput.height];
            CaveGenerator.Generate(_caveMap, in caveInput);
        }
        
        private void PositionChildren()
        {
            if (_scalables != null)
            {
                foreach (var scalable in _scalables)
                {
                    var width = (_caveMap.GetWidth() - 1) * squareSize;
                    var height = (_caveMap.GetHeight() - 1) * squareSize;
                    scalable.localPosition = new Vector3(width * 0.5f, height * 0.5f, 0.0f);
                    scalable.localScale = new Vector3(width, height, 1.0f);
                }
            }
        }

        public void Build()
        {
            BuildCaveMap();
            if (_caveMesh != null)
                _caveMesh.Map = _caveMap;
            if (_caveTexture != null)
                _caveTexture.Map = _caveMap;
            PositionChildren();
        }

        public void SetUnitPath()
        {
            var startPosition = Vector2Int.RoundToInt(unitBehaviour.transform.position);
            if (PathGenerator.TryFindPath(_caveMap, startPosition.x, startPosition.y, targetPosition.x, targetPosition.y, out var node))
            {
                var path = PathGenerator.ToLeanPath(node);
                unitBehaviour.SetPath(path, Vector2.one * squareSize);
            }
        }

        private void AutoUnitPath()
        {
            var startPosition = Vector2Int.RoundToInt(unitBehaviour.transform.position);

            int caveWidth = _caveMap.GetLength(0);
            int caveHeight = _caveMap.GetLength(1);
            int x, y;
            do
            {
                x = Random.Range(0, caveWidth);
                y = Random.Range(0, caveHeight);
            }
            while (!_caveMap[x, y]);

            targetPosition = new Vector2Int(x, y);

            if (PathGenerator.TryFindPath(_caveMap, startPosition.x, startPosition.y, targetPosition.x, targetPosition.y, out var node))
            {
                var path = PathGenerator.ToLeanPath(node);
                unitBehaviour.SetPath(path, Vector2.one * squareSize);
                unitBehaviour.StartFollowPath();
            }
        }

        private void Start()
        {
            Build();
        }

        private void Update()
        {
            if (auto && unitBehaviour != null && unitBehaviour.HasReachedDestination)
            {
                AutoUnitPath();
            }
        }

#if UNITY_EDITOR
        [Header("Gizmos")]
        [SerializeField]
        protected bool _drawTarget;
        [SerializeField]
        protected bool _drawCaveMap;

        private void OnDrawGizmos()
        {
            if (_drawCaveMap)
            {
                var caveMapWidth = _caveMap.GetLength(0);
                var caveMapHeight = _caveMap.GetLength(1);

                for (int y = 0; y < caveMapHeight; y++)
                {
                    for (int x = 0; x < caveMapWidth; x++)
                    {
                        bool isWall = _caveMap[x, y];
                        Gizmos.color = isWall ? Color.black : Color.white;
                        Gizmos.DrawWireSphere(new Vector3(x * squareSize, y * squareSize, -1.0f), 0.2f);
                    }
                }
            }

            if (_drawTarget)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(new Vector3(targetPosition.x * squareSize, targetPosition.y * squareSize, -1.0f), 0.5f);
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
