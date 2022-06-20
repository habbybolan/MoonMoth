using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Health for the spider's web. Once broken, spider falls
public class SpiderWebHealth : Health
{
    private Joint m_Joint;

    protected override void Start()
    {
        m_Joint = GetComponent<Joint>();
    }
    
    public override void Damage(DamageInfo damageInfo) 
    {
        if (damageInfo.m_Instigator.tag == "Player")
            base.Damage(damageInfo);
    }

    protected override void Death()
    {
        Rigidbody rbFalling = m_Joint.connectedBody;
        m_Joint.connectedBody = null;
        Destroy(rbFalling.GetComponent<Joint>());
        base.Death();
    }
}
