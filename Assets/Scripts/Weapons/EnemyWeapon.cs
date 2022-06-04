using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon<T> : WeaponBase<T> where T : Projectile
{
    protected PlayerController playerController;
    protected LayerMask m_PlayerMask; 

    private void Start()
    {
        playerController = PlayerManager.PropertyInstance.PlayerController;
        m_PlayerMask = LayerMask.GetMask("Player");
    }
    public void ShootAtPlayer()
    {
        // Only shoot at player if there's a line of sight to them
        RaycastHit hit;
        Vector3 playerPosition = playerController.transform.position;
        if (Physics.Raycast(transform.position, playerPosition - transform.position, out hit, Mathf.Infinity, m_PlayerMask))
        {
            if (hit.collider.tag == "Player")
                ShootPosition(playerPosition);
        }
    }
}
