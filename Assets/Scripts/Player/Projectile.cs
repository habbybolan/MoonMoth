using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float m_Speed = 100f;
    [SerializeField] protected Rigidbody m_rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        m_rigidBody.velocity = m_Speed * transform.forward;
    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: Do something
        if (other.GetComponent<PlayerMovement>())
            return;
        Destroy(gameObject);
    }
}
