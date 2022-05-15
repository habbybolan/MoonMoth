using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private float m_currentHealth = 100;
    [SerializeField] private ParticleSystem m_deathParticles;
    [SerializeField] private float m_deathParticlesDuration = 1f;
    [SerializeField] private UnityEvent m_OnDeathEvent;
     
    [SerializeField] private float m_HealthPercentLosePerSecond = 1f;

    private float m_maxHealth;

    public float HealthPercentage => (float)m_currentHealth / m_maxHealth;

    private void Start()
    {
        m_maxHealth = m_currentHealth;
    }

    public void LosePassiveHealth()
    {
        float healthToLose = GetHealthPercent(m_HealthPercentLosePerSecond);
        SetHealth(healthToLose * Time.deltaTime);
    }

    public void Damage(float amount)
    {
        m_currentHealth -= amount;
        if (m_currentHealth <= 0)
        {
            Death();
        }
    }

    public void Damage(int percent) 
    {
        float healthPercent = GetHealthPercent(percent);
        SetHealth(healthPercent);
    }

    private void SetHealth(float healthToLose)
    {
        m_currentHealth -= healthToLose;
        if (m_currentHealth <= 0)
        {
            Death();
        }
    }

    private float GetHealthPercent(float percent)
    {
        return m_maxHealth * percent;
    }

    private void Death()
    {
        if (m_deathParticles != null)
        {
            Transform currentTransform = transform;
            Destroy(Instantiate(m_deathParticles, currentTransform.position, currentTransform.rotation), m_deathParticlesDuration);
        }
        m_OnDeathEvent?.Invoke();
    }
}
