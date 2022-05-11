using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Singleton manager dealing with the creation, management and deletion of all fireflies
public class FireflyManager : MonoBehaviour
{
    
    private List<Firefly> m_FireflyList;

    // TODO: Change to an actual player script, temp CatmullWalker for testing
    [SerializeField] private CatmullWalker m_Player;

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
        m_FireflyList = new List<Firefly> ();
    }

    public void SpawnNewFirefly()
    {
        // TODO: Spawn firefly 2 tiles infront of player for testing
    }
}
