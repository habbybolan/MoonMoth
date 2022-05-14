using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private int m_currentHealth = 1;
    [SerializeField] private ParticleSystem m_deathParticles;
    [SerializeField] private float m_deathParticlesDuration = 1f;
    [SerializeField] private UnityEvent m_OnDeathEvent;

    private int m_maxHealth;

    public float HealthPercentage => (float)m_currentHealth / m_maxHealth;

    private void Start()
    {
        m_maxHealth = m_currentHealth;
    }

    public void Damage(int amount)
    {
        m_currentHealth -= amount;
        if (m_currentHealth <= 0)
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
        m_OnDeathEvent?.Invoke();
    }
}
