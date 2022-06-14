using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected float m_Speed = 100f;
    [SerializeField] protected float m_DamageAmount = 5f;
    [SerializeField] protected DamageInfo.HIT_EFFECT m_HitEffect = DamageInfo.HIT_EFFECT.NORMAL;
    [SerializeField] protected float m_Duration = 10f;

    protected Rigidbody m_rigidBody;

    protected GameObject m_Owner;

    private void Awake()
    {
        m_rigidBody = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    public void Shoot()
    {
        m_rigidBody.velocity = m_Speed * transform.forward;
        StartCoroutine(durationCoroutine());
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

    protected virtual void OnTriggerEnter(Collider other)
    {
       
        Health health = other.gameObject.GetComponent<Health>();
        if (health != null)
        {
            // prevent instigator from hitting self
            if (m_Owner == health.gameObject) return;

            DamageInfo damageInfo = new DamageInfo(m_DamageAmount, m_Owner, health.gameObject, DamageInfo.DAMAGE_TYPE.PROJECTILE, m_HitEffect);
            health.Damage(damageInfo);
            CollisionPoint(other);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.tag == "Terrain")
        {
            CollisionPoint(other);
            Destroy(gameObject);
        }     
    }

    // destroy projectile after a certain duration of not colliding with anything
    IEnumerator durationCoroutine()
    {
        yield return new WaitForSeconds(m_Duration);
        Destroy(gameObject);
    }

    public GameObject Owner 
    { 
        get { return m_Owner; }
        set { m_Owner = value; }
    }
}
