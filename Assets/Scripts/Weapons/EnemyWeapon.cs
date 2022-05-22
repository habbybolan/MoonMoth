using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : WeaponBase
{
    protected PlayerController playerController;

    private void Start()
    {
        playerController = PlayerManager.PropertyInstance.PlayerController;
    }
    public void ShootAtPlayer()
    {
        ShootPosition(playerController.transform.position);
    }
}
