using Common;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
    public class PF_Agent : MonoBehaviour
    {
        public float speed = 1.0f;

        private List<Vector3> _path;
        private int _node = 0;

        private bool _follow;

        public bool HasPath
        {
            get => _path != null;
        }

        public void Move(Vector3 position)
        {
            Halt();

            foreach (var instance in PF_Instance.Instances)
            {
                var startGridPosition = instance.ToGridPosition(transform.position);
                var targetGridPosition = instance.ToGridPosition(position);
                if (
                    instance.Contains(startGridPosition) &&
                    instance.Contains(targetGridPosition)
                )
                {
                    var grid = instance.Grid;

                    if (PF_Core.TryFindPath(grid, startGridPosition, targetGridPosition, out var path))
                    {
                        _path = new List<Vector3>(path.Count);
                        _path.Add(transform.position);
                        foreach (var gridPosition in path)
                        {
                            var pathPosition = instance.FromGridPosition(gridPosition);
                            _path.Add(pathPosition);
                        }
                        _path.Add(position);

                        Resume();
                        return;
                    }
                }
            }
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
                    positions[i] = _path[i] + Vector3.back;
                }

                Gizmos.color = Color.green;
                GizmosUtility.DrawLines(positions);
                GizmosUtility.DrawWireSpheres(positions, 0.2f);
            }
        }
#endif
    }
}
