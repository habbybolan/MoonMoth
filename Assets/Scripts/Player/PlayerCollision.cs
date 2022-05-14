using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private PlayerController m_PlayerController;

    private void OnCollisionEnter(Collision collision)
    {
        Terrain terrain = collision.gameObject.GetComponent<Terrain>();
        if (terrain != null)
        {
            m_PlayerController.OnTerrainCollision(collision.contacts[0]);
        }
    }
}
