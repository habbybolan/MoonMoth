using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Range(0f, 100f)]
    [SerializeField] private float m_DamageAmount = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
        if (playerHealth)
        {
            playerHealth.Damage(new DamageInfo(m_DamageAmount, gameObject, playerHealth.gameObject, DamageInfo.DAMAGE_TYPE.OBSTACLE));
        }
    }
}
