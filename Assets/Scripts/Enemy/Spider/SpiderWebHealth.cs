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
        {
            m_CharacterJoint.connectedBody = null;
            m_SpiderWeb.SpiderWebBroke();
        }
    }
}
