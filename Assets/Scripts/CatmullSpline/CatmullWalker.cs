using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullWalker : MonoBehaviour
{ 
    [SerializeField] protected float m_Duration = 5f;
    [SerializeField] protected float m_Speed = 1;
    [SerializeField] protected bool m_IsFollowingspline = true;
    [SerializeField] protected SplineCreator m_Spline;

    private float m_Dist = 0;
    private int m_CurrCurve = -1;
    private float m_CurrCurveLength = 0;
    protected float m_CurrSpeed;

    private bool m_StartMoving = false;

    protected virtual void Start()
    {
        m_CurrSpeed = m_Speed;
        transform.position = m_Spline.GetPoint(0);
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        if (!m_IsFollowingspline) {
            return;
        }

        if (!m_Spline.IsInitialized)
            m_Spline.InitializeSplineAtHead();

        MovePlayerConstant();
    }

    // Uses distance travelled so far inside current curve 
    private void MovePlayerConstant()
    {
        // If first move, initialize starting values
        if (m_CurrCurve == -1)
        {
            m_Dist = 0;
            m_CurrCurve = 1;
            m_CurrCurveLength = m_Spline.GetCurveLength(m_CurrCurve);
        }

        // convert distance travelled to percent curve has been walked along
        m_Dist += m_CurrSpeed * Time.deltaTime;
        float t = (m_Dist) / m_CurrCurveLength;

        Vector3 position = m_Spline.GetPointLocal(t, m_CurrCurve);
        transform.localPosition = position;
        transform.LookAt(position + m_Spline.GetDirectionLocal(t, m_CurrCurve));

        // Move to next curve
        if (m_Dist >= m_CurrCurveLength)
        {
            NewCurveEntered();
        }
    }

    // called whenever a new curve has been entered.
    protected virtual void NewCurveEntered() 
    {
        m_Spline.AddNewPoint();
        m_Dist = 0;
        // Reset to beginning if reached end
        if (m_CurrCurve == m_Spline.CurveCount)
            m_CurrCurve = 1;
        // Otherwise, move to next curve
        else
            m_CurrCurve++;
        m_CurrCurveLength = m_Spline.GetCurveLength(m_CurrCurve);
    }

    public bool IsFollowSpline 
    {
        get { return m_IsFollowingspline;  }
        set { m_IsFollowingspline = value; }
    }


    public SplineCreator spline
    {
        get { return m_Spline; }
    }
}
