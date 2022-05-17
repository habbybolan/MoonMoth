using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : WeaponBase
{
    [Tooltip("Offset forwards from the crosshair to shoot projectile towards on a missed shot (No Collider hit)")]
    [SerializeField] private float m_ShotMissedOffset = 1000f;

    private PlayerController m_Controller;      // Player Controller

    private void Start()
    {
        m_Controller = PlayerManager.PropertyInstance.PlayerController;
    }

    public IEnumerator Shooting()
    {
        while (true)
        {
            ShootDirection(GetDirectionToFireAt());
            yield return null;
        }
    }

    private Vector3 GetDirectionToFireAt()
    {
        Ray crosshairRay = m_Controller.PlayerMovement.CrosshairScreenRay;
        return crosshairRay.direction;
    }
}
