using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullWalker : MonoBehaviour
{ 
    [SerializeField] protected float m_Duration = 5f;
    [SerializeField] protected SplineCreator m_Spline;
    [SerializeField] protected float m_Speed = 1;
    [SerializeField] protected float m_MaxTurnAngle = 40f;
    [SerializeField] protected int m_numLUTSegments = 20;

    private float m_Dist = 0;
    protected int m_CurrCurve = -1;
    private float m_CurrCurveLength = 0;
    protected float m_CurrSpeed;
    protected float[] m_LUT;


    // If movement uses RigidBody, child class will set a value for this
    protected Rigidbody m_RigidBody;

    protected virtual void Start()
    {
        m_CurrSpeed = m_Speed;
        m_LUT = new float[m_numLUTSegments];
    }

    // Called in controllers update method
    public virtual void TryMove()
    {
        bool isSplineInitialized = IsSplineInitialized();
        if (!isSplineInitialized)
            m_Spline.InitializeSplineAtHead();

        MovePlayerConstant(!isSplineInitialized);
    }

    private void CreateLUTForCurrCurve()
    {
        Vector3 prevPos = m_Spline.GetPointLocal(0, m_CurrCurve);
        float currDist = 0;
        m_LUT[0] = 0;
        for (int i = 1; i < m_numLUTSegments; i++)
        {
            Vector3 nextPos = m_Spline.GetPointLocal((float)i/ m_numLUTSegments, m_CurrCurve);
            currDist += Vector3.Distance(prevPos, nextPos);
            m_LUT[i] = currDist;
            prevPos = nextPos;
        }
    }

    private float TFromLUT(float distance)
    {
        float arcLength = m_LUT[m_LUT.Length - 1];
        int n = m_LUT.Length;

        if (distance >= 0 && distance <= arcLength)
        {
            for (int i = 0; i < n - 1; i++)
            {
                if (distance >= m_LUT[i] && distance <= m_LUT[i + 1])
                {
                    float distT = Mathf.InverseLerp(m_LUT[i], m_LUT[i + 1], distance);
                    float seg = Mathf.Lerp(i , i + 1, distT);
                    return seg / (m_numLUTSegments - 1);
                }
            }
        }
        // distance outside of length of curve
        return distance / arcLength;
    }

    // Uses distance travelled so far inside current curve 
    private void MovePlayerConstant(bool isFirstMove)
    {
        // If first move, initialize starting values
        if (m_CurrCurve == -1)
        {
            m_Dist = 0;
            m_CurrCurve = 1;
            m_CurrCurveLength = m_Spline.GetCurveLength(m_CurrCurve);
            CreateLUTForCurrCurve();

            // Set initial position
            if (m_RigidBody != null)
            {
                m_RigidBody.transform.position = m_Spline.GetPointLocal(0, m_CurrCurve);
            } else
            {
                transform.position = m_Spline.GetPointLocal(0, m_CurrCurve);
            }
        }

        // convert distance travelled to percent curve has been walked along
        m_Dist += m_CurrSpeed * Time.fixedDeltaTime;
        float t = Mathf.Clamp01(TFromLUT(m_Dist));

        Vector3 newRotation = m_Spline.GetDirectionLocal(t, m_CurrCurve);
        Vector3 position;
        position = m_Spline.GetPointLocal(t, m_CurrCurve);

        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(newRotation), m_MaxTurnAngle);

        // If no rigidBody, move transform direction
        if (m_RigidBody != null)
        {
            // If first move, move instantly
            if (isFirstMove)
            {
                m_RigidBody.transform.position = position;
                m_RigidBody.transform.rotation = rotation;
            } else
            {
                m_RigidBody.MovePosition(position);
                m_RigidBody.MoveRotation(rotation);
            }
            
        } else
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        // Move to next curve
        if (t >= 1) // m_Dist >= m_CurrCurveLength)
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
        CreateLUTForCurrCurve();
    }

    protected void ResetValues()
    {
        m_Dist = 0;
        m_CurrCurve = -1;
        m_CurrCurveLength = 0;
        m_CurrSpeed = m_Speed;
    }

    protected bool IsSplineInitialized()
    {
        return m_Spline.IsInitialized;
    }

    public SplineCreator spline
    {
        get { return m_Spline; }
    }

    public float Speed { get { return m_Speed; } }
    public Rigidbody RigidBody { get { return m_RigidBody; } }
}
