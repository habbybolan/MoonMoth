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

    [SerializeField] protected AudioSource m_DeathSound;
    [SerializeField] protected AudioSource m_DamageSound;  

    protected float m_CurrentHealth;
    protected HEALTH_STATE healthState;       // If the player can be damaged or not by non-terrain damage types

    public delegate void DeathDelegate();
    public DeathDelegate d_DeathDelegate;

    public delegate void DamageDelegate(DamageInfo damageInfo);
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

    public virtual void HealAmount(float healthAmount)
    {
        m_CurrentHealth += healthAmount;
        if (m_CurrentHealth > m_MaxHealth)
            m_CurrentHealth = m_MaxHealth;
    }

    public void LosePassiveHealth()
    {
        Damage(new DamageInfo(m_HealthPercentLosePerSecond * Time.deltaTime, null, null, DamageInfo.DAMAGE_TYPE.TICK));
    }

    public virtual void Damage(DamageInfo damageInfo) 
    {
        // apply damage from no direct source
        if (damageInfo.m_Instigator == null)
        {
            RemoveHealth(damageInfo.m_DamageAmount);
            return;
        }

        // prevent all things from hitting themselves
        if (damageInfo.m_Instigator == damageInfo.m_Victim || 
            damageInfo.m_Instigator.tag == damageInfo.m_Victim.tag)
        {
            return;
        }
        if (d_DamageDelegate != null) d_DamageDelegate(damageInfo);

        m_LastInstigator = damageInfo.m_Instigator;
        RemoveHealth(damageInfo.m_DamageAmount);

        // play damage sound 
        if (m_DamageSound != null && m_CurrentHealth > 0 && damageInfo.m_DamageType != DamageInfo.DAMAGE_TYPE.TICK)
        {
            m_DamageSound.Play();
        }
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
            return;
        }
    }

    protected virtual void Death()
    {
        // play death sound
        if (m_DeathSound != null)
        {
            AudioSource.PlayClipAtPoint(m_DeathSound.clip, transform.position);
        }

        if (m_deathParticles != null)
        {
            Transform currentTransform = transform;
            Destroy(Instantiate(m_deathParticles, currentTransform.position, currentTransform.rotation), m_deathParticlesDuration);
        }
        d_DeathDelegate();
    }

    protected IEnumerator SetInvulnerabilityForDuration(float invulnDuration)
    {
        healthState = HEALTH_STATE.INVULNERABLE;
        yield return new WaitForSeconds(invulnDuration);
        healthState = HEALTH_STATE.VULNERABLE;
    }

    public enum HEALTH_STATE
    {
        VULNERABLE,
        INVULNERABLE // invincibility frames after getting damaged by certain things
    }
}
