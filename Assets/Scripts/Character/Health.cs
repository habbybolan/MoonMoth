using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 * Attached to any object that has health and can be destroyed / Shot at.
 */
public abstract class Health : MonoBehaviour
{
    [SerializeField] private float m_MaxHealth = 100; 
    [SerializeField] private ParticleSystem m_deathParticles;
    [SerializeField] private float m_deathParticlesDuration = 1f;
     
    [SerializeField] private float m_HealthPercentLosePerSecond = 1f;

    private float m_CurrentHealth;

    public delegate void DeathDelegate();
    public DeathDelegate d_DeathDelegate; 

    public float HealthPercentage => (float)m_CurrentHealth / m_MaxHealth;

    protected virtual void Start()
    {
        m_CurrentHealth = m_MaxHealth;
    }

    public void LosePassiveHealth()
    {
        RemoveHealth(m_HealthPercentLosePerSecond * Time.deltaTime);
    }

    public virtual void Damage(DamageInfo damageInfo) 
    {
        // prevent all things from hitting themselves
        if (damageInfo.m_Instigator == damageInfo.m_Victim || 
            damageInfo.m_Instigator.tag == damageInfo.m_Victim.tag)
        {
            return;
        }
        RemoveHealth(damageInfo.m_Percent);
    }

    private void RemoveHealth(float healthToLose)
    {
        m_CurrentHealth -= healthToLose;
        if (m_CurrentHealth <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        if (m_deathParticles != null)
        {
            Transform currentTransform = transform;
            Destroy(Instantiate(m_deathParticles, currentTransform.position, currentTransform.rotation), m_deathParticlesDuration);
        }
        d_DeathDelegate();
    }

}
