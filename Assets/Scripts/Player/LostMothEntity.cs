using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class LostMothEntity : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float m_DurationInDirection = .4f;
    [SerializeField] private float m_Speed = .4f;

    [Header("Light")]
    [SerializeField] private Light m_PointLight;
    [SerializeField] private float m_FlickerDelayMin = .5f;
    [SerializeField] private float m_FlickerDelayMax = 1.5f;
    [SerializeField] private float m_FlickerIntensity = 10f;
    [SerializeField] private float m_FlickerDuration = .4f;
    

    private bool m_IsGoingUp = false;
    private bool m_IsFlickerDelay = false;

    private void Start()
    {
        m_PointLight.intensity = 0;
        StartCoroutine(LightFlickerCoroutine());
    }
    private void Update()
    {
        // fly up
        if (m_IsGoingUp)
        {
            transform.Translate(Vector3.up * m_Speed * Time.deltaTime);
        }
        // fly down
        else
        {
            transform.Translate(Vector3.down * m_Speed * Time.deltaTime);
        }
    }

    IEnumerator LightFlickerCoroutine()
    {
        while (true)
        {
            if (!m_IsFlickerDelay)
            {
                StartCoroutine(PerformLightFlicker());
            }
            yield return null;
        }
    }

    IEnumerator PerformLightFlicker()
    {
        m_IsFlickerDelay = true;
        float currDuration = 0;

        // flicker intensity increase
        while (currDuration < m_FlickerDuration)
        {
            m_PointLight.intensity = Mathf.Lerp(0, m_FlickerIntensity, currDuration / m_FlickerDuration);
            currDuration += Time.deltaTime;
            yield return null;
        }
        // flicker intensity decrease
        currDuration = 0;
        while (currDuration < m_FlickerDuration)
        {
            m_PointLight.intensity = Mathf.Lerp(m_FlickerIntensity, 0, currDuration / m_FlickerDuration);
            currDuration += Time.deltaTime;
            yield return null;
        }

        // wait random duration to perform next flicker
        float rand = Random.Range(m_FlickerDelayMin, m_FlickerDelayMax);
        yield return new WaitForSeconds(rand);
        m_IsFlickerDelay = false;
    }

    public void EndOfFlap()
    {
        m_IsGoingUp = !m_IsGoingUp;
    }
}
