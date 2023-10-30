using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargedProjectile : HomingProjectile
{
    [Tooltip("Override m_DamageAmount in parent projectile")]
    [SerializeField] protected float m_MinDamage = 1f;
    [SerializeField] protected float m_MaxDamage = 10f;
    
    private float m_ChargePercent = 1;

    public void SetPercent(float chargePercent)
    {
        m_ChargePercent = chargePercent;
    }

    protected override float CalculateDamage()
    {
        return Mathf.Lerp(m_MinDamage, m_MaxDamage, m_ChargePercent);
    }
}
