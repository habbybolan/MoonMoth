using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageInfo
{
    public float m_Percent;
    public GameObject m_Instigator;
    public GameObject m_Victim;
    public DAMAGE_TYPE m_DamageType;
    public HIT_EFFECT m_HitEffect;

    public DamageInfo(float percent, GameObject instigator, GameObject victim, DAMAGE_TYPE damageType = DAMAGE_TYPE.PROJECTILE, HIT_EFFECT hitEffect = HIT_EFFECT.NORMAL)
    {
        m_Percent = percent;
        m_Instigator = instigator;
        m_Victim = victim;
        m_DamageType = damageType;
        m_HitEffect = hitEffect;
    }


    public enum DAMAGE_TYPE
    {
        TERRAIN,
        OBSTACLE,
        PROJECTILE
    }
     
    public enum HIT_EFFECT
    {
        NORMAL,
        SLOW
    }
}
