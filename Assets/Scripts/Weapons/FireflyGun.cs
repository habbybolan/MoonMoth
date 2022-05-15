using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyGun : WeaponBase
{
    PlayerController playerController;

    private void Start()
    {
        playerController = PlayerManager.PropertyInstance.PlayerController;
    }
    public void ShootAtPlayer()
    {
        Shoot(playerController.transform.position);
    }    
}
