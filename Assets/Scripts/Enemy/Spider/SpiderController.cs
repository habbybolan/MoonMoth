using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : CharacterController<SpiderHealth>
{
    [SerializeField] private SpiderWebHealth m_SpiderWebHealth;
    [SerializeField] private SpiderMovement m_SpiderMovement;
    
    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        // TODO:
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
