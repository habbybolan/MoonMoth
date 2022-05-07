using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerParentMovement : CinemachineDollyCart
{
    [Tooltip("The base speed to follow the path, determines Speed field")]
    [SerializeField] private float m_BaseSpeed = 10f;

    [Header("Dash")]
    [Tooltip("Speed percentage increase forward")]
    [SerializeField] private float m_SpeedIncreasePercent = 0.5f;

    private void Start()
    {
        m_Speed = m_BaseSpeed;
    }

    private void Update()
    {
        transform.transform.Translate(Vector3.forward * m_Speed * Time.deltaTime);
    }

    public void PerformDash(float duration)
    {
        StartCoroutine(DashSpeedChange(duration));
    }

    IEnumerator DashSpeedChange(float duration)
    {
        float currDuration = 0f;

        m_Speed += m_Speed * m_SpeedIncreasePercent;
        while (currDuration < duration)
        {
            currDuration += Time.deltaTime;
            yield return null;
        }

        m_Speed = m_BaseSpeed;
    }
}
