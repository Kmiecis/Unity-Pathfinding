using Common;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
    public class PF_Agent : MonoBehaviour
    {
        public float speed = 1.0f;
        public int size = 1;

        private Vector3 _worldTarget;
        private Vector3 _gridTarget;

        private List<Vector3> _path;
        private int _node = 0;

        private bool _follow;

        public bool HasPath
        {
            get => _path != null;
        }

        public bool TryMove(Vector3 position)
        {
            Halt();

            foreach (var instance in PF_IInstance.Instances)
            {
                var startPosition = transform.position;
                var targetPosition = position;

                if (instance.TryFindPath(startPosition, targetPosition, size, out _path))
                {
                    _worldTarget = position;
                    _gridTarget = instance.RoundToGrid(_worldTarget);

                    Resume();
                    return true;
                }
            }

            return false;
        }

        public void Resume()
        {
            _follow = true;
        }

        public void Halt()
        {
            _follow = false;
        }

        private void FollowPath(float deltaTime)
        {
            var currentPosition = transform.position;

            var movement = speed * deltaTime;
            while (movement > 0.0f && _path != null)
            {
                var targetPosition = _path[_node];

                var direction = targetPosition - currentPosition;
                var distance = direction.magnitude;
                direction.Normalize();

                if (distance > movement)
                {
                    currentPosition += movement * direction;
                    movement = 0.0f;
                }
                else
                {
                    currentPosition += distance * direction;
                    movement -= distance;

                    _node++;
                    if (_node == _path.Count)
                    {
                        _path = null;
                        _node = 0;

                        _worldTarget = default;
                        _gridTarget = default;
                    }
                }
            }

            transform.position = currentPosition;
        }

        private void FixedUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;

            if (_follow && _path != null)
            {
                FollowPath(deltaTime);
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
                    positions[i] = _path[i];
                }

                Gizmos.color = Color.green;
                UGizmos.DrawLines(positions);
                UGizmos.DrawWireSpheres(positions, 0.2f);

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_worldTarget, 0.2f);

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_gridTarget, 0.2f);
            }
        }
#endif
    }
}
