using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomObstacle : Obstacle
{
    [SerializeField] private float m_SensingRadiusRange = 50f;  
    [SerializeField] private float m_PoisonRadiusRange = 40f;
    [SerializeField] private float m_PoisonTime = 10f; 
    [SerializeField] private float m_PoisonTickRate = .1f;
    [SerializeField] private float m_ExplosionTimeAfterSensing = 1f;
    [SerializeField] private float m_TickDamageAmount = 1f;

    private bool m_IsWaiting = true;    // If the mushroom has not been activated by the player
    private LayerMask m_ObstacleLayerMask;
    private STATE m_State = STATE.WAITING;

    Coroutine m_SensingPlayer;

    private void Start()
    {
        m_ObstacleLayerMask = 1 << 10;
    }

    private void Update()
    {
        if (m_State == STATE.EXPLODED)
            return;


        // Check if player is in range to activate mushroom exploding
        if (m_State == STATE.WAITING)
        {
            if (PlayerManager.PropertyInstance.PlayerController.DistanceFromPlayer(transform.position) <= m_SensingRadiusRange)
            {
                m_SensingPlayer = StartCoroutine(SensingPlayer());
            }
        } 
    }

    IEnumerator SensingPlayer()
    {
        m_State = STATE.SENSING;
        yield return new WaitForSeconds(m_ExplosionTimeAfterSensing);
        StartCoroutine(StartExplosion());
    }

    IEnumerator StartExplosion()
    {
        // TODO: Start poison cloud particles

        m_State = STATE.EXPLODED;
        float currDuration = 0;
        // Keep poison cloud open for a certain duration
        while (currDuration < m_PoisonTime)
        {
            currDuration += Time.deltaTime;

            // Cause damage at rate m_PoisonTickRate
            if (currDuration >= m_PoisonTickRate)
            {
                currDuration -= m_PoisonTickRate;
                m_PoisonTime -= m_PoisonTickRate;

                // Sphere cast and tick damage everything inside range of mushroom poison cloud
                Collider[] hits = Physics.OverlapSphere(transform.position, m_PoisonRadiusRange);
                foreach (Collider collider in hits)
                {
                    // Apply tick damage to health object
                    Health health = collider.gameObject.GetComponent<Health>();
                    if (health != null)
                    {
                        health.Damage(new DamageInfo(m_TickDamageAmount, gameObject, collider.gameObject, DamageInfo.DAMAGE_TYPE.TICK, DamageInfo.HIT_EFFECT.POISON));
                    }
                }
            }
            yield return null;
        }

        // TODO: End poison cloud particles
    }

    void OnDrawGizmosSelected()
    {
        if (!m_IsWaiting)
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, m_PoisonRadiusRange);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, m_SensingRadiusRange);

    }

    enum STATE 
    { 
        WAITING,
        SENSING,    // within sensing range, not close enough it explode
        EXPLODED    // reached explosion range,  
    }
}
