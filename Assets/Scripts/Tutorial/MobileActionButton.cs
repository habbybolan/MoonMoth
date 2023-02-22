using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileActionButton : MonoBehaviour
{
    [SerializeField] private Image m_ActionImage;
    [Tooltip("Image for highlighting the button.")]
    [SerializeField] private Image m_HighlightingImage;

    [Range(0f, 1f)]
    [SerializeField] private float m_MinHighlightOpacity = .1f;
    [Range(0f, 1f)]
    [SerializeField] private float m_MaxHighlightOpacity = .6f;
    [Tooltip("Duration to go between min/max highlight opacity")]
    [SerializeField] private float m_FadeSpeed = 1;

    private float m_CurrHighlightOpacity = 0;
    private float m_CurrHighlightTime = 0;
    private int m_FadeDirection = 1;

    private bool b_IsHighlighting = false;

    private void Start()
    {
        m_HighlightingImage.enabled = false;
    }

    public void StartHighlighting()
    {
        if (b_IsHighlighting) return;

        b_IsHighlighting = true;
        m_HighlightingImage.enabled = true;
        StartCoroutine(HighlightingCoroutine());
    }

    public void StopHighlighting()
    {
        if (b_IsHighlighting == false) return;

        b_IsHighlighting = false;
        StopCoroutine(HighlightingCoroutine());
        m_HighlightingImage.enabled = false;
    }

    private IEnumerator HighlightingCoroutine()
    {
        m_CurrHighlightTime = 0;
        m_FadeDirection = 1;

        while (true)
        {
            m_CurrHighlightTime += Time.deltaTime * m_FadeDirection;

            float opacity = Mathf.Lerp(m_MinHighlightOpacity, m_MaxHighlightOpacity, m_CurrHighlightTime / m_FadeSpeed);
            Color opacityColor = m_HighlightingImage.color;
            opacityColor.a = opacity;
            m_HighlightingImage.color = opacityColor;

            // if reached one of the opacity bounds, flip the direction
            if (m_CurrHighlightTime >= m_FadeSpeed || m_CurrHighlightTime <= 0)
            {
                m_FadeDirection *= -1;
            }

            
            yield return null;
        }
    }
}
