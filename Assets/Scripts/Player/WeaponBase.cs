using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [SerializeField] private float m_Cooldown = 0.25f;
    [SerializeField] private Projectile bullet;

    private bool isCooldown = false;

    void Start()
    {

    }

    void Update()
    {

    }

    public void Shoot(Vector3 fireAtLocation)
    {
        if (isCooldown)
            return;

        Instantiate(bullet, transform.position, Quaternion.LookRotation(fireAtLocation - transform.position));
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
