using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Linear line representation from point on line and point perpendicular to line.
/// </summary>
public struct Line {

	const float verticalLineGradient = 1e5f;
    // y = mx + b.
	float gradient;         // m
	float y_intercept;      // b
	Vector2 pointOnLine_1;
	Vector2 pointOnLine_2;

	float gradientPerpendicular;

    bool approachSide;

    /// <summary>
    /// Construction of line based on point on it and point perpendicular to line.
    /// </summary>
	public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
	{
		float dx = pointOnLine.x - pointPerpendicularToLine.x;
		float dy = pointOnLine.y - pointPerpendicularToLine.y;

        // In case 'dx' is 0, prevent division by 0 when line is vertical.
		if (dx == 0)
		{
			this.gradientPerpendicular = verticalLineGradient;
		}
		else
		{
			this.gradientPerpendicular = dy / dx;
		}

        // In case 'dy' is 0.
		if (gradientPerpendicular == 0)
		{
			gradient = verticalLineGradient;
		}
		else
		{
			gradient = -1 / gradientPerpendicular;
		}
		// y = mx + b -> b = y - mx
		y_intercept = pointOnLine.y - gradient * pointOnLine.x;

		pointOnLine_1 = pointOnLine;
		pointOnLine_2 = pointOnLine + new Vector2(1, gradient);

		approachSide = false;   // ASssign default value, to prevent error with no value assigned.
		approachSide = GetSide(pointPerpendicularToLine);	
	}

    /// <summary>
    /// Returns side from which we approach the line.
    /// </summary>
	bool GetSide(Vector2 p)
	{
		return (p.x - pointOnLine_1.x) * (pointOnLine_2.y * pointOnLine_1.y) > (p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
	}

    /// <summary>
    /// Return flag whether we have crossed the line based on actual position.
    /// </summary>
	public bool HasCrossedLine(Vector2 p)
	{
		return GetSide(p) != approachSide;
	}

    /// <summary>
    /// Calculate distance to point.
    /// </summary>
	public float DistanceFromPoint(Vector2 p)
	{
		float yInterceptPerpendicular = p.y - gradientPerpendicular * p.x;
		float intersectX = (yInterceptPerpendicular - y_intercept) / (gradient - gradientPerpendicular);
		float intersectY = gradient * intersectX + y_intercept;
		return Vector2.Distance(p, new Vector2(intersectX, intersectY));
	}

    /// <summary>
    /// Draw line in Gizmos.
    /// </summary>
	public void DrawWithGizmos(float length)
	{
		Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
		Vector3 lineCentre = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;	// +.up so it will be above ground

		Gizmos.DrawLine(lineCentre - lineDir * length / 2f, lineCentre + lineDir * length / 2f);
	}
}
