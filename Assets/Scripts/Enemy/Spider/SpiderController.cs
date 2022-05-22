using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : CharacterController<SpiderHealth>
{
    [SerializeField] private SpiderWebHealth m_SpiderWebHealth;
    [SerializeField] private SpiderMovement m_SpiderMovement;

    public override void Death()
    {
        // TODO:
    }

    protected override void ApplyEffect(DamageInfo.HIT_EFFECT effect)
    {
        // TODO:
    }

    private void Start()
    {
        // TODO:
    }

    private void Update()
    {
        // TODO:
    }
}
