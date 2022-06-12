using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : Projectile
{
    [SerializeField] private float m_MaxHomingAngle = 1f;

    protected Health m_Target;

    protected override void Start()
    {
        base.Start();
    }

    public void SetTarget(Health health)
    {
        m_Target = health;
    }

    private void Update()
    {
        HomeToTarget();
    }

    private void HomeToTarget()
    {
        if (m_Target == null)
            return;

        Vector3 targetPos = m_Target.transform.position;
        Vector3 directionToTarget = targetPos - transform.position;
        Quaternion rotationToTarget = Quaternion.FromToRotation(Vector3.forward, directionToTarget);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationToTarget, m_MaxHomingAngle);
        m_rigidBody.velocity = transform.forward * m_Speed;
    }
}
