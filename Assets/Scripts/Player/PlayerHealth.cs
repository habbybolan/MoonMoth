using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
 * Deals with any object that can interact and damage the player.
 *  - Includes enemy attacks, terrain and obstacles of all kinds
 */
public class PlayerHealth : Health
{
    [SerializeField] private float m_InvincibilityDuration = 2f;
    [Range(0f, 100f)]
    [SerializeField] private float m_TerrainDamageAmount = 5f;
    [SerializeField] private TextMeshProUGUI m_HealthText;
    [SerializeField] private GameObject m_MothBody; 
    
    private HEALTH_STATE healthState;       // If the player can be damaged or not by non-terrain damage types

    private void Awake()
    {
    }

    protected override void Start()
    { 
        healthState = HEALTH_STATE.VULNERABLE;
        base.Start();
        SetHealthText();
    }

    public override void Damage(DamageInfo damageInfo)
    {
        // Tick damage cannot be blocked
        if (damageInfo.m_DamageType == DamageInfo.DAMAGE_TYPE.TICK)
        {
            base.Damage(damageInfo);
            SetHealthText();
            return;
        }
         
        // Dont take any damage if Invulnerable
        if (healthState == HEALTH_STATE.INVULNERABLE)
            return;

        base.Damage(damageInfo);
        if (damageInfo.m_DamageType != DamageInfo.DAMAGE_TYPE.TERRAIN)
        {
            StartCoroutine(InvulnerabilityFrames());
        }
        SetHealthText();
    }

    private void SetHealthText()
    {
        m_HealthText.text = Mathf.Floor(m_CurrentHealth).ToString();
    }

    // Deals with invincibility frames after colliding with an obstacle
    public IEnumerator InvulnerabilityFrames()
    {
        healthState = HEALTH_STATE.INVULNERABLE;
        yield return new WaitForSeconds(m_InvincibilityDuration);
        healthState = HEALTH_STATE.VULNERABLE;
    }

    public override void HealAmount(float healthAmount)
    {
        base.HealAmount(healthAmount);
        SetHealthText();
    }

    public enum HEALTH_STATE
    {
        VULNERABLE,
        INVULNERABLE // invincibility frames after getting damaged by certain things
    }
}
