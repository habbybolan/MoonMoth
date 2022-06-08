using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Singleton fog manager for changes the fog color based on the current level
public class FogManager : MonoBehaviour
{
    [SerializeField] private Color[] m_FogLevelColors;
    [SerializeField] private Color m_DefaultFogColor = new Color(185, 181, 171, 255);

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

    private void Start()
    {
        // set fog of first level
        NextLevel();
        GameManager.PropertyInstance.d_NextLevelDelegate += NextLevel;
    }

    // Called when player reaches next level, update fog
    public void NextLevel()
    {
        int currLevel = GameManager.PropertyInstance.CurrLevel;
        if (currLevel >= m_FogLevelColors.Length)
            UpdateFog(m_DefaultFogColor);
        else
            UpdateFog(m_FogLevelColors[currLevel]);
    }

    private void UpdateFog(Color color)
    {
        RenderSettings.fogColor = color;
    }
}
