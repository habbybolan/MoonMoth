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

        m_RigidBody = GetComponent<Rigidbody>();
    }

    public override void TryMove()
    {
        if (m_IsIndependentMovement)
        {
            transform.transform.Translate(Vector3.forward * m_Speed);
            return;
        }
        
        bool bSplineInitialized = IsSplineInitialized();
        base.TryMove();

        // If spline was not initialzied before moving, then update control point location
        if (!bSplineInitialized)
        {
            PlayerManager.PropertyInstance.PlayerController.PlayerMovement.ResetPosition();
        }
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
        ResetValues();
    }

    public void ConnectBackToSpline()
    {
        m_IsIndependentMovement = false;
        TryMove();
    }
}
