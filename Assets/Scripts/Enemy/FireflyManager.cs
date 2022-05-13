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
        FireflyContainer fireflyContainer = Instantiate(m_FireflyPrefab);
        fireflyContainer.FireflyWalker.Offset = m_FireflyList.Count;
        m_FireflyList.Add(fireflyContainer);
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

    public void OnFireflyDeath()
    {
        // TODO:
        //  remove firefly from list
        //  Drop light from firefly
        //  Update offset positions of all alive fireflies in list
    }

    public int FireflyCount { get { return m_FireflyList.Count; } }
}
