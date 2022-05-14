using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        Terrain terrain = collision.gameObject.GetComponent<Terrain>();
        if (terrain != null)
        {
            Vector3 normal = collision.contacts[0].normal;
            Vector3 contactPoint = collision.contacts[0].point;
            Debug.DrawRay(contactPoint, normal, Color.red, 10);
        }
    }
}
