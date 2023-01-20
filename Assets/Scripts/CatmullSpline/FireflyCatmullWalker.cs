using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyCatmullWalker : CatmullWalker
{
    [Tooltip("The offset distance each firefly will have in relation to the previous")]
    [SerializeField] private float m_DistanceOffsetFromPrev = 10f;
    [Tooltip("The base distance the firefly is from the player and the distance from the player the firefly will start moving along spline")]
    [SerializeField] private float m_DistanceFromCamera = 15f; 
    [Tooltip("Speed relative to player for catching up when spawned behind")]
    [Range(1f, 10f)]
    [SerializeField] private float m_BoostSpeed = 1.15f;
    [Tooltip("The speed multiplier to correct the firefly being too far/close to player")]
    [Range(0f, 1f)] 
    [SerializeField] private float m_SpeedCorrection = .1f;
    [SerializeField] private bool m_IsIndependent = false;
     
    private float m_Offset = 0; // Current firefly offset number, 0 if oldest firefly spawned

    override protected void Start()
    {
        // Copy the base movement speed of the player
        m_Speed = PlayerManager.PropertyInstance.PlayerController.PlayerParent.Speed;
        if (!m_IsIndependent)
        {
            m_Spline.IntializeAtEndOfHead();
            base.Start();
        }
    }

    public override void TryMove()
    {
        if (m_IsIndependent)
        {
            transform.Translate(Vector3.forward * -15);
            return;
        }

        base.TryMove();
        
        UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        float zDistanceFromCamera = PlayerManager.PropertyInstance.PlayerController.ZDistanceFromPlayerCamera(transform.position);
        // Firefly to boost in front of player when behind
        if (zDistanceFromCamera < PlayerManager.PropertyInstance.PlayerController.PlayerMovement.CameraOffset - 5)
        {
            m_CurrSpeed = m_Speed * m_BoostSpeed;
            return;
        }
        float targetDistance = TargetDistance();
        // Alter speed of firesly if too far/close to player to keep at contant distance
        float percentToChangeSpeed = (targetDistance - zDistanceFromCamera) * m_SpeedCorrection;
        m_CurrSpeed = m_Speed * percentToChangeSpeed;
    }

    private float TargetDistance()
    {
        return m_DistanceFromCamera + m_Offset * m_DistanceOffsetFromPrev;
    }

    public void Decrementoffset()
    {
        if (m_Offset == 0)
        {
            Debug.LogWarning("Cannot decrement offset when already at 0");
            return;
        }
            
        m_Offset--;
    }

    public float Offset { 
        get { return m_Offset;  } 
        set { m_Offset = value; }
    }
}
