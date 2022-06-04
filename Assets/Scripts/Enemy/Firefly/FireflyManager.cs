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
        StartCoroutine(SpawnFireflyDelay());
    }

    public void SpawnNewFirefly()
    {
        // prevent spawning new fireflies if firefly limit reached
        if (m_FireflyLimit == m_FireflyList.Count)
            return;

        // Spawn new firefly

        FireflyContainer fireflyContainer = Instantiate(m_FireflyPrefab);
        fireflyContainer.FireflyWalker.Offset = m_FireflyList.Count;
        m_FireflyList.Add(fireflyContainer);
    }

    // Coroutine to spawn a firefly after certain amount of time
    IEnumerator SpawnFireflyDelay()
    {
        float currDuration = 0f;

        // Loop infinitely, spawning firefly after amount of time passed
        // TODO: Only start counting again when able to spawn another
        while (true)
        {
            // prevent spawning more fireflies if game not running
            if (GameState.m_GameState != GameStateEnum.RUNNING)
            {
                currDuration = 0;
                yield return null;
            }
            currDuration += Time.deltaTime;
            if (currDuration >= m_DelayToSpawn)
            {
                currDuration = 0f;
                SpawnNewFirefly();
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
                isDeleted = true;
            }
        }
        // TODO:
        //  Update offset positions of all alive fireflies in list
    }

    public void TileSetFinished() 
    {
        for (int i = 0; i < m_FireflyList.Count; i++)
        {
            Destroy(m_FireflyList[i].gameObject);
            m_FireflyList.RemoveAt(i);
        }
    }

    public int FireflyCount { get { return m_FireflyList.Count; } }
}
