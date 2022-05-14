using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerParentMovement : CatmullWalker
{
    [Header("Dash")]
    [Tooltip("Speed percentage increase forward")]

    [Header("Movement")]
    [SerializeField] private float m_SpeedIncreasePercent = 0.5f;
    [SerializeField] private bool m_IsIndependentMovement = false;
    [Tooltip("Duration of the dash")]
    [SerializeField] private float m_DashDuration = 2.5f;
    
    override protected void Start()
    {
        base.Start(); 
    }

    public override void TryMove()
    {
        if (m_IsIndependentMovement)
        {
            transform.transform.Translate(Vector3.forward * m_Speed * Time.deltaTime);
            return;
        }
        base.TryMove();
    }

    public IEnumerator TerrainCollision(System.Action callback, ContactPoint contact)
    {
        Vector3 forward = -transform.forward;
        // Angle between parent's forward movement and normal of contact
        float degrees = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(forward, contact.normal) / (Vector3.Magnitude(forward) * Vector3.Magnitude(contact.normal)));

        float gravityCoeff = 200f;

        float currVelocity = -m_Speed;
        while (currVelocity < m_Speed)
        {
            Debug.Log(currVelocity);
            currVelocity += gravityCoeff * Time.deltaTime;
            transform.Translate(Vector3.forward * currVelocity * Time.deltaTime);
            yield return null;
        }

        //m_CurrSpeed = -m_CurrSpeed;
        //// go back to the normal movement speed
        //while (m_CurrSpeed < m_Speed)
        //{
        //    m_CurrSpeed += gravityCoeff * Time.deltaTime * Time.deltaTime;
        //    Debug.Log(m_CurrSpeed);
        //    yield return null;
        //}

        //m_CurrSpeed = m_Speed;
        callback();
    }

    public IEnumerator Dash(System.Action callback)
    {
        float currDuration = 0f;

        m_CurrSpeed += m_CurrSpeed * m_SpeedIncreasePercent;
        while (currDuration < m_DashDuration)
        {
            currDuration += Time.deltaTime;
            yield return null;
        }

        m_CurrSpeed = m_Speed;
        callback();
    }

    public float DashDuration { get { return m_DashDuration; } }
}
