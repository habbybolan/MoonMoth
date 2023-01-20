using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : WeaponBase<HomingProjectile>
{
    [SerializeField] private float m_ShootCastRadius = 10f;
    private PlayerController m_Controller;      // Player Controller
    private bool m_IsShooting = false;
    private int m_TargetableLayerMask;

    private void Start()
    {
        m_Controller = PlayerManager.PropertyInstance.PlayerController;
        m_TargetableLayerMask = 1 << LayerMask.NameToLayer("Terrain");
        m_TargetableLayerMask = m_TargetableLayerMask | 1 << LayerMask.NameToLayer("Obstacle");
        m_TargetableLayerMask = m_TargetableLayerMask | 1 << LayerMask.NameToLayer("Enemy");
    }

    public void TryShoot()
    {
        if (!m_IsShooting) return;

        Ray crosshairRay = m_Controller.PlayerMovement.CrosshairScreenRay;

        // Check if an enemy is near the crosshair
        int m_EnemyLayerMask = 1 << LayerMask.NameToLayer("Enemy");
        RaycastHit[] hits = Physics.SphereCastAll(crosshairRay, m_ShootCastRadius, Mathf.Infinity, m_EnemyLayerMask);
        if (hits.Length > 0)
        {
            // TODO: Get closest enemy and check if enemy is not obstructed from this weapon?
            ShootPosition(hits[0].point);
            // if spherecast hits enemy, apply target to Homing projectile
            Health health = hits[0].collider.gameObject.GetComponent<Health>();
            if (m_LastShotProjectile != null && health != null)
            {
                m_LastShotProjectile.SetTarget(health);
            }
        } 
        // Otherwise, shoot towards terrain/obstacle, or generally forward
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(crosshairRay, out hit, Mathf.Infinity, m_TargetableLayerMask))
            {
                ShootPosition(hit.point);
            }
            else
            {
                ShootDirection(crosshairRay.direction);
            }
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
