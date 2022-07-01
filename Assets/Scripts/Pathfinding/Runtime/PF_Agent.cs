using Common;
using Common.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
    public class PF_Agent : MonoBehaviour
    {
        public float speed = 1.0f;

        private List<Vector2> _path;
        private int _node = 0;

        private Coroutine _followPathRoutine;

        public bool HasPath
            => _path != null;

        public void Move(Vector2 p)
        {
            Halt();

            foreach (var instance in PF_Instance.Instances)
            {
                var startPosition = instance.ToGridPosition(transform.position);
                var targetPosition = instance.ToGridPosition(p);
                if (
                    instance.Contains(startPosition) &&
                    instance.Contains(targetPosition)
                )
                {
                    var grid = instance.Grid;

                    if (PF_Core.TryFindPath(grid, startPosition.x, startPosition.y, targetPosition.x, targetPosition.y, out var path))
                    {
                        _path = new List<Vector2>(path.Count);
                        foreach (var gridPosition in path)
                        {
                            var position = instance.FromGridPosition(gridPosition);
                            _path.Add(position);
                        }

                        Resume();
                        return;
                    }
                }
            }
        }

        public void Resume()
        {
            _followPathRoutine = StartCoroutine(FollowPathCoroutine());
        }

        public void Halt()
        {
            if (_followPathRoutine != null)
            {
                StopCoroutine(_followPathRoutine);
                _followPathRoutine = null;
            }
        }

        private IEnumerator FollowPathCoroutine()
        {
            while (_path != null)
            {
                var fromPosition = (Vector2)transform.position;
                var toPosition = _path[_node];

                var t = 0.0f;
                var f = 1.0f / (toPosition - fromPosition).magnitude;
                while (t < 1.0f)
                {
                    var newPosition = Vector2.Lerp(fromPosition, toPosition, t);
                    transform.position = newPosition;

                    t += f * speed * Time.deltaTime;

                    yield return null;
                }

                transform.position = toPosition;
                _node++;

                if (_node >= _path.Count)
                {
                    _path = null;
                    _node = 0;
                }

                yield return null;
            }
        }

#if UNITY_EDITOR
        [Header("Editor")]
        public bool showPath = true;

        private void OnDrawGizmos()
        {
            if (showPath && _path != null)
            {
                var positions = new Vector3[_path.Count];
                for (int i = 0; i < _path.Count; ++i)
                {
                    positions[i] = _path[i].XY_() + Vector3.back;
                }

                Gizmos.color = Color.green;
                GizmosUtility.DrawLines(positions);
                GizmosUtility.DrawWireSpheres(positions, 0.2f);
            }
        }
#endif
    }
}
