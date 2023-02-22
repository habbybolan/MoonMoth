using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CatmullRomSpline : MonoBehaviour
{
    [SerializeField]
    protected Vector3[] m_Points;

    [SerializeField] private bool isConstantSpeed = true;

    // Get the point at a time t along the entire spline, where t [0, 1] is the time on the spline
    // Time is not constant along the curve.
    //  - If curve A is longer than curve B, the time t difference from the start and center of the curve will be the same t amount
    public virtual Vector3 GetPoint(float t)
    {
        int i; // index of current curve

        // Prevent creating curve on points past max index
        if (t >= 1f)
        {
            t = 1f;
            i = m_Points.Length - 2;
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

    // Get the point on a specific curve at time t represented by the points at indexes ind0 and ind1
    private Vector3 GetPointHelper(int ind0, int ind1, float t)
    {
        Vector3 p0, p1;
        p0 = m_Points[ind0];
        p1 = m_Points[ind1];

        Vector3 m0, m1; //Tangents

        // Tangent M[k] = (P[k+1] - P[k-1])

        // m0
        if (ind0 == 0)
        {
            m0 = p1 - p0;
        }
        else
        {
            m0 = p1 - m_Points[ind0 - 1];
        }

        // m1
        if (ind1 < m_Points.Length - 2)
        {
            m1 = m_Points[ind1 + 1] - p0;
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

        Vector3 p0 = m_Points[ind0];
        Vector3 p1 = m_Points[ind1];
        Vector3 m0, m1; //Tangents

        // Tangent M[k] = (P[k+1] - P[k-1])

        // m0
        if (ind0 == 0)
        {
            m0 = p1 - p0;
        }
        else
        {
            m0 = p1 - m_Points[ind0 - 1];
        }

        // m1
        if (ind1 < m_Points.Length - 2)
        {
            m1 = m_Points[ind1 + 1] - p0;
        }
        else
        {
            m1 = p1 - p0;
        }

        m0 *= 0.5f;
        m1 *= 0.5f;

        return CatmullRom.GetCurveLength(p0, p1, m0, m1, 10);
    }

    // Get the direction at a time t along the entire spline, where t [0, 1] is the time on the spline
    public virtual Vector3 GetDirection(float t)
    {
        int i; // index of current curve

        // Prevent creating curve on points past max index
        if (t >= 1f)
        {
            t = 1f;
            i = m_Points.Length - 2;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i; // Convert t to local time t on current curve
        }

        return transform.TransformPoint(GetDirectionHelper(i, i + 1, t)).normalized;
    }

    // Adding new curve to the spline to make it continuous
    public void AddCurve()
    {
        Vector3 point = m_Points[m_Points.Length - 1];
        Array.Resize(ref m_Points, m_Points.Length + 1);
        point.x += 20f;
        m_Points[m_Points.Length - 1] = point;
    }

    public void AddPoint(Vector3 newPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(newPoint);
        if (m_Points == null)
        {
            m_Points = new Vector3[] { };
        }
        Array.Resize(ref m_Points, m_Points.Length + 1);
        
        m_Points[m_Points.Length - 1] = localPoint;
    }

    // Move a point on the curve
    public void SetControlPoint(int index, Vector3 point)
    {
        m_Points[index] = point;
    }

    public int PointCount
    {
        get { return m_Points.Length; }
    }

    private Vector3 GetDirectionHelper(int ind0, int ind1, float t)
    {
        Vector3 p0, p1;
        p0 = m_Points[ind0];
        p1 = m_Points[ind1];

        Vector3 m0, m1; //Tangents

        // Tangent M[k] = (P[k+1] - P[k-1])

        // m0
        if (ind0 == 0)
        {
            m0 = p1 - p0;
        }
        else
        {
            m0 = p1 - m_Points[ind0 - 1];
        }

        // m1
        if (ind1 < m_Points.Length - 2)
        {
            m1 = m_Points[ind1 + 1] - p0;
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
        return m_Points[index];
    }

    public int CurveCount
    {
        get
        {
            return (m_Points.Length - 1);
        }
    }

    public Vector3 EndOfSpline
    {
        get { return m_Points[m_Points.Length - 1]; }
    }
}
