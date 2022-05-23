using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : CharacterController<SpiderHealth>
{
    [SerializeField] private SpiderWebHealth m_SpiderWebHealth;
    [SerializeField] private SpiderMovement m_SpiderMovement;
    [SerializeField] private SpiderWeapon m_SpiderWeapon;
    [SerializeField] private SpiderWeb m_SpiderWeb;
    [SerializeField] private float m_ShootDistance = 50f;

    private int m_TileID;   // ID of the tile the spider spawned on, used for deleting spiders when tile deleted
    
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
        SpiderManager.PropertyInstance.OnSpiderDeath(gameObject);
    }

    protected override void ApplyEffect(DamageInfo.HIT_EFFECT effect)
    {
        // TODO:
    }

    public int TileID
    {
        get { return m_TileID; }
        set { m_TileID = value; }
    }

    public SpiderWeb spiderWeb { get { return m_SpiderWeb; } }
    
}
