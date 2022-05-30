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
        // Only shoot at player if there's a line of sight to them
        RaycastHit hit;
        Vector3 playerPosition = playerController.transform.position;
        if (Physics.Raycast(transform.position, playerPosition - transform.position, out hit))
        {
            if (hit.collider.tag == "Player")
                ShootPosition(playerPosition);
        }
    }
}
