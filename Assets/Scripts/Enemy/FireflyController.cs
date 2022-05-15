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

    private FIREFLY_STATE m_State;

    private void Start()
    {
        m_State = FIREFLY_STATE.WAITING;
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

    enum FIREFLY_STATE
    {
        WAITING,
        ACTIVE
    }
}
