using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthOrb : HomingProjectile
{
    [Header("Health Orb")]
    [SerializeField] private float m_healthRecovery = 10f;
    [SerializeField] private float m_FlyUpDuration = 1f;
    [SerializeField] private float m_FlyUpSpeed = 3f;

    private ParticleSystem m_TrailParticles;

    protected override void Start()
    {
        base.Start();
        m_TrailParticles = GetComponentInChildren<ParticleSystem>();
        StartCoroutine(GoUp());
        
        // start with no velocity
        m_rigidBody.velocity = Vector3.up * m_FlyUpSpeed;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.HealAmount(m_healthRecovery);
            DetachParticles();
            Destroy(gameObject);
        }
    }

    private void DetachParticles()
    {
        m_TrailParticles.transform.parent = null;

        m_TrailParticles.Stop();

        // This finds the particleAnimator associated with the emitter and then
        // sets it to automatically delete itself when it runs out of particles
        //m_TrailParticles.GetComponent<ParticleAnimator>().autoDestruct = true;
    }

    // Sends the object up for a time
    IEnumerator GoUp()
    {
        float currDuration = 0;
        while (currDuration < m_FlyUpDuration)
        {
            // TODO:
            currDuration += Time.deltaTime;
            yield return null;
        }
        m_Target = PlayerManager.PropertyInstance.PlayerController.Health;
        // look at the player
        transform.rotation = Quaternion.LookRotation(PlayerManager.PropertyInstance.PlayerController.transform.position - transform.position);
        Shoot();
    }
}
