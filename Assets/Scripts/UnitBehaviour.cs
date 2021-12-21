using Common;
using Common.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Custom
{
    public class UnitBehaviour : MonoBehaviour
    {
        public float speed = 1.0f;

        private List<Vector2Int> _currentPath = new List<Vector2Int>();
        private Vector2 _currentScale;
        private int _currentIndex;

        private Coroutine _followPathRoutine;

        public void SetPath(List<Vector2Int> path, Vector2 scale)
        {
            _currentPath = path;
            _currentScale = scale;
            _currentIndex = 0;
        }

        public void StartFollowPath()
        {
            StopFollowPath();
            _followPathRoutine = StartCoroutine(FollowPathCoroutine());
        }

        public void StopFollowPath()
        {
            if (_followPathRoutine != null)
                StopCoroutine(_followPathRoutine);
        }

        public bool HasReachedDestination => _currentIndex == _currentPath.Count;

        private IEnumerator FollowPathCoroutine()
        {
            while (_currentIndex < _currentPath.Count)
            {
                var next = _currentPath[_currentPath.Count - 1 - _currentIndex];

                var fromPosition = transform.position;
                var toPosition = new Vector3(next.x * _currentScale.x, next.y * _currentScale.y);

                var t = 0.0f;
                var f = 1.0f / (toPosition - fromPosition).magnitude;
                while (t < 1.0f)
                {
                    var newPosition = Vector3.Lerp(fromPosition, toPosition, t);
                    transform.position = newPosition;

                    t += f * speed * Time.deltaTime;

                    yield return null;
                }

                transform.position = toPosition;
                _currentIndex++;

                yield return null;
            }
        }

#if UNITY_EDITOR
        [Header("Gizmos")]
        [SerializeField]
        protected bool _drawPath = true;

        private void OnDrawGizmos()
        {
            if (_drawPath && _currentPath != null)
            {
                var positions = new Vector3[_currentPath.Count];
                for (int i = 0; i < _currentPath.Count; ++i)
                {
                    positions[i] = (_currentPath[i] * _currentScale).XY_() + Vector3.back;
                }

                Gizmos.color = Color.green;
                GizmosUtility.DrawLines(positions);
                GizmosUtility.DrawWireSpheres(positions, 0.2f);
            }
        }
#endif
    }
}
