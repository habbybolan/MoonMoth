using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParentMovement : MonoBehaviour
{
    [Tooltip("Movement speed forward")]
    [SerializeField] private float m_MovementSpeed = 25f;

    [Header("Dash")]
    [Tooltip("Speed percentage increase forward")]
    [SerializeField] private float m_SpeedIncreasePercent = 0.5f;

    private float m_CurrMovementSpeed;  // Current movement speed forward

    private void Start()
    {
        m_CurrMovementSpeed = m_MovementSpeed;
    }

    void Update()
    {
        transform.transform.Translate(Vector3.forward * m_CurrMovementSpeed * Time.deltaTime);
    }

    public void PerformDash(float duration)
    {
        StartCoroutine(DashSpeedChange(duration));
    }

    IEnumerator DashSpeedChange(float duration)
    {
        float currDuration = 0f;

        m_CurrMovementSpeed += m_CurrMovementSpeed * m_SpeedIncreasePercent;
        while (currDuration < duration)
        {
            currDuration += Time.deltaTime;
            yield return null;
        }

        m_CurrMovementSpeed = m_MovementSpeed;
    }
}
