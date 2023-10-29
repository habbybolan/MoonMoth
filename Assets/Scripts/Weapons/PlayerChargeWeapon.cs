using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChargeWeapon : WeaponBase<HomingProjectile>
{
    [SerializeField] private float m_ShootCastRadius = 10f;
    [SerializeField] private float m_MaxChargeTime = 2;
    [SerializeField] private ParticleSystem m_ChargeParticle;

    private PlayerController m_Controller;      // Player Controller
    private int m_TargetableLayerMask;

    private bool m_IsCharging = false;
    private float m_CurrChargeTime = -1;

    private void Start()
    {
        m_Controller = PlayerManager.PropertyInstance.PlayerController;
        m_TargetableLayerMask = 1 << LayerMask.NameToLayer("Terrain");
        m_TargetableLayerMask = m_TargetableLayerMask | 1 << LayerMask.NameToLayer("Obstacle");
        m_TargetableLayerMask = m_TargetableLayerMask | 1 << LayerMask.NameToLayer("Enemy");

        m_ChargeParticle.Stop();
    }

    private void Update()
    {
        if (m_IsCharging & !isCooldown)
        {
            m_CurrChargeTime++;
            m_CurrChargeTime = Mathf.Min(m_MaxChargeTime, m_CurrChargeTime);
        }
    }

    public void TryShoot()
    {
        if (!m_IsCharging) return;

        TryStopCharging();

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

    protected override void OnCooldownEnded()
    {
        base.OnCooldownEnded();

        if (m_IsCharging)
        {
            TryStartCharging();
        }
    }

    public void TryStartCharging()
    {
        m_IsCharging = true;
        if (!isCooldown)
        {
            m_CurrChargeTime = 0;
            m_ChargeParticle.Play();
        }
    }

    public void TryStopCharging()
    {
        if (m_IsCharging)
        {
            m_CurrChargeTime = -1;
            m_IsCharging = false;
            m_ChargeParticle.Stop();
        }
    }

    public bool IsCharging
    {
        get { return m_CurrChargeTime > 0; }
        set { if (value) { TryStartCharging(); } else { TryStopCharging(); } }
    }

    public float CurrChargePercentage
    {
        get { return !m_IsCharging ? 0 : m_CurrChargeTime / m_MaxChargeTime; }
    }
}
