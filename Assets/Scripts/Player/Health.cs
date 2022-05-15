using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 * Attached to any object that has health and can be destroyed.
 */
public abstract class Health : MonoBehaviour
{
    [SerializeField] private float m_MaxHealth = 100; 
    [SerializeField] private ParticleSystem m_deathParticles;
    [SerializeField] private float m_deathParticlesDuration = 1f;
     
    [SerializeField] private float m_HealthPercentLosePerSecond = 1f;

    private float m_CurrentHealth;

    public delegate void DeathDelegate();
    public DeathDelegate deathDelegate;

    public float HealthPercentage => (float)m_CurrentHealth / m_MaxHealth;

    protected virtual void Start()
    {
        m_CurrentHealth = m_MaxHealth;
    }

    public void LosePassiveHealth()
    {
        RemoveHealth(m_HealthPercentLosePerSecond * Time.deltaTime);
    }

    public virtual void Damage(float percent, DAMAGE_TYPE damageType = DAMAGE_TYPE.PROJECTILE, Projectile.PROJECTILE_EFFECTS projectileEffects = Projectile.PROJECTILE_EFFECTS.NORMAL) 
    {   
        RemoveHealth(percent);
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
        deathDelegate();
    }

    public enum DAMAGE_TYPE
    {
        TERRAIN,
        OBSTACLE,
        PROJECTILE
    }
}
