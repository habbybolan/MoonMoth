using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected float m_Speed = 100f;
    [SerializeField] protected float m_DamageAmount = 5f;
    [SerializeField] protected Rigidbody m_rigidBody;

    protected GameObject m_Owner;

    // Start is called before the first frame update
    void Start()
    {
        m_rigidBody.velocity = m_Speed * transform.forward;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CollisionPoint(collision);
        ApplyHit(collision);
        Destroy(gameObject);
    }

    protected virtual void CollisionPoint(Collision collision) 
    {
        // For overriding functionality in children classes
        // - ex) radius explosion
    }

    protected virtual void ApplyHit(Collision collision)
    {
        Health health = collision.gameObject.GetComponent<Health>();
        if (health != null)
        {
            DamageInfo damageInfo = new DamageInfo(m_DamageAmount, m_Owner, health.gameObject);
            health.Damage(damageInfo);
        }
    }

    public GameObject Owner 
    { 
        get { return m_Owner; }
        set { m_Owner = value; }
    }
}
