using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Health for the spider's web. Once broken, spider falls
public class SpiderWebHealth : Health
{
    [SerializeField] private CharacterJoint m_CharacterJoint;
    private SpiderWeb m_SpiderWeb;

    protected override void Start()
    {
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
        Rigidbody rbFalling = m_CharacterJoint.connectedBody;
        m_CharacterJoint.connectedBody = null;
        Destroy(rbFalling.GetComponent<Joint>());

        m_SpiderWeb.SpiderWebBroke();
    }
}
