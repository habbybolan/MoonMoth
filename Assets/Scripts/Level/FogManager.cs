using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Singleton fog manager for changes the fog color based on the current level
public class FogManager : MonoBehaviour
{
    static FogManager s_PropertyInstance;
    public static FogManager PropertyInstance
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


}
