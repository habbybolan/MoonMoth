using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float m_Cooldown = 0.25f;
    [SerializeField] protected Projectile m_BulletPrefab;

    protected bool isCooldown = false;

    public void ShootPosition(Vector3 fireAtLocation)
    {
        ShootDirection(fireAtLocation - transform.position);
    }

    public void ShootDirection(Vector3 shootDirection)
    {
        if (isCooldown)
            return;

        Projectile projectileShot = Instantiate(m_BulletPrefab, transform.position, Quaternion.LookRotation(shootDirection));
        projectileShot.Owner = gameObject;
        StartCoroutine(WeaponCooldown());
    }

    IEnumerator WeaponCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(m_Cooldown);
        isCooldown = false;
    }
}
