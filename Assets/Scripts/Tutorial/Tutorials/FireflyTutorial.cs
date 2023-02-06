using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyTutorial : Tutorial
{
    [SerializeField] private float m_FireflySpawnDelay = 2;
    [SerializeField] private float m_MaxSpawnDistanceX = 50;
    [SerializeField] private float m_MaxSpawnDistanceY = 50;
    [SerializeField] private int m_NumFirefliesToKill = 4;
    [SerializeField] private FireflyContainer m_FireflyPrefab;

    private float m_CurrSpawnDelay = 0;
    private FireflyManager m_FireflyManager;

    private PlayerParentMovement m_PlayerParent;

    private void Start()
    {
        m_PlayerParent = PlayerManager.PropertyInstance.PlayerController.PlayerParent;
    }

    public override void SetupTutorial()
    {
        base.SetupTutorial();

        m_FireflyManager = FireflyManager.PropertyInstance;

        AddChecklistItem("Number of fireflies shot", m_NumFirefliesToKill);
    }

    public override void EndTutorialLogic()
    {
        base.EndTutorialLogic();

        m_FireflyManager.KillAllFireflies();
    }

    private void FixedUpdate()
    {
        if (!IsRunning) return;
        m_CurrSpawnDelay += Time.fixedDeltaTime;
        if (m_CurrSpawnDelay > m_FireflySpawnDelay)
        {
            Vector3 spawnPos = new Vector3(
                m_PlayerParent.transform.position.x + Random.value * m_MaxSpawnDistanceX,
                m_PlayerParent.transform.position.y + Random.value * m_MaxSpawnDistanceY,
                m_PlayerParent.transform.position.z - 25);

            FireflyController spawnedFirefly = m_FireflyManager.SpawnNewFirefly(m_FireflyPrefab, spawnPos);
            spawnedFirefly.Health.d_DeathDelegate += OnFireflyDeath;
            m_CurrSpawnDelay = 0;
        }
    }

    private void OnFireflyDeath()
    {
        UpdateTutorial(0);
    }
}
