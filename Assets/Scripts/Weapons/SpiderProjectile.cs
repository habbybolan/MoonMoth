using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderProjectile : Projectile
{
    protected override void ApplyHit(Collision collision)
    {
        Health health = collision.gameObject.GetComponent<Health>();
        if (health != null)
        {
            DamageInfo damageInfo = new DamageInfo(m_DamageAmount, m_Owner, health.gameObject, DamageInfo.DAMAGE_TYPE.PROJECTILE, DamageInfo.HIT_EFFECT.SLOW);
            health.Damage(damageInfo);
        }
    }
}
