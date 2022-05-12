using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerParentMovement : CatmullWalker
{
    [Header("Dash")]
    [Tooltip("Speed percentage increase forward")]
    [SerializeField] private float m_SpeedIncreasePercent = 0.5f;

    public void PerformDash(float duration)
    {
        StartCoroutine(DashSpeedChange(duration));
    }

    IEnumerator DashSpeedChange(float duration)
    {
        float currDuration = 0f;

        m_CurrSpeed += m_CurrSpeed * m_SpeedIncreasePercent;
        while (currDuration < duration)
        {
            currDuration += Time.deltaTime;
            yield return null;
        }

        m_CurrSpeed = m_Speed;
    }
}
