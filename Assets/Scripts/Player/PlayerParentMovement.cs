using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerParentMovement : CatmullWalker
{
    [Header("Dash")]
    [Tooltip("Speed percentage increase forward")]

    [Header("Movement")]
    [SerializeField] private float m_SpeedIncreasePercent = 0.5f;
    [SerializeField] private bool m_IsIndependentMovement = false;
    [Tooltip("Duration of the dash")]
    [SerializeField] private float m_DashDuration = 2.5f;

    public override void TryMove()
    {
        if (m_IsIndependentMovement)
        {
            transform.transform.Translate(Vector3.forward * m_Speed * Time.deltaTime);
            return;
        }
        base.TryMove();
    }

    override protected void Start()
    {
        base.Start(); 
    }

    public IEnumerator Dash(System.Action callback)
    {
        float currDuration = 0f;

        m_CurrSpeed += m_CurrSpeed * m_SpeedIncreasePercent;
        while (currDuration < m_DashDuration)
        {
            currDuration += Time.deltaTime;
            yield return null;
        }

        m_CurrSpeed = m_Speed;
        callback();
    }

    public float DashDuration { get { return m_DashDuration; } }
}
