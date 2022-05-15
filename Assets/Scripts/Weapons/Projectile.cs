using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected float m_Speed = 100f;
    [SerializeField] protected float m_DamageAmount = 5f;
    [SerializeField] protected Rigidbody m_rigidBody;

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
            health.Damage(m_DamageAmount, Health.DAMAGE_TYPE.PROJECTILE);
        }
    }

    public enum PROJECTILE_EFFECTS
    {
        NORMAL,
        SLOW
    }
}
