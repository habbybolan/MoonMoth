using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Deals with Firefly's States and which methods to call each frame.
 * Acts as the central hub for interacting with all Firefly components
 */
public class FireflyController : CharacterController<FireflyHealth>
{
    [SerializeField] private FireflyCatmullWalker m_FireflyWalker;
    [SerializeField] private FireflyGun m_Weapon;
    [SerializeField] private HealthOrb m_HealthOrbPrefab;
    [SerializeField] private float m_DistancePastCameraToStartShooting = 10f;

    private FIREFLY_STATE m_State;

    protected override void Start()
    {
        base.Start();
        m_State = FIREFLY_STATE.WAITING;
    }

    private void Update()
    {
        // only shoot at player if passed a certain distance from camera
        if (PlayerManager.PropertyInstance.PlayerController.ZDistanceFromPlayerCamera(transform.position) > m_DistancePastCameraToStartShooting)
        {
            m_Weapon.ShootAtPlayer();
        }
        
        m_FireflyWalker.TryMove();
    }

    public override void Death()
    {
        
        Instantiate(m_HealthOrbPrefab, m_Health.gameObject.transform.position, Quaternion.identity);
        FireflyManager.PropertyInstance.OnFireflyDeath(transform.parent.gameObject);
    }

    protected override void ApplyEffect(DamageInfo.HIT_EFFECT effect)
    {
        // TODO:
    }

    enum FIREFLY_STATE
    {
        WAITING,
        ACTIVE
    }
}
