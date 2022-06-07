using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

/*
 * Deals with any object that can interact and damage the player.
 *  - Includes enemy attacks, terrain and obstacles of all kinds
 */
public class PlayerHealth : Health
{
    [SerializeField] private float m_InvincibilityDuration = 2f;
    [Range(0f, 100f)]
    [SerializeField] private float m_TerrainDamageAmount = 5f;
    [SerializeField] private TextMeshProUGUI m_HealthText;
    

    [Header("Emission")]
    [Tooltip("Moth renderers that contain the Moth emission shader")]
    [SerializeField] private Renderer[] m_EmissionRenderer;
    [Tooltip("Duration that the heal emission burst lasts for")]
    [SerializeField] private float m_HealEmissionBurstDuration = 1.5f;
    [Tooltip("Amount of intensity is temporarily increased during the heal emission burst")]
    [SerializeField] private float m_HealEmissionBurstAmount = 10f;
    [Tooltip("The speed the emission returns back to normal after the heal emission burst")]
    [SerializeField] private float m_ReturnFromHealEmissionBurstSpeed = 10f;

    private HEALTH_STATE healthState;       // If the player can be damaged or not by non-terrain damage types
    private float m_MaxEmissionRGB;
    private float m_RGBDifference;
    private Color m_StartingEmissionColor;

    private bool m_isHealBurst = false;
     
    protected override void Start()
    { 
        healthState = HEALTH_STATE.VULNERABLE;
        base.Start();
        SetHealthText();

        m_StartingEmissionColor = m_EmissionRenderer[0].material.GetColor("_EmissionColor");
        m_MaxEmissionRGB = Mathf.Max(m_StartingEmissionColor.r, m_StartingEmissionColor.g, m_StartingEmissionColor.b); 
    }

    public override void Damage(DamageInfo damageInfo)
    {
        // Tick damage cannot be blocked
        if (damageInfo.m_DamageType == DamageInfo.DAMAGE_TYPE.TICK)
        {
            base.Damage(damageInfo);
            SetHealthText();
            UpdateEmission();
            return;
        }
         
        // Dont take any damage if Invulnerable
        if (healthState == HEALTH_STATE.INVULNERABLE)
            return;

        base.Damage(damageInfo);
        if (damageInfo.m_DamageType != DamageInfo.DAMAGE_TYPE.TERRAIN)
        {
            StartCoroutine(InvulnerabilityFrames());
        }
        SetHealthText();
        UpdateEmission();
    }

    // Scale the emission amount of moth with their health percentage
    private void UpdateEmission()
    {
        // prevent updating emission if heal burst animation playing
        if (m_isHealBurst)
        {
            return;
        }

        foreach (Renderer renderer in m_EmissionRenderer)
        {
            Color currColor = renderer.material.GetColor("_EmissionColor");
            float LerpValueOfMaxRGB = Mathf.Lerp(0, m_MaxEmissionRGB, HealthPercentage);
            float differenceFromMax = m_MaxEmissionRGB - LerpValueOfMaxRGB;
            Color newColor = new Color(GetValidColor(m_StartingEmissionColor.r - differenceFromMax),
                                        GetValidColor(m_StartingEmissionColor.g - differenceFromMax),
                                        GetValidColor(m_StartingEmissionColor.b - differenceFromMax),
                                        currColor.a);
            renderer.material.SetColor("_EmissionColor", newColor);
        }
    }

    private float GetValidColor(float value) 
    {
        return value < 0 ? 0 : value;
    }

    private void SetHealthText()
    {
        m_HealthText.text = Mathf.Floor(m_CurrentHealth).ToString();
    }

    // Deals with invincibility frames after colliding with an obstacle
    public IEnumerator InvulnerabilityFrames()
    {
        healthState = HEALTH_STATE.INVULNERABLE;
        yield return new WaitForSeconds(m_InvincibilityDuration);
        healthState = HEALTH_STATE.VULNERABLE;
    }

    public override void HealAmount(float healthAmount)
    {
        base.HealAmount(healthAmount);
        SetHealthText();
        StartCoroutine(HealEmissionBurst());
    }

    // Moth gains a burst of emission for a duration
    private IEnumerator HealEmissionBurst()
    {
        m_isHealBurst = true;
        float m_HealEmissionBurstDuration = 1.5f;
        float m_HealEmissionBurstAmount = 10f;  

        float currDuration = 0;

        // use first material as others should be the same
        Color ColorBeforeHeal = m_EmissionRenderer[0].material.GetColor("_EmissionColor");

        while (currDuration < m_HealEmissionBurstDuration)
        {
            foreach (Renderer renderer in m_EmissionRenderer)
            {
                Color currColor = renderer.material.GetColor("_EmissionColor");
                float LerpEmitIncr = Mathf.Lerp(0, m_HealEmissionBurstAmount, currDuration / m_HealEmissionBurstDuration);
                Color newColor = new Color(GetValidColor(ColorBeforeHeal.r + LerpEmitIncr),
                                            GetValidColor(ColorBeforeHeal.g + LerpEmitIncr),
                                            GetValidColor(ColorBeforeHeal.b + LerpEmitIncr),
                                            currColor.a);
                renderer.material.SetColor("_EmissionColor", newColor);
            }
            currDuration += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(ReturnFromHealEmissionBurst());
    }
     
    // Moth loses the temporary burst of emission, returning back to normal emission after the heal
    private IEnumerator ReturnFromHealEmissionBurst()
    { 
        bool isNotReachedCorrectEmission = true; 

        while (isNotReachedCorrectEmission)
        {
            foreach (Renderer renderer in m_EmissionRenderer)
            {
                Color currColor = renderer.material.GetColor("_EmissionColor");
                Color newColor = new Color(GetValidColor(currColor.r - m_ReturnFromHealEmissionBurstSpeed * Time.deltaTime),
                                            GetValidColor(currColor.g - m_ReturnFromHealEmissionBurstSpeed * Time.deltaTime),
                                            GetValidColor(currColor.b - m_ReturnFromHealEmissionBurstSpeed * Time.deltaTime),
                                            currColor.a);
                renderer.material.SetColor("_EmissionColor", newColor);

                float correctEmission = Mathf.Lerp(0, m_MaxEmissionRGB, HealthPercentage);
                float currMaxRGB = Mathf.Max(newColor.r, newColor.g, newColor.b);

                // notify the proper emission was reached, leave coroutine after updating all materials
                if (correctEmission > currMaxRGB) isNotReachedCorrectEmission = false;
            }
            yield return null;
        }
        m_isHealBurst = false;
    }

    public enum HEALTH_STATE
    {
        VULNERABLE,
        INVULNERABLE // invincibility frames after getting damaged by certain things
    }
}
