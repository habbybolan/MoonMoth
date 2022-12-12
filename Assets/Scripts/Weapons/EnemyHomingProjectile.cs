using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHomingProjectile : HomingProjectile
{
    protected void FixedUpdate()
    {
        base.FixedUpdate();

        // Apply homing only if projectile is in front of player camera
        if (m_IsHoming && PlayerManager.PropertyInstance.PlayerController.ZDistanceFromPlayerCamera(transform.position) < 0)
        {
            m_IsHoming = false;
        }
    }
}
