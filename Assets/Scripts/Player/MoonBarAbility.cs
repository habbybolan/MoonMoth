using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoonBarAbility : MonoBehaviour
{
    [Header("Aim Mode")]
    [Range(0.1f, 1)]
    [SerializeField] private float m_AimModeTimescaleChange = 0.5f;

    [Header("MoonBar")]
    [SerializeField] private float m_MoonBarPercentLossPerSec = 20f;
    [SerializeField] private float m_MoonBarPercentGainedPerSec = 1f;
    [SerializeField] private float m_MoonBarCooldown = 3f;
    
    [SerializeField] private float m_EnemyPercentBoost = 10f;
    [SerializeField] private int m_MaxNumberOfEnemyBoost = 3;
    [SerializeField] private float m_EnemyBoostDuration = 2f;
    [SerializeField] private Image m_MoonBarReticle;
    [SerializeField] private Image m_MoonBarBorder;
    [Tooltip("How fast the moon bar will flicker when on cooldown")]
    [SerializeField] private float m_BarCooldownFlickerTime = 1f;

    // Moon bar reticle
    private bool m_IsMoonBarCooldown = false;
    private float m_MoonBarCurrPercent = 100f;
    private Coroutine m_MoonBarCoroutine;

    // Delegates for starting and stopping moon bar abilities
    public delegate void AimModeStartDelegate();
    public AimModeStartDelegate d_AimModeStartDelegate;

    public delegate void AimModeEndDelegate();
    public AimModeEndDelegate d_AimModeEndDelegate;

    public delegate void DashStartDelegate(); 
    public DashStartDelegate d_DashStartDelegate;

    public delegate void DashEndDelegate();
    public DashEndDelegate d_DashEndDelegate;

    private bool m_IsUnlimitedMoonBar = false;

    public bool IsUnlimitedMoonBar
    {
        get { return m_IsUnlimitedMoonBar; }
        set { m_IsUnlimitedMoonBar = value; }
    }

    // Aim Mode
    private bool m_IsAimMode = false;

    // Dash
    private bool m_IsDashing = false;

    private LinkedList<float> m_AimModeEnemyKilledList;  // Keeps track of the enemies killed and their remaining time to apply aim mode amount boost 

    private void Awake()
    {
        m_AimModeEnemyKilledList = new LinkedList<float>();
    }

    public void AddEnemyKilled()
    {
        m_AimModeEnemyKilledList.AddLast(m_EnemyBoostDuration);
    }
    

    public void AimModeStartHelper()
    {
        // prevent going into aim mode if on cooldown, or in dash mode
        if (m_IsMoonBarCooldown || m_MoonBarCurrPercent <= 0 || m_IsDashing) return;

        m_IsAimMode = true;
        m_MoonBarCoroutine = StartCoroutine(MoonBarAbilityDuration());
        Time.timeScale = m_AimModeTimescaleChange;
        d_AimModeStartDelegate();
    }

    public void AimModeEndHelper()
    {
        // prevent leaving aim mode if currently not in it
        if (m_MoonBarCoroutine == null || !m_IsAimMode) return;

        m_IsAimMode = false;
        StopCoroutine(m_MoonBarCoroutine);
        Time.timeScale = 1f;
        d_AimModeEndDelegate();
        StartCoroutine(MoonBarCooldown());
    }

    IEnumerator MoonBarCooldown()
    {
        m_IsMoonBarCooldown = true;

        float currDuration = 0;
        Color barColor = m_MoonBarReticle.color;
        Color barColorClear = new Color(barColor.r, barColor.g, barColor.b, 0);
        bool isToClear = true;
        float currFlickerDuration = m_BarCooldownFlickerTime;

        // flicker between clear and base moonbar color
        while (currDuration < m_MoonBarCooldown)
        {
            Color newColor;
            // Check if the moon bar should be flickering to base color or clear
            if (isToClear)
                newColor = Color.Lerp(barColorClear, barColor, currFlickerDuration / m_BarCooldownFlickerTime);
            else
                newColor = Color.Lerp(barColor, barColorClear, currFlickerDuration / m_BarCooldownFlickerTime);

            // Update color of moon bar
            m_MoonBarReticle.color = newColor;
            m_MoonBarBorder.color = newColor;

            currFlickerDuration -= Time.deltaTime;
            // change flicker state if curr state finished
            if (currFlickerDuration < 0)
            {
                currFlickerDuration = m_BarCooldownFlickerTime;
                isToClear = !isToClear;
            }
            currDuration += Time.deltaTime;
            yield return null;
        }

        m_MoonBarReticle.color = barColor;
        m_MoonBarBorder.color = barColor;

        m_IsMoonBarCooldown = false;
    }

    // Start dash, return true if dash was successful
    public void OnDashStartHelper()
    {
        // prevent going into aim mode if on cooldown or in aim mode
        if (m_IsMoonBarCooldown || m_MoonBarCurrPercent <= 0 || m_IsAimMode) return;

        m_IsDashing = true;
        m_MoonBarCoroutine = StartCoroutine(MoonBarAbilityDuration());
        d_DashStartDelegate();
    }

    public void OnDashEndHelper()
    {
        // prevent leaving Dash if currently not in it
        if (m_MoonBarCoroutine == null || !m_IsDashing) return;

        m_IsDashing = false;
        StopCoroutine(m_MoonBarCoroutine);
        d_DashEndDelegate();
        StartCoroutine(MoonBarCooldown());
    }

    IEnumerator MoonBarAbilityDuration()
    {
        // decrement Moon bar as ability is being used
        while (m_MoonBarCurrPercent > 0)
        {
            m_MoonBarCurrPercent -= m_MoonBarPercentLossPerSec * Time.deltaTime;
            if (m_MoonBarCurrPercent < 0) m_MoonBarCurrPercent = 0;
            yield return null;
        }

        // end moon bar ability
        if (m_IsDashing)
            OnDashEndHelper();
        else if (m_IsAimMode)
            AimModeEndHelper();
    }

    // Update each enemy duration for AimMode boost, removing if it hit 0
    public void UpdateAimModeEnemyKilledList()
    {
        if (m_IsUnlimitedMoonBar)
        {
            m_MoonBarCurrPercent = 100;
            return;
        }
        for (LinkedListNode<float> node = m_AimModeEnemyKilledList.First; node != null; node = node.Next)
        {
            node.Value -= Time.deltaTime;
            // remove from list if duraton hit 0
            if (node.Value <= 0) m_AimModeEnemyKilledList.RemoveFirst();
        }
    }

    // update the fill moon reticle meter
    public void UpdateAimModeReticleBar()
    {
        // gain aimMode percent not in aim mode and not maxed out
        if (m_MoonBarCurrPercent < 100 && !m_IsAimMode && !m_IsDashing)
        {
            int enemyBoostCount = m_AimModeEnemyKilledList.Count;
            // Boost by the defaulty amount + the enemy boost amount, scaled by the number of enemies killed recently
            m_MoonBarCurrPercent += (m_MoonBarPercentGainedPerSec + (m_EnemyPercentBoost * Mathf.Min(m_MaxNumberOfEnemyBoost, enemyBoostCount))) * Time.deltaTime;
        }
        m_MoonBarReticle.fillAmount = (m_MoonBarCurrPercent / 100);
    }
}
