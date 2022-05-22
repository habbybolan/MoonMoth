using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : CharacterController<SpiderHealth>
{
    [SerializeField] private SpiderWebHealth m_SpiderWebHealth;
    [SerializeField] private SpiderMovement m_SpiderMovement;
    [SerializeField] private SpiderWeapon m_SpiderWeapon;
    [SerializeField] private float m_ShootDistance = 50f;
    
    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (PlayerManager.PropertyInstance.PlayerController.DistanceFromPlayer(transform.position) < m_ShootDistance)
        {
            m_SpiderWeapon.ShootAtPlayer();
        }
    }

    

    public override void Death()
    {
        // TODO:
    }

    protected override void ApplyEffect(DamageInfo.HIT_EFFECT effect)
    {
        // TODO:
    }

    
}
