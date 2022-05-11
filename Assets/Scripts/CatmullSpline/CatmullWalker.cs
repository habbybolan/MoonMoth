using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullWalker : MonoBehaviour
{ 
    public SplineCreator m_Spline;

    [SerializeField] protected float m_Duration = 5f;
    [SerializeField] protected float m_Speed = 1;
    [SerializeField] protected bool m_IsFollowingspline = true;

    private float m_Dist = 0;
    private int m_CurrCurve = -1;
    private float m_CurrCurveLength = 0;
    protected float m_CurrSpeed;

    private bool m_StartMoving = false;

    private void Start()
    {
        m_CurrSpeed = m_Speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_IsFollowingspline) {
            transform.transform.Translate(Vector3.forward * m_Speed * Time.deltaTime);
            return;
        }

        if (!m_StartMoving)
        {
            m_Spline.InitializeSplineAtHead();
            m_StartMoving = true;
        }
       

        if (m_Spline && m_StartMoving)
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

    // called whenever a new tile has been entered.
    // Virtual to allow any child class to add to the functionality
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

    public Tile GetTileInFront(int index)
    {
        return m_Spline.GetTileInfront(index);
    }
}
