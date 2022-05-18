using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Deals with Firefly's States and which methods to call each frame.
 * Acts as the central hub for interacting with all Firefly components
 */
public class FireflyController : MonoBehaviour
{
    [SerializeField] private FireflyCatmullWalker m_FireflyWalker;
    [SerializeField] private FireflyGun m_Weapon;
    [SerializeField] private Health m_Health;

    private FIREFLY_STATE m_State;

    private void Start()
    {
        m_State = FIREFLY_STATE.WAITING;
        m_Health.deathDelegate = Death;
    }

    private void Update()
    {
        if (m_State == FIREFLY_STATE.WAITING)
            // Check if player is in range of waiting firefly
            if (m_FireflyWalker.IsInRangeOfPlayer())
                m_State = FIREFLY_STATE.ACTIVE;
            else
                return;

        m_Weapon.ShootAtPlayer();
        m_FireflyWalker.TryMove();
    }

    public void Death()
    {
        Debug.Log("Firely killed");
        FireflyManager.PropertyInstance.OnFireflyDeath(gameObject);
        //  TODO:
        //   - Drop light from firefly
    }

    enum FIREFLY_STATE
    {
        WAITING,
        ACTIVE
    }
}
