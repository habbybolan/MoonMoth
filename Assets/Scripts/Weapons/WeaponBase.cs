using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float m_Cooldown = 0.25f;
    [SerializeField] protected Projectile m_BulletPrefab;
    [SerializeField] private Vector2 m_PitchRange = new Vector2(0.9f, 1.1f); 

    private AudioSource m_ProjectileAudio;

    // Owner that contains the tag (the 'team' they are on)
    [SerializeField] protected Health m_WeaponOwner;

    protected bool isCooldown = false;

    private void Awake()
    {
        m_ProjectileAudio = GetComponent<AudioSource>();
    }

    public void ShootPosition(Vector3 fireAtLocation)
    {
        ShootDirection(fireAtLocation - transform.position);
    }

    public void ShootDirection(Vector3 shootDirection)
    {
        if (isCooldown)
            return;

        Projectile projectileShot = Instantiate(m_BulletPrefab, transform.position, Quaternion.LookRotation(shootDirection));
        projectileShot.Owner = m_WeaponOwner.gameObject;

        m_ProjectileAudio.pitch = Random.Range(m_PitchRange.x, m_PitchRange.y);
        m_ProjectileAudio.Play();

        StartCoroutine(WeaponCooldown());
    }

    IEnumerator WeaponCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(m_Cooldown);
        isCooldown = false;
    }
}
