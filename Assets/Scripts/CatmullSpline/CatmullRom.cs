using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullRom : MonoBehaviour
{
	public static Vector3 GetFirstDerivative(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t)
	{
		// p'(t) = (6t² - 6t)p0 + (3t² - 4t + 1)m0 + (-6t² + 6t)p1 + (3t² - 2t)m1
		Vector3 tangent = (6 * t * t - 6 * t) * start
			+ (3 * t * t - 4 * t + 1) * tanPoint1
			+ (-6 * t * t + 6 * t) * end
			+ (3 * t * t - 2 * t) * tanPoint2;

		return tangent.normalized;
	}

	//Calculates curve position at t[0, 1]
	public static Vector3 GetPoint(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t)
	{
		// Hermite curve formula:
		// (2t^3 - 3t^2 + 1) * p0 + (t^3 - 2t^2 + t) * m0 + (-2t^3 + 3t^2) * p1 + (t^3 - t^2) * m1
		Vector3 position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * start
			+ (t * t * t - 2.0f * t * t + t) * tanPoint1
			+ (-2.0f * t * t * t + 3.0f * t * t) * end
			+ (t * t * t - t * t) * tanPoint2;

		return position;
	}

	public static float GetCurveLength(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, int n)
	{
		float sumDistance = 0;
		for (int i = 0; i < n; i++)
		{
			Vector3 p0 = GetPoint(start, end, tanPoint1, tanPoint2, i / n);
			Vector3 p1 = GetPoint(start, end, tanPoint1, tanPoint2, (i + 1) / n);
			sumDistance += Vector3.Distance(p0, p1);
		}
		return sumDistance;
	}
}
