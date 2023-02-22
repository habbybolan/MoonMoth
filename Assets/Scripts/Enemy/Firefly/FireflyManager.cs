using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton manager dealing with the creation, management and deletion of all fireflies
public class FireflyManager : MonoBehaviour
{
    [SerializeField] private FireflyContainer m_FireflyPrefab; 
    [SerializeField] private float m_DelayToSpawn = 10f;
    [SerializeField] private int m_FireflyLimit = 3;
    
    private List<FireflyContainer> m_FireflyList;

    static FireflyManager s_PropertyInstance;
    public static FireflyManager PropertyInstance
    {
        get { return s_PropertyInstance; }
    }

    private void Awake()
    {
        // Singleton
        if (s_PropertyInstance != null && s_PropertyInstance != this)
            Destroy(this);
        else
            s_PropertyInstance = this;
    }

    private void Start()
    {
        m_FireflyList = new List<FireflyContainer> ();
        GameState.PropertyInstance.d_GameTransitioningDelegate += TileSetFinished;
        StartCoroutine(SpawnFireflyDelay());
    }

    /*
     * Spawns a firefly that is managed by this manager.
     */
    public FireflyController SpawnNewFirefly()
    {
        return SpawnNewFirefly(m_FireflyPrefab, Vector3.zero);
    }

    /*
     * Spawn a specific prefab firefly type. Utilized mostly by tutorial to spawn tutorial fireflies.
     * @param fireflyPrefab Custom prefab type to spawn
     * @param spawnLocation Location to spawn the parent object. Only matters when in tutorial.
     */
    public FireflyController SpawnNewFirefly(FireflyContainer fireflyPrefab, Vector3 spawnLocation)
    {
        // prevent spawning new fireflies if firefly limit reached
        if (m_FireflyLimit == m_FireflyList.Count) return null;

        // prevent spawning if player not passed first tile
        if (GameState.PropertyInstance.GameStateEnum != GameStateEnum.TUTORIAL &&
            !TileManager.PropertyInstance.IsValidToSpawnFirefly())
        {
            return null;
        }

        // Spawn new firefly
        FireflyContainer fireflyContainer = Instantiate(fireflyPrefab, spawnLocation, Quaternion.identity);
        fireflyContainer.FireflyWalker.Offset = m_FireflyList.Count;
        m_FireflyList.Add(fireflyContainer);

        // TODO: return null if no firefly spawned, otherwise return firefly spawned
        return fireflyContainer.Firefly;
    }

    // Coroutine to spawn a firefly after certain amount of time
    IEnumerator SpawnFireflyDelay()
    {
        float currDuration = 0f;

        // Loop infinitely, spawning firefly after amount of time passed
        while (true)
        {
            // prevent spawning more fireflies if game not running
            if (GameState.PropertyInstance.GameStateEnum != GameStateEnum.RUNNING)
            {
                currDuration = 0;
                yield return null;
            }
            currDuration += Time.deltaTime;
            if (currDuration >= m_DelayToSpawn)
            {
                SpawnNewFirefly();
                currDuration = 0f;
            }
            yield return null;
        }
    }

    public void OnFireflyDeath(GameObject fireflyGameObject)
    {
        bool isDeleted = false;
        for (int i = 0; i < m_FireflyList.Count; i++)
        {
            // Decrement offset of all fireflies after one killed
            if (isDeleted)
                m_FireflyList[i].FireflyWalker.Decrementoffset();

            if (GameObject.ReferenceEquals(m_FireflyList[i].gameObject, fireflyGameObject))
            {
                Destroy(fireflyGameObject);
                m_FireflyList.RemoveAt(i);
                i--;
                isDeleted = true;
            }
        }
    }

    public void KillAllFireflies()
    {
        // TODO: Calling OnFireflyDeath not optimized, but probably doesn't matter
        for (int i = m_FireflyList.Count - 1; i >= 0; i--)
        {
            m_FireflyList[i].Firefly.Health.ManuallyKill();
        }
    }

    public void TileSetFinished() 
    {
        for (int i = m_FireflyList.Count-1; i >= 0; i--)
        {
            Destroy(m_FireflyList[i].gameObject);
            m_FireflyList.RemoveAt(i);
        }
    }

    public int FireflyCount { get { return m_FireflyList.Count; } }
}
