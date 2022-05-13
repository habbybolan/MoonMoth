using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyCatmullWalker : CatmullWalker
{
    [Tooltip("The offset distance each firefly will have in relation to the previous")]
    [SerializeField] private float m_DistanceOffsetFromPrev = 10f;
    [SerializeField] private float m_SensingRange = 50f;

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
        // TODO: Update the speed if too far/close to player
    }

    private float DistanceFromPlayer()
    {
        Vector3 PlayerPos = PlayerManager.PropertyInstance.PlayerParent.transform.position;
        return Vector3.Distance(PlayerPos, transform.position);
    }

    private void CheckIsInRangeOfPlayer()
    {
        if (DistanceFromPlayer() < 100)
        {
            m_IsActive = true;
            IsFollowSpline = true;
        }
    }
}
