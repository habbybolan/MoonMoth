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

    protected bool m_IsAllInvul;            // If in invulnerabiltiy state to everything but tick damage
    protected bool m_IsProjectileInvuln;    // If in invulnerability state to projectiles only

    public delegate void DeathDelegate();
    public DeathDelegate d_DeathDelegate;

    public delegate void DamageDelegate(DamageInfo damageInfo);
    public DamageDelegate d_DamageDelegate; 

    private GameObject m_LastInstigator;
    private Coroutine m_AllInvulnFramesCoroutine;
    private Coroutine m_ProjectileInulnFramesCoroutine;

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
        // Tick damage cannot be blocked
        if (damageInfo.m_DamageType == DamageInfo.DAMAGE_TYPE.TICK)
        {
            RemoveHealth(damageInfo.m_DamageAmount);
            return;
        }

        // Dont take any damage if Invulnerable
        if (m_IsAllInvul)
            return;

        // Dont take projectile damage if invulnerable to it
        if (m_IsProjectileInvuln && damageInfo.m_DamageType == DamageInfo.DAMAGE_TYPE.PROJECTILE)
            return;

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

    public void SetAllInvulnFrames(float invulnDuration)
    {
        if (m_IsAllInvul) StopCoroutine(m_AllInvulnFramesCoroutine);
        m_AllInvulnFramesCoroutine = StartCoroutine(SetAllInvulnerabilityForDuration(invulnDuration));
    }

    private IEnumerator SetAllInvulnerabilityForDuration(float invulnDuration)
    {
        m_IsAllInvul = true;
        yield return new WaitForSeconds(invulnDuration);
        m_IsAllInvul = false;
    }
    
    public void SetProjectileInvulnFrames(float invulnDuration)
    {
        if (m_IsProjectileInvuln) StopCoroutine(m_ProjectileInulnFramesCoroutine);
        m_ProjectileInulnFramesCoroutine = StartCoroutine(SetProjectileInvulnerabilityForDuration(invulnDuration));
    }

    private IEnumerator SetProjectileInvulnerabilityForDuration(float invulnDuration)
    {
        m_IsProjectileInvuln = true;
        yield return new WaitForSeconds(invulnDuration);
        m_IsProjectileInvuln = false;
    }
}
