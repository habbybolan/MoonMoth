using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerParentMovement m_Player;

    static PlayerManager s_PropertyInstance;
    public static PlayerManager PropertyInstance
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

    public PlayerParentMovement Player
    {
        get { return m_Player; }
    }
}
