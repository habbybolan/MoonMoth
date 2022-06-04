using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerParentMovement : CatmullWalker
{
    [Header("Dash")]
    [Tooltip("Speed percentage increase forward")]
    [SerializeField] private float m_SpeedIncreasePercent = 0.5f;

    [Header("Movement")]
    [SerializeField] private bool m_IsIndependentMovement = false;

    [Header("Slow")]
    [SerializeField] private float m_SlowEffectSpeedDecreasePercent = 0.5f;
    
    public Vector3 GetClosestPointToPlayer()
    {
        return m_Spline.GetClosestPointToCharacter(m_CurrCurve, PlayerManager.PropertyInstance.PlayerController.transform.position);
    }

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

        float currDuration = 0f;
        float duration = 2f;

        m_CurrSpeed = 0;
        while (currDuration < duration)
        {
            m_CurrSpeed = Mathf.Lerp(0, m_Speed, currDuration / duration);
            currDuration += Time.deltaTime;
            yield return null;
        }
        m_CurrSpeed = m_Speed;
        callback();
    }

    public void DashStart() {
        m_CurrSpeed += m_CurrSpeed * m_SpeedIncreasePercent;
    }

    public void DashEnd() {
        m_CurrSpeed = m_Speed;
    }

    public void SLowEffectStart()
    {
        m_CurrSpeed = m_Speed * m_SlowEffectSpeedDecreasePercent;
    }

    public void SlowEffectStop()
    {
        m_CurrSpeed = m_Speed;
    }

    // Disconnect movement from the spline and uninitialize the spline
    public void DisconnectFromSpline()
    {
        m_IsIndependentMovement = true;
        m_Spline.UninitializeSpline();
    }
}
