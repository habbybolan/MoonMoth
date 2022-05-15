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
     
    private int m_Offset = 0; // Current firefly offset number, 0 if oldest firefly spawned

    override protected void Start()
    {
        // Copy the base movement speed of the player
        m_Speed = PlayerManager.PropertyInstance.PlayerController.PlayerParent.Speed;
        m_Spline.InitializeSplineAtTile(PlayerManager.PropertyInstance.PlayerController.PlayerParent.spline.GetTileInfront(0));
        base.Start();
    }

    public override void TryMove()
    {
        UpdateSpeed();
        base.TryMove();
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
        Vector3 PlayerPos = PlayerManager.PropertyInstance.PlayerController.PlayerParent.transform.position;
        return Vector3.Distance(PlayerPos, transform.position);
    }

    private float TargetDistance()
    {
        return m_SensingRange + m_Offset * m_DistanceOffsetFromPrev; ;
    }

    public bool IsInRangeOfPlayer()
    {
        return DistanceFromPlayer() < TargetDistance();
    }

    public int Offset { 
        get { return m_Offset;  } 
        set { m_Offset = value; }
    }
}
