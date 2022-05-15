using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float m_Cooldown = 0.25f;
    [SerializeField] protected Projectile bullet;

    protected bool isCooldown = false;

    public void Shoot(Vector3 fireAtLocation)
    {
        if (isCooldown)
            return;

        Instantiate(bullet, transform.position, Quaternion.LookRotation(fireAtLocation - transform.position));
        StartCoroutine(WeaponCooldown());
    }

    public void ShootDirection(Vector3 shootDirection)
    {
        if (isCooldown)
            return;

        Instantiate(bullet, transform.position, Quaternion.LookRotation(shootDirection));
        StartCoroutine(WeaponCooldown());
    }

    IEnumerator WeaponCooldown()
    {
        isCooldown = true;
        float currCooldownTime = 0f;
        while (currCooldownTime < m_Cooldown)
        {
            currCooldownTime += Time.deltaTime;
            yield return null;
        }
        isCooldown = false;
    }
}
