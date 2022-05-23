using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyHealth : Health 
{
    protected override void Start()
    {
        base.Start();
    }

    public override void Damage(DamageInfo damageInfo)
    {
        base.Damage(damageInfo);
    }
}
