using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTest : MonoBehaviour
{

    public CatmullRomSpline m_Spline;

    [SerializeField] private float m_Duration = 5f;
    [SerializeField] private float m_Speed = 1;

    private float m_Dist = 0;
    private int m_CurrCurve = -1;
    private float m_CurrCurveLength = 0;

    // Update is called once per frame
    void Update()
    {
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
        m_Dist += m_Speed * Time.deltaTime;
        float t = (m_Dist) / m_CurrCurveLength;

        Vector3 position = m_Spline.GetPointLocal(t, m_CurrCurve);
        transform.localPosition = position;
        transform.LookAt(position + m_Spline.GetDirectionLocal(t, m_CurrCurve));

        // Move to next curve
        if (m_Dist >= m_CurrCurveLength)
        {
            m_Dist = 0;
            // Reset to beginning if reached end
            if (m_CurrCurve == m_Spline.CurveCount)
                m_CurrCurve = 1;
            // Otherwise, move to next curve
            else
                m_CurrCurve++;
            m_CurrCurveLength = m_Spline.GetCurveLength(m_CurrCurve);
        }
    }
}
