using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

	const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;

	public Transform target;
	public float speed = 20;
	public float turnSpeed = 3;
	public float turnDist = 5;
	public float stoppingDist = 10;

	Path path;
    bool recalculatePath = false;

	private void Start()
	{
        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        
    }

    public void RecalculatePath()
    {
        recalculatePath = true;
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
	{
		if (pathSuccessful)
		{
			this.path = new Path(waypoints, transform.position, turnDist, stoppingDist);
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator UpdatePath()
	{
		if (Time.timeSinceLevelLoad < .3f)
		{
			yield return new WaitForSeconds(.3f);
		}
		PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));

		float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
		Vector3 targetPosOld = target.position;

        while (true)
		{
            yield return new WaitForSeconds(minPathUpdateTime);
            // Request new path if Target has changed it's position significantly.
			if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
			{
				PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
				targetPosOld = target.position;
			}
            // Request new path if necessary.
            if (recalculatePath)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
                recalculatePath = false;
            }
        }
    }

	IEnumerator FollowPath()
	{
		bool followingPath = true;
		int pathIndex = 0;
		transform.LookAt(path.lookPoints[0]);

		float speedPercent = 1;

		while (followingPath)
		{
			Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
			while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
			{
				if (pathIndex == path.finishLineIndex)
				{
					followingPath = false;
					break;
				}
				else
				{
					++pathIndex;
				}
			}

			if (followingPath)
			{
				if (pathIndex >= path.slowDownIndex && stoppingDist > 0)
				{
					speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDist);
					if (speedPercent < .01f)
					{
						followingPath = false;
					}
				}

                // Rotate toward next point.
				Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

				// Move forward.
				transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
			}
			yield return null;
		}
	}

	public void OnDrawGizmos()
	{
		if (path != null)
		{
			path.DrawWithGizmos();
		}
	}
}
