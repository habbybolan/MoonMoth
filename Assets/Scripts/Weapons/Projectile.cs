using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected float m_Speed = 100f;
    [SerializeField] protected float m_DamageAmount = 5f;
    [SerializeField] protected Rigidbody m_rigidBody;
    [SerializeField] protected DamageInfo.HIT_EFFECT m_HitEffect = DamageInfo.HIT_EFFECT.NORMAL;

    protected GameObject m_Owner;

    // Start is called before the first frame update
    void Start()
    {
        m_rigidBody.velocity = m_Speed * transform.forward;
    }
     
    protected virtual void CollisionPoint(Collider collider)  
    {
        // For overriding functionality in children classes
        // - ex) radius explosion
    }
     
    protected virtual void ApplyHit(Collider collider)
    {
        Health health = collider.gameObject.GetComponent<Health>();
        if (health != null)
        {
            DamageInfo damageInfo = new DamageInfo(m_DamageAmount, m_Owner, health.gameObject, DamageInfo.DAMAGE_TYPE.PROJECTILE, m_HitEffect);
            health.Damage(damageInfo);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Health health = other.gameObject.GetComponent<Health>();
        if (health != null)
        {
            DamageInfo damageInfo = new DamageInfo(m_DamageAmount, m_Owner, health.gameObject, DamageInfo.DAMAGE_TYPE.PROJECTILE, m_HitEffect);
            health.Damage(damageInfo);
            CollisionPoint(other);
            Destroy(gameObject);
            return;
        }

        Terrain terrain = other.gameObject.GetComponent<Terrain>();
        if (terrain != null)
        {
            CollisionPoint(other);
            Destroy(gameObject);
        }     
    }

    public GameObject Owner 
    { 
        get { return m_Owner; }
        set { m_Owner = value; }
    }
}
