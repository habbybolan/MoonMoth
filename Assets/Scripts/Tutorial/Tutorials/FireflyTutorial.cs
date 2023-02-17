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

    private const int Group0 = 0;

    private PlayerParentMovement m_PlayerParent;

    private void Start()
    {
        m_PlayerParent = PlayerManager.PropertyInstance.PlayerController.PlayerParent;
    }

    public override void SetupTutorial()
    {
        base.SetupTutorial();

        m_FireflyManager = FireflyManager.PropertyInstance;

#if UNITY_ANDROID && !UNITY_EDITOR
        AddChecklistItem("Defeat fireflies to get health back", m_NumFirefliesToKill, Group0, true, "Hold or press [Fire Button] to shoot at fireflies");
#else
        AddChecklistItem("Defeat fireflies to get health back", m_NumFirefliesToKill, Group0, true, "Hold or press [Right Trigger] to shoot at fireflies");
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        m_PlayerController.FireButton.StartHighlighting();
#endif

        TutorialSetupFinished();
    }

    public override void EndTutorialLogic()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_PlayerController.FireButton.StopHighlighting();
#endif

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
        UpdateTutorial(0, Group0);
    }
}
