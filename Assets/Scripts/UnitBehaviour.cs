using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Custom
{
    public class UnitBehaviour : MonoBehaviour
    {
        public float speed = 1.0f;
#if UNITY_EDITOR
        [SerializeField] protected bool m_ShowPathGizmo = true;
#endif
        private List<Vector2Int> m_CurrentPath = new List<Vector2Int>();
        private Vector2 m_CurrentScale;
        private int m_CurrentIndex;

        private Coroutine m_FollowPathRoutine;

        public void SetPath(List<Vector2Int> path, Vector2 scale)
        {
            m_CurrentPath = path;
            m_CurrentScale = scale;
            m_CurrentIndex = 0;
        }

        public void StartFollowPath()
        {
            StopFollowPath();
            m_FollowPathRoutine = StartCoroutine(FollowPathCoroutine());
        }

        public void StopFollowPath()
        {
            if (m_FollowPathRoutine != null)
                StopCoroutine(m_FollowPathRoutine);
        }

        public bool HasReachedDestination => m_CurrentIndex == m_CurrentPath.Count;

        private IEnumerator FollowPathCoroutine()
        {
            while (m_CurrentIndex < m_CurrentPath.Count)
            {
                var next = m_CurrentPath[m_CurrentPath.Count - 1 - m_CurrentIndex];

                var fromPosition = transform.position;
                var toPosition = new Vector3(next.x * m_CurrentScale.x, next.y * m_CurrentScale.y);

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
                m_CurrentIndex++;

                yield return null;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!m_ShowPathGizmo)
                return;

            Gizmos.color = Color.green;

            for (int i = 1; m_CurrentPath != null && i < m_CurrentPath.Count; i++)
            {
                var prevNode = m_CurrentPath[i - 1];
                var currentNode = m_CurrentPath[i];

                var prevPosition = new Vector3(prevNode.x * m_CurrentScale.x, prevNode.y * m_CurrentScale.y) + Vector3.back;
                var currentPosition = new Vector3(currentNode.x * m_CurrentScale.x, currentNode.y * m_CurrentScale.y) + Vector3.back;

                Gizmos.DrawLine(prevPosition, currentPosition);
                Gizmos.DrawWireSphere(prevPosition, 0.2f);
                Gizmos.DrawWireSphere(currentPosition, 0.2f);
            }
        }
#endif
    }
}
