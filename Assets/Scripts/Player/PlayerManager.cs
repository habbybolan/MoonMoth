using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{ 
    [SerializeField] private PlayerController m_PlayerController;

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

    public PlayerController PlayerController
    {
        get { return m_PlayerController; }
    }
}
