using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerParentMovement : CatmullWalker
{
    [Header("Dash")]
    [Tooltip("Speed percentage increase forward")]
    [SerializeField] private float m_SpeedIncreasePercent = 0.5f;
    [SerializeField] private bool m_IsIndependentMovement = false;

    private PlayerMovement m_PlayerMovement;

    protected override void Update()
    {
        if (m_IsIndependentMovement)
        {
            transform.transform.Translate(Vector3.forward * m_Speed * Time.deltaTime);
            return;
        }
        base.Update();
    }

    override protected void Start()
    {
        m_PlayerMovement = GetComponentInChildren<PlayerMovement>();
        base.Start(); 
    }

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

    public PlayerMovement playerMovement { 
        get { return m_PlayerMovement; } 
    }
}
