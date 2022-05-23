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
    
    private HEALTH_STATE healthState;       // If the player can be damaged or not by non-terrain damage types

    public delegate void TerrainColisionDelegate(ContactPoint contact);
    public TerrainColisionDelegate terrainCollisionDelegate; 

    protected override void Start()
    {
        healthState = HEALTH_STATE.VULNERABLE;
        base.Start();
        m_HealthText.text = m_MaxHealth.ToString();
    }

    public override void Damage(DamageInfo damageInfo)
    {
        // Dont take any damage if Invulnerable
        if (healthState == HEALTH_STATE.INVULNERABLE)
            return;

        base.Damage(damageInfo);
        if (damageInfo.m_DamageType != DamageInfo.DAMAGE_TYPE.TERRAIN)
        {
            StartCoroutine(InvulnerabilityFrames());
        }
        m_HealthText.text = m_CurrentHealth.ToString();
    }

    // Deals with invincibility frames after colliding with an obstacle
    public IEnumerator InvulnerabilityFrames()
    {
        healthState = HEALTH_STATE.INVULNERABLE;
        yield return new WaitForSeconds(m_InvincibilityDuration);
        healthState = HEALTH_STATE.VULNERABLE;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // TODO: Remove once proper terrain collision added
        // Collision check on Terrain
        Terrain terrain = collision.gameObject.GetComponent<Terrain>();
        if (terrain != null)
        {
            // Deals with movement changes on terrain collision
            terrainCollisionDelegate(collision.contacts[0]);
            Damage(new DamageInfo(m_TerrainDamageAmount, terrain.gameObject, gameObject, DamageInfo.DAMAGE_TYPE.TERRAIN));
        }
    }

    public enum HEALTH_STATE
    {
        VULNERABLE,
        INVULNERABLE // invincibility frames after getting damaged by certain things
    }
}
