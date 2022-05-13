using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Singleton manager dealing with the creation, management and deletion of all fireflies
public class FireflyManager : MonoBehaviour
{
    [SerializeField] private FireflyContainer m_FireflyPrefab; 
    [SerializeField] private float m_DelayToSpawn = 10f;
    
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
        m_FireflyList.Add(Instantiate(m_FireflyPrefab));
    }

    // Coroutine to spawn a firefly after certain amount of time
    IEnumerator SpawnFireflyDelay()
    {
        float currDuration = 0f;

        // Loop infinitely, spawning firefly after amount of time passed
        while (true)
        {
            currDuration += Time.deltaTime;
            if (currDuration >= m_DelayToSpawn)
            {
                currDuration = 0f;
                SpawnNewFirefly();
            }
            yield return null;
        }
    }

    public int FireflyCount { get { return m_FireflyList.Count; } }
}
