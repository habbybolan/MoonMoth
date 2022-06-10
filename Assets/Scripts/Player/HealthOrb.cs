using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthOrb : HomingProjectile
{
    [Header("Health Orb")]
    [SerializeField] private float m_healthRecovery = 10f;
    [SerializeField] private float m_FlyUpDuration = 1f;
    [SerializeField] private float m_FlyUpSpeed = 3f;

    protected override void Start()
    {
        base.Start();
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
            Destroy(gameObject);
        }
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
