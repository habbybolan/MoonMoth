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
    [SerializeField] private AudioSource m_ExplosionSound;

    public ParticleSystem m_MushroomExplosionParticle;

    private LayerMask m_ObstacleLayerMask;
    private STATE m_State = STATE.WAITING;

    private float m_CurrPoisonTime;

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
                StartCoroutine(SensingPlayer());
            }
        } 
    }

    protected void OnEnable()
    {
        m_State = STATE.WAITING;
    }

    IEnumerator SensingPlayer()
    {
        m_State = STATE.SENSING;
        yield return new WaitForSeconds(m_ExplosionTimeAfterSensing);
        StartCoroutine(StartExplosion());
    }

    IEnumerator StartExplosion()
    {
        ParticleSystem explosion = Instantiate(m_MushroomExplosionParticle,transform.position,Quaternion.identity);
        m_ExplosionSound.Play();
        m_CurrPoisonTime = m_PoisonTime;

        m_State = STATE.EXPLODED;
        float currDuration = 0;
        // Keep poison cloud open for a certain duration
        while (currDuration < m_CurrPoisonTime)
        {
            currDuration += Time.deltaTime;

            // Cause damage at rate m_PoisonTickRate
            if (currDuration >= m_PoisonTickRate)
            {
                currDuration -= m_PoisonTickRate;
                m_CurrPoisonTime -= m_PoisonTickRate;

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

        Destroy(explosion);
    }

    void OnDrawGizmosSelected()
    {
        if (m_State == STATE.EXPLODED)
        {
            // Show range of poison cloud once spawned
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, m_PoisonRadiusRange);
        }

        if (m_State == STATE.WAITING)
        {
            // Show sensing range while waiting state
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, m_SensingRadiusRange);
        }

        

    }

    enum STATE 
    { 
        WAITING,
        SENSING,    // within sensing range, not close enough it explode
        EXPLODED    // reached explosion range,  
    }
}
