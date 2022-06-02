using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : WeaponBase<HomingProjectile>
{
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
        if (Physics.SphereCast(crosshairRay, 5f, out hit, Mathf.Infinity, m_AvoidLayerMask))
        {
            ShootPosition(hit.point);
            // if spherecast hits enemy, apply target to Homing projectile
            Health health = hit.collider.gameObject.GetComponent<Health>();
            if (m_LastShotProjectile != null && health != null)
            {
                m_LastShotProjectile.SetTarget(health);
            }
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
