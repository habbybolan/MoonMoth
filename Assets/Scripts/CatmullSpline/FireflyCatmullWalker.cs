using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyCatmullWalker : CatmullWalker
{
    [Tooltip("The offset distance each firefly will have in relation to the previous")]
    [SerializeField] private float m_DistanceOffsetFromPrev = 10f;
    [Tooltip("The base distance the firefly is from the player and the distance from the player the firefly will start moving along spline")]
    [SerializeField] private float m_SensingRange = 50f;
    [Tooltip("The speed multiplier to correct the firefly being too far/close to player")]
    [Range(0f, 1f)]
    [SerializeField] private float m_speedCorrection = .25f;
     
    private bool m_IsActive;    // If the firefly has been within range of the player and starts moving along spline

    override protected void Start()
    {
        m_IsActive = false;
        // Copy the base movement speed of the player
        m_Speed = PlayerManager.PropertyInstance.PlayerParent.Speed;
        IsFollowSpline = false;
        m_Spline.InitializeSplineAtTile(PlayerManager.PropertyInstance.PlayerParent.spline.GetTileInfront(0));
        base.Start();
    }

    protected override void Update()
    {
        if (!m_IsActive)
        {
            CheckIsInRangeOfPlayer();
            return;
        }
        UpdateSpeed();
        base.Update();
    }

    private void UpdateSpeed()
    { 
        float distFromPlayer = DistanceFromPlayer();
        float targetDistance = TargetDistance();
        // Alter speed of firesly if too far/close to player to keep at contant distance
        float percentToChangeSpeed = (targetDistance - distFromPlayer) * m_speedCorrection;
        m_CurrSpeed = m_Speed + m_Speed * percentToChangeSpeed;
    }

    private float DistanceFromPlayer()
    {
        Vector3 PlayerPos = PlayerManager.PropertyInstance.PlayerParent.transform.position;
        return Vector3.Distance(PlayerPos, transform.position);
    }

    private float TargetDistance()
    {
        return m_SensingRange + (FireflyManager.PropertyInstance.FireflyCount - 1) * m_DistanceOffsetFromPrev; ;
    }

    private void CheckIsInRangeOfPlayer()
    {
        if (DistanceFromPlayer() < TargetDistance())
        {
            m_IsActive = true;
            IsFollowSpline = true;
        }
    }
}
