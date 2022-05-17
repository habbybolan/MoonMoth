using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Deals with any object that can interact and damage the player.
 *  - Includes enemy attacks, terrain and obstacles of all kinds
 */
public class PlayerHealth : Health
{
    [SerializeField] private float m_InvincibilityDuration = 2f;
    [Range(0f, 100f)]
    [SerializeField] private float m_TerrainDamageAmount = 5f;
    [Range(0f, 100f)]
    [SerializeField] private float m_ObstacleDamageAmount = 5f; 
    
    private HEALTH_STATE healthState;       // If the player can be damaged or not by non-terrain damage types

    public delegate void TerrainColisionDelegate(ContactPoint contact);
    public TerrainColisionDelegate terrainCollisionDelegate; 

    protected override void Start()
    {
        healthState = HEALTH_STATE.VULNERABLE;
        base.Start();
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
        // Collision check on Terrain
        Terrain terrain = collision.gameObject.GetComponent<Terrain>();
        if (terrain != null)
        {
            // Deals with movement changes on terrain collision
            terrainCollisionDelegate(collision.contacts[0]);
            Damage(new DamageInfo(m_TerrainDamageAmount, terrain.gameObject, gameObject, DamageInfo.DAMAGE_TYPE.TERRAIN));
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Collision check on Obstacle
        Obstacle obstacle = collision.gameObject.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            Damage(new DamageInfo(m_ObstacleDamageAmount, obstacle.gameObject, gameObject, DamageInfo.DAMAGE_TYPE.OBSTACLE));
        }
    }

    public enum HEALTH_STATE
    {
        VULNERABLE,
        INVULNERABLE // invincibility frames after getting damaged by certain things
    }
}
