using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyGun : EnemyWeapon<HomingProjectile>
{
    protected override void Shoot()
    {
        base.Shoot();
        if (m_LastShotProjectile != null)
        {
            m_LastShotProjectile.SetTarget(PlayerManager.PropertyInstance.PlayerController.Health);
        }
    }
}
