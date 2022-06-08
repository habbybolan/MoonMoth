using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

    [Header("Vignette")]
    [SerializeField] private Volume m_PostProcessVolume;
    [Range (0,1)]
    [SerializeField] private float m_BaseVignette = 0.25f;
    [Range(0, 1)]
    [SerializeField] private float m_MaxVignette = .8f;
    [SerializeField] private float m_vignetteSpeed = .1f;

    [Header("Emission")]
    [Tooltip("Moth renderers that contain the Moth emission shader")]
    [SerializeField] private Renderer[] m_EmissionRenderer;
    [Tooltip("Duration that the heal emission burst lasts for")]
    [SerializeField] private float m_HealEmissionBurstDuration = 1.5f;
    [Tooltip("Amount of intensity is temporarily increased during the heal emission burst")]
    [SerializeField] private float m_HealEmissionBurstAmount = 10f;
    [Tooltip("The speed the emission returns back to normal after the heal emission burst")]
    [SerializeField] private float m_EmissionLossSpeed = 10f; 

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

    private void Update()
    {
        UpdateEmission();
        UpdateVignette();
    }

    public override void Damage(DamageInfo damageInfo)
    {
        // Tick damage cannot be blocked
        if (damageInfo.m_DamageType == DamageInfo.DAMAGE_TYPE.TICK)
        {
            base.Damage(damageInfo);
            SetHealthText();
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
    }

    // Scale the emission amount of moth with their health percentage
    private void UpdateEmission()
    {
        // prevent updating emission if heal burst animation playing
        if (m_isHealBurst)
        {
            return;
        }

        // loop over all renderers and update the emission
        foreach (Renderer renderer in m_EmissionRenderer)
        {
            Color currColor = renderer.material.GetColor("_EmissionColor");
            float correctEmission = Mathf.Lerp(0, m_MaxEmissionRGB, HealthPercentage);
            float currMaxRGB = Mathf.Max(currColor.r, currColor.g, currColor.b);

            // if reached the correct emission amount, dont lose any more emission 
            if (correctEmission > currMaxRGB) return; 
                
            // update emission after health loss
            Color newColor = new Color(GetValidColor(currColor.r - m_EmissionLossSpeed * Time.deltaTime),
                                        GetValidColor(currColor.g - m_EmissionLossSpeed * Time.deltaTime),
                                        GetValidColor(currColor.b - m_EmissionLossSpeed * Time.deltaTime),
                                        currColor.a);
            renderer.material.SetColor("_EmissionColor", newColor);
        }
    }

    private void UpdateVignette()
    {
        if (m_PostProcessVolume.profile.TryGet<Vignette>(out var vignette))
        {
            float correctVignette = Mathf.Lerp(m_BaseVignette, m_MaxVignette, 1 - HealthPercentage);

            float vignetteChange = m_vignetteSpeed;
            if (correctVignette < vignette.intensity.value)
                vignetteChange *= -1;

            vignette.intensity.value = vignette.intensity.value + vignetteChange * Time.deltaTime;
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
        m_isHealBurst = false;
    }

    public enum HEALTH_STATE
    {
        VULNERABLE,
        INVULNERABLE // invincibility frames after getting damaged by certain things
    }
}
