using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullWalker : MonoBehaviour
{ 
    [SerializeField] protected float m_Duration = 5f;
    [SerializeField] protected SplineCreator m_Spline;
    [SerializeField] protected float m_Speed = 1;
    [SerializeField] protected float m_MaxTurnAngle = 40f;

    private float m_Dist = 0;
    protected int m_CurrCurve = -1;
    private float m_CurrCurveLength = 0;
    protected float m_CurrSpeed;

    protected virtual void Start()
    {
        m_CurrSpeed = m_Speed;
    }

    // Called in controllers update method
    public virtual void TryMove()
    {
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
        m_Dist += m_CurrSpeed * Time.fixedDeltaTime;  
        float t = (m_Dist) / m_CurrCurveLength;

        Vector3 position = m_Spline.GetPointLocal(t, m_CurrCurve);
        transform.position = position;
        Vector3 newRotation = m_Spline.GetDirectionLocal(t, m_CurrCurve);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(newRotation), m_MaxTurnAngle);

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

    protected void ResetValues()
    {
        m_Dist = 0;
        m_CurrCurve = -1;
        m_CurrCurveLength = 0;
        m_CurrSpeed = m_Speed;
    }

    public SplineCreator spline
    {
        get { return m_Spline; }
    }

    public float Speed { get { return m_Speed; } }
}
