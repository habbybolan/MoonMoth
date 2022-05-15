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
    [SerializeField] private PlayerController m_PlayerController;
    [Range(0f, 100f)]
    [SerializeField] private float m_TerrainDamageAmount = 5f;
    [Range(0f, 100f)]
    [SerializeField] private float m_ObstacleDamageAmount = 5f; 
    
    private HEALTH_STATE healthState;       // If the player can be damaged or not by non-terrain damage types

    protected override void Start()
    {
        healthState = HEALTH_STATE.VULNERABLE;
        base.Start();
    }

    public override void Damage(float percent, Health.DAMAGE_TYPE damageType = Health.DAMAGE_TYPE.PROJECTILE, Projectile.PROJECTILE_EFFECTS projectileEffects = Projectile.PROJECTILE_EFFECTS.NORMAL)
    {
        // Dont take any damage if Invulnerable
        if (healthState == HEALTH_STATE.INVULNERABLE)
            return;

        Debug.Log("Damage player");
        base.Damage(percent, damageType);
        if (damageType != DAMAGE_TYPE.TERRAIN)
        {
            StartCoroutine(InvulnerabilityFrames());
        }
    }

    // Deals with invincibility frames after colliding with an obstacle
    public IEnumerator InvulnerabilityFrames()
    {
        healthState = HEALTH_STATE.INVULNERABLE;
        float currDuration = 0f;
        while (currDuration < m_InvincibilityDuration)
        {
            currDuration += Time.deltaTime;
            yield return null;
        }
        healthState = HEALTH_STATE.VULNERABLE;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Collision check on Terrain
        Terrain terrain = collision.gameObject.GetComponent<Terrain>();
        if (terrain != null)
        {
            // Deals with movement changes on terrain collision
            m_PlayerController.OnTerrainCollision(collision.contacts[0]);
            Damage(m_TerrainDamageAmount, DAMAGE_TYPE.TERRAIN);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Collision check on Obstacle
        Obstacle obstacle = collision.gameObject.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            Damage(m_ObstacleDamageAmount, DAMAGE_TYPE.OBSTACLE);
        }
    }

    public enum HEALTH_STATE
    {
        VULNERABLE,
        INVULNERABLE // invincibility frames after getting damaged by certain things
    }
}
