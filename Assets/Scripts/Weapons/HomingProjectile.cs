using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : Projectile
{
    [SerializeField] protected float m_MaxHomingAngle = 1f;
    [SerializeField] protected float m_DistToIncrHomingAngle = 20;
    [Min(1)]
    [SerializeField] protected float m_MaxHomingAngleInRange = 1f;

    [SerializeField] protected bool m_IsHoming = true;

    protected Health m_Target;

    protected override void Start()
    {
        base.Start();
    }

    protected void FixedUpdate()
    {
        HomeToTarget();
    }

    public void SetTarget(Health health)
    {
        m_Target = health;
    }

    private void HomeToTarget()
    {
        if (m_Target == null || m_IsHoming == false)
            return;

        Vector3 targetPos = m_Target.transform.position;
        Vector3 directionToTarget = targetPos - transform.position;
        Quaternion rotationToTarget = Quaternion.FromToRotation(Vector3.forward, directionToTarget);

        float homingAngle = m_MaxHomingAngle;
        float dist = Vector3.Distance(targetPos, transform.position);
        // check to increase homing amount when closer to target
        if (dist <= m_DistToIncrHomingAngle)
        {
            // Add the extra homing power when within certain range of target
            homingAngle += Mathf.Lerp(m_MaxHomingAngleInRange, 0, dist / m_DistToIncrHomingAngle);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationToTarget, homingAngle);
        m_rigidBody.velocity = transform.forward * m_CurrSpeed;
    }
}
