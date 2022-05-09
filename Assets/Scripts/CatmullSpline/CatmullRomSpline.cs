using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CatmullRomSpline : MonoBehaviour
{
    [SerializeField]
    protected Vector3[] points;

    [SerializeField]
    private ControlPointMode[] modes;


    [SerializeField] private bool isConstantSpeed = true;

    public virtual Vector3 GetPoint(float t)
    {
        int i; // index of current curve

        // Prevent creating curve on points past max index
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 2;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i; // Convert t to local time t on current curve
        }

        return transform.TransformPoint(GetPointHelper(i, i + 1, t));
    }

    public virtual Vector3 GetPointLocal(float t, int curve)
    {
        return transform.TransformPoint(GetPointHelper(curve - 1, curve, t));
    }

    public virtual Vector3 GetDirectionLocal(float t, int curve)
    {
        return transform.TransformPoint(GetDirectionHelper(curve - 1, curve, t)).normalized;
    }


    private Vector3 GetPointHelper(int ind0, int ind1, float t)
    {
        Vector3 p0, p1;
        p0 = points[ind0];
        p1 = points[ind1];

        Vector3 m0, m1; //Tangents

        // Tangent M[k] = (P[k+1] - P[k-1])

        // m0
        if (ind0 == 0)
        {
            m0 = p1 - p0;
        }
        else
        {
            m0 = p1 - points[ind0 - 1];
        }

        // m1
        if (ind1 < points.Length - 2)
        {
            m1 = points[ind1 + 1] - p0;
        }
        else
        {
            m1 = p1 - p0;
        }

        m0 *= 0.5f;
        m1 *= 0.5f;

        // Get point based on time
        return CatmullRom.GetPoint(p0, p1, m0, m1, t);
    }

    // Get the length of the curve
    // Curve from 1-points.length-2
    public float GetCurveLength(int curve)
    {
        int ind0 = curve - 1;
        int ind1 = curve;

        Vector3 p0 = points[ind0];
        Vector3 p1 = points[ind1];
        Vector3 m0, m1; //Tangents

        // Tangent M[k] = (P[k+1] - P[k-1])

        // m0
        if (ind0 == 0)
        {
            m0 = p1 - p0;
        }
        else
        {
            m0 = p1 - points[ind0 - 1];
        }

        // m1
        if (ind1 < points.Length - 2)
        {
            m1 = points[ind1 + 1] - p0;
        }
        else
        {
            m1 = p1 - p0;
        }

        m0 *= 0.5f;
        m1 *= 0.5f;

        return CatmullRom.GetCurveLength(p0, p1, m0, m1, 10);
    }

    public virtual Vector3 GetDirection(float t)
    {
        int i; // index of current curve

        // Prevent creating curve on points past max index
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 2;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i; // Convert t to local time t on current curve
        }

        return transform.TransformPoint(GetDirectionHelper(i, i + 1, t)).normalized;
    }

    public void Reset()
    {
        points = new Vector3[] {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0f, 1f)
        };
    }

    // Adding new curve to the spline to make it continuous
    public void AddCurve()
    {
        Vector3 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 1);
        point.x += 20f;
        points[points.Length - 1] = point;
    }

    public void AddPoint(Vector3 newPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(newPoint);
        Array.Resize(ref points, points.Length + 1);
        points[points.Length - 1] = localPoint;
    }

    // Move a point on the curve
    public void SetControlPoint(int index, Vector3 point)
    {
        points[index] = point;
    }

    public int PointCount
    {
        get { return points.Length; }
    }



    private Vector3 GetDirectionHelper(int ind0, int ind1, float t)
    {
        Vector3 p0, p1;
        p0 = points[ind0];
        p1 = points[ind1];

        Vector3 m0, m1; //Tangents

        // Tangent M[k] = (P[k+1] - P[k-1])

        // m0
        if (ind0 == 0)
        {
            m0 = p1 - p0;
        }
        else
        {
            m0 = p1 - points[ind0 - 1];
        }

        // m1
        if (ind1 < points.Length - 2)
        {
            m1 = points[ind1 + 1] - p0;
        }
        else
        {
            m1 = p1 - p0;
        }

        m0 *= 0.5f;
        m1 *= 0.5f;

        return CatmullRom.GetFirstDerivative(p0, p1, m0, m1, t);
    }

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    public int CurveCount
    {
        get
        {
            return (points.Length - 1);
        }
    }

    public struct CatmullRomPoint
    {
        public Vector3 position;
        public Vector3 tangent;
        public Vector3 normal;

        public CatmullRomPoint(Vector3 position, Vector3 tangent, Vector3 normal)
        {
            this.position = position;
            this.tangent = tangent;
            this.normal = normal;
        }
    }
}
