using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingTutorial : Tutorial
{
    [SerializeField] private TutorialTarget m_TutorialTargetPrefab;
    [SerializeField] private float m_TargetSpawnDelay = 5;
    [SerializeField] private float m_SpawnDistanceForward = 50;
    [SerializeField] private float m_MaxSpawnDistanceX = 50;
    [SerializeField] private float m_MaxSpawnDistanceY = 50;
    [SerializeField] private int m_NumTargetsToDestroy = 4;

    const int Group0 = 0;

    List<TutorialTarget> m_SpawnedTargets = new List<TutorialTarget>();

    private float m_CurrSpawnTime = 0;
    private PlayerParentMovement m_PlayerParent;

    private void Start()
    {
        m_PlayerParent = PlayerManager.PropertyInstance.PlayerController.PlayerParent;
    }

    public override void SetupTutorial()
    {
        base.SetupTutorial();
        
        AddChecklistItem("Targets destroyed", m_NumTargetsToDestroy, Group0, true, "Hold or press right trigger to shoot at targets");

        TutorialSetupFinished();
    }

    private void FixedUpdate()
    {
        if (!IsRunning) return;
        m_CurrSpawnTime += Time.fixedDeltaTime;
        if (m_CurrSpawnTime > m_TargetSpawnDelay)
        {
            SpawnTarget();
            m_CurrSpawnTime = 0;
        }
    }

    private void SpawnTarget()
    {
        Vector3 spawnPos = new Vector3(
            m_PlayerParent.transform.position.x + Random.value * m_MaxSpawnDistanceX,
            m_PlayerParent.transform.position.y + Random.value * m_MaxSpawnDistanceY,
            m_PlayerParent.transform.position.z + m_SpawnDistanceForward);
        TutorialTarget target = Instantiate(m_TutorialTargetPrefab, spawnPos, Quaternion.LookRotation(-1 * m_PlayerParent.RigidBody.velocity));
        target.Health.d_DamageDelegate += OnTargetDestroyed;
        m_SpawnedTargets.Add(target);
    }

    private void OnTargetDestroyed(DamageInfo damageInfo)
    {
        for (int i = 0; i < m_SpawnedTargets.Count; i++)
        {
            if (GameObject.ReferenceEquals(m_SpawnedTargets[i].gameObject, damageInfo.m_Victim))
            {
                m_SpawnedTargets.RemoveAt(i);
                break;
            }
        }
        UpdateTutorial(0, Group0);
    }

    public override void EndTutorialLogic()
    {
        foreach (TutorialTarget target in m_SpawnedTargets)
        {
            Destroy(target.gameObject);
        }
        m_SpawnedTargets.Clear();

        base.EndTutorialLogic();
    }
}
