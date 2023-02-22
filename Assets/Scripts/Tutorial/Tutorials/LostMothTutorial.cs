using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LostMothTutorial : Tutorial
{
    [SerializeField] private int m_LostMothWinCondition = 4;
    [SerializeField] private float m_LostMothSpawnDelay = 1;
    [SerializeField] private float m_SpawnDistanceForward = 50;
    [SerializeField] private float m_MaxSpawnDistanceX = 18;
    [SerializeField] private float m_MaxSpawnDistanceY = 13;
    [SerializeField] private LostMoth m_LostMothPrefab;

    public int LostMothWinCondition
    {
        get { return m_LostMothWinCondition; }
    }

    private float m_CurrSpawnTime = 0;
    List<LostMoth> m_SpawnedLostMoths = new List<LostMoth>();
    private PlayerParentMovement m_PlayerParent;

    override protected void Start()
    {
        base.Start();
        m_PlayerParent = PlayerManager.PropertyInstance.PlayerController.PlayerParent;
    }

    public override void SetupTutorial()
    {
        base.SetupTutorial();

#if UNITY_ANDROID && !UNITY_EDITOR
        AddChecklistItem("Collect lost moths", m_LostMothWinCondition, 0, true, "Fly over lost moths to collect them. Collecting the specified amount finishes the current level.");
#else
        AddChecklistItem("Collect lost moths", m_LostMothWinCondition, 0, true, "Fly over lost moths to collect them. Collecting the specified amount finishes the current level.");
#endif

        m_PlayerController.lostMothCollectedDelegate += OnLostMothCollected;

        TutorialSetupFinished();
    }

    public override void EndTutorialLogic()
    {
        base.EndTutorialLogic();

        foreach (LostMoth lostMoth in m_SpawnedLostMoths)
        {
            if (lostMoth != null)
            {
                Destroy(lostMoth);
            }
        }
    }

    private void FixedUpdate()
    {
        // Spawn lost moths on a delay
        if (!IsRunning) return;
        m_CurrSpawnTime += Time.fixedDeltaTime;
        if (m_CurrSpawnTime > m_LostMothSpawnDelay)
        {
            SpawnLostMoth();
            m_CurrSpawnTime = 0;
        }
    }

    private void SpawnLostMoth()
    {
        Vector3 spawnPos = new Vector3(
           m_PlayerParent.transform.position.x + Random.value * m_MaxSpawnDistanceX,
           m_PlayerParent.transform.position.y + Random.value * m_MaxSpawnDistanceY,
           m_PlayerParent.transform.position.z + m_SpawnDistanceForward);
        LostMoth lostMoth = Instantiate(m_LostMothPrefab, spawnPos, Quaternion.LookRotation(-1 * m_PlayerParent.RigidBody.velocity));
        m_SpawnedLostMoths.Add(lostMoth);
    }

    private void OnLostMothCollected()
    {
        UpdateTutorial(0, 0);
    }

}
