using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private PlayerController m_PlayerController;
    [SerializeField] private float m_InvincibilityDuration = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        Terrain terrain = collision.gameObject.GetComponent<Terrain>();
        if (terrain != null)
        {
            m_PlayerController.OnTerrainCollision(collision.contacts[0]);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Obstacle obstacle = collision.gameObject.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            m_PlayerController.OnObstacleCollision(collision.contacts[0]);
        }
    }

    // Deals with invincibility frames after colliding with an obstacle
    public IEnumerator ObstacleCollision(System.Action callback) 
    {
        float currDuration = 0f;
        while (currDuration < m_InvincibilityDuration)
        {
            currDuration += Time.deltaTime;
            yield return null;
        }
        callback();
    }
}
