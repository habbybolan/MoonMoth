using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTarget : CharacterController<TutorialTargetHealth>
{
    public override void Death()
    {
        Destroy(gameObject);
    }

    protected override void ApplyEffect(DamageInfo.HIT_EFFECT effect)
    {
        // Do nothing
    }
}
