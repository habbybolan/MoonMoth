using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 * Attached to any object that has health and can be destroyed / Shot at.
 */
public class Health : MonoBehaviour
{
    [SerializeField] protected float m_MaxHealth = 100; 
    [SerializeField] private ParticleSystem m_deathParticles;
    [SerializeField] private float m_deathParticlesDuration = 1f;
     
    [SerializeField] private float m_HealthPercentLosePerSecond = 0f;
    [SerializeField] private bool m_IsInvincible = false;   // for developer use, makes Health script invincible

    protected float m_CurrentHealth;

    public delegate void DeathDelegate();
    public DeathDelegate d_DeathDelegate;

    public delegate void DamageDelegate();
    public DamageDelegate d_DamageDelegate; 

    private GameObject m_LastInstigator;

    public bool IsInvincible { 
        get { return m_IsInvincible; } 
        set { m_IsInvincible = value; }
    }

    public float HealthPercentage => (float)m_CurrentHealth / m_MaxHealth;

    protected virtual void Start()
    {
        m_CurrentHealth = m_MaxHealth;
    }

    public void HealAmount(float healthAmount)
    {
        m_CurrentHealth += healthAmount;
        if (m_CurrentHealth > m_MaxHealth)
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
        if (d_DamageDelegate != null) d_DamageDelegate();

        m_LastInstigator = damageInfo.m_Instigator;
        RemoveHealth(damageInfo.m_DamageAmount);
    }

    private void RemoveHealth(float healthToLose)
    {
        // for developer use
        if (m_IsInvincible)
            return;

        m_CurrentHealth -= healthToLose;
        if (m_CurrentHealth <= 0)
        {
            // If enemy dies, notify PlayerController
            if (gameObject.tag == "Enemy")
            {
                PlayerManager.PropertyInstance.PlayerController.OnEnemyKilled();
            }
            Death();
        }
    }

    protected virtual void Death()
    {
        if (m_deathParticles != null)
        {
            Transform currentTransform = transform;
            Destroy(Instantiate(m_deathParticles, currentTransform.position, currentTransform.rotation), m_deathParticlesDuration);
        }
        d_DeathDelegate();
    }
}
