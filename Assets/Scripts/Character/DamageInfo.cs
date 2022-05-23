using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageInfo
{
    public float m_DamageAmount;
    public GameObject m_Instigator;
    public GameObject m_Victim;
    public DAMAGE_TYPE m_DamageType;
    public HIT_EFFECT m_HitEffect;

    public DamageInfo(float damageAmount, GameObject instigator, GameObject victim, DAMAGE_TYPE damageType = DAMAGE_TYPE.PROJECTILE, HIT_EFFECT hitEffect = HIT_EFFECT.NORMAL)
    {
        m_DamageAmount = damageAmount;
        m_Instigator = instigator;
        m_Victim = victim;
        m_DamageType = damageType;
        m_HitEffect = hitEffect;
    }

    // Type of damage object that caused damage
    public enum DAMAGE_TYPE
    {
        TERRAIN,
        OBSTACLE,
        PROJECTILE
    }
     
    // special effect damage type has
    public enum HIT_EFFECT
    {
        NORMAL,
        SLOW
    }
}
