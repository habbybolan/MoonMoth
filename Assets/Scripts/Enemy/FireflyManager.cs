using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Singleton manager dealing with the creation, management and deletion of all fireflies
public class FireflyManager : MonoBehaviour
{
     
    private List<FireflyContainer> m_FireflyList;
    [SerializeField] private FireflyContainer m_FireflyPrefab; 

    // TODO: Change to an actual player script, temp CatmullWalker for testing
    [SerializeField] private PlayerCatmullWalker m_Player;
    [SerializeField] private float m_DelayToSpawn = 10f;

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
}
