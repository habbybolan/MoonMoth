using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : WeaponBase
{
    [Tooltip("Offset forwards from the crosshair to shoot projectile towards on a missed shot (No Collider hit)")]
    [SerializeField] private float m_ShotMissedOffset = 1000f;

    private PlayerController m_Controller;      // Player Controller
    private bool m_IsShooting = false;
    private LayerMask m_AvoidLayerMask;

    private void Start()
    {
        m_Controller = PlayerManager.PropertyInstance.PlayerController;
        m_AvoidLayerMask = LayerMask.NameToLayer("PlayerController");
        m_AvoidLayerMask = m_AvoidLayerMask | LayerMask.NameToLayer("Player");
        m_AvoidLayerMask = ~m_AvoidLayerMask;
    }

    public void TryShoot()
    {
        if (!m_IsShooting) return;

        Ray crosshairRay = m_Controller.PlayerMovement.CrosshairScreenRay;
        RaycastHit hit;
        if (Physics.Raycast(crosshairRay, out hit, Mathf.Infinity, m_AvoidLayerMask))
        {
            ShootPosition(hit.point);
        }
        else
        {
            ShootDirection(crosshairRay.direction);
        }
    }

    private Vector3 GetDirectionToFireAt()
    {
        Ray crosshairRay = m_Controller.PlayerMovement.CrosshairScreenRay;
        return crosshairRay.direction;
    }

    public bool IsShooting { 
        get { return m_IsShooting; } 
        set { m_IsShooting = value; }
    }
}
