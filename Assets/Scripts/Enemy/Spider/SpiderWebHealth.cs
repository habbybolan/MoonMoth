using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Health for the spider's web. Once broken, spider falls
public class SpiderWebHealth : Health
{
    private Joint m_Joint;
    private SpiderWeb m_SpiderWeb;

    protected override void Start()
    {
        m_Joint = GetComponent<Joint>();
        m_SpiderWeb = transform.parent.gameObject.GetComponent<SpiderWeb>();
    }
    
    public override void Damage(DamageInfo damageInfo) 
    {
        if (damageInfo.m_Instigator.tag == "Player")
            base.Damage(damageInfo);
    }

    protected override void Death()
    {
        // manually call Controller
        Rigidbody rbFalling = m_Joint.connectedBody;
        m_Joint.connectedBody = null;
        Destroy(rbFalling.GetComponent<Joint>());

        m_SpiderWeb.SpiderWebBroke();
    }
}
