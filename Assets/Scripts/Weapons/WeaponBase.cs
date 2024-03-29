using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase<T> : MonoBehaviour where T : Projectile
{
    [SerializeField] protected float m_Cooldown = 0.25f;
    [SerializeField] protected T m_BulletPrefab;
    [SerializeField] private Vector2 m_PitchRange = new Vector2(0.9f, 1.1f); 

    private AudioSource m_ProjectileAudio;

    // Owner that contains the tag (the 'team' they are on)
    [SerializeField] protected Health m_WeaponOwner;

    protected bool isCooldown = false;
    protected T m_LastShotProjectile;   // The projectile shot this frame

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

        m_LastShotProjectile = Instantiate(m_BulletPrefab, transform.position, Quaternion.LookRotation(shootDirection));
        Shoot();
    }

    private void LateUpdate()
    {
        // reset the last shot projectile
        m_LastShotProjectile = null;
    }

    protected virtual void Shoot()
    {
        m_LastShotProjectile.Owner = m_WeaponOwner.gameObject;
        m_LastShotProjectile.Shoot();

        if (m_ProjectileAudio != null)
        {
            m_ProjectileAudio.pitch = Random.Range(m_PitchRange.x, m_PitchRange.y);
            m_ProjectileAudio.Play();
        }

        StartCoroutine(WeaponCooldown());
    }

    IEnumerator WeaponCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(m_Cooldown);
        isCooldown = false;
    }
}
