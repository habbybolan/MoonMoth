using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Singleton fog manager for changes the fog color based on the current level
public class UIManager : MonoBehaviour
{
    [Header("Atmospheric fog")]
    [SerializeField] private Color[] m_FogLevelColors;
    [SerializeField] private Color m_DefaultFogColor = new Color(185, 181, 171, 255);

    [Header("Screen fade")]
    [SerializeField] private Image m_FadeScreen;

    private bool m_IsFadeIn;
    private Coroutine m_FadeInCoroutine;
    private bool m_IsFadeOut;
    private Coroutine m_FadeOutCoroutine;

    static UIManager s_PropertyInstance;
    public static UIManager PropertyInstance
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
        RenderSettings.skybox.SetColor("_Color", color);
    }

    // Fade screen into black
    public void FadeIn(float fadeInDuration)
    {
        // prevent double fading in
        if (m_IsFadeIn) return;

        m_FadeScreen.gameObject.SetActive(true);
        // if currently fading out, cancel and start fading in
        if (m_IsFadeOut)
        {
            StopCoroutine(m_FadeOutCoroutine);
            m_IsFadeOut = false;
        }
        m_FadeInCoroutine = StartCoroutine(FadeInCoroutine(fadeInDuration));
    }

    private IEnumerator FadeInCoroutine(float fadeInDuration)
    {
        m_IsFadeIn = true;
        float elapsedTime = 0.0f; 
        Color startColor = m_FadeScreen.color;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            m_FadeScreen.color = Color.Lerp(startColor,Color.black, (elapsedTime / fadeInDuration));        
            yield return null;
        }
        m_IsFadeIn = false;
    }

    public void FadeOut(float fadeOutDuration) 
    {
        // prevent double fading out
        if (m_IsFadeOut) return;

        // if currently fading in, cancel and start fading out
        if (m_IsFadeIn)
        {
            StopCoroutine(m_FadeInCoroutine);
            m_IsFadeIn = false;
        }
        m_FadeOutCoroutine = StartCoroutine(FadeOutCoroutine(fadeOutDuration));
    }
     
    // fade screen out of black 
    private IEnumerator FadeOutCoroutine(float fadeOutDuration)
    {
        m_IsFadeOut = true;
        float elapsedTime = 0.0f;
        Color startColor = Color.black;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            m_FadeScreen.color = Color.Lerp(startColor, Color.clear, (elapsedTime / fadeOutDuration));
            yield return null;
        }
        m_IsFadeOut = false;
        m_FadeScreen.gameObject.SetActive(false);
    }
}
