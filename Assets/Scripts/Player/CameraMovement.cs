using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float m_CameraFovChangeRate = 2f;
    [SerializeField] private float m_BaseFov = 60f;

    [Header("Camera Shake")]
    [SerializeField] private float m_ShakeDuration = 0.4f;
    [SerializeField] private float m_ShakeIntensity = 1f;

    [Header("Dash")]
    [Tooltip("Amount to zoom while dashing, positive for zooming out")]
    [Range(-30, 30)]
    [SerializeField] private float m_DashFovOffset = 20f;

    [Header("Aim Mode")]
    [Tooltip("Amount to zoom while in aim mode, negative for zooming in")]
    [Range (-30, 30)]
    [SerializeField] private float m_AimModeFovOffset = -20;

    private float m_TargetFov;
    private float m_StartingZ;
    private CinemachineVirtualCamera m_Camera;
    private CinemachineBasicMultiChannelPerlin m_Noise;

    private Coroutine m_ShakeCoroutine;

    private void Start()
    {
        m_StartingZ = transform.localPosition.z;
        m_Camera = GetComponent<CinemachineVirtualCamera>();
        m_Noise = m_Camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        m_Noise.m_AmplitudeGain = 0;
        m_TargetFov = m_BaseFov;
    }

    private void Update()
    {
        // change the FOV to match the targetFov if not already matching
        m_Camera.m_Lens.FieldOfView += (m_TargetFov - m_Camera.m_Lens.FieldOfView) * m_CameraFovChangeRate * Time.deltaTime;
    }

    public void CameraAimModeZoom()
    {
        m_TargetFov = m_BaseFov + m_AimModeFovOffset;
    }

    public void CameraDashZoom()
    {
        m_TargetFov = m_BaseFov + m_DashFovOffset;
    }

    // Reset zoom back to original
    public void ResetZoom()
    {
        m_TargetFov = m_BaseFov;
    }
    
    public void StartCameraShake()
    {
        // if already shaking, kill current coroutine and start another
        if (m_ShakeCoroutine != null)
        {
            StopCoroutine(m_ShakeCoroutine);
        }
        m_ShakeCoroutine = StartCoroutine(CameraShakeDuration());
    }

    // Camera shake for a duration
    private IEnumerator CameraShakeDuration()
    {
        m_Noise.m_AmplitudeGain = m_ShakeIntensity;
        yield return new WaitForSeconds(m_ShakeDuration);
        m_Noise.m_AmplitudeGain = 0;
    }

    public void ResetPosition()
    {
        // wait for the next frame to update position on parent, otherwise parent wont be updated
        StartCoroutine(ResetPositionCoroutine());
    }

    private IEnumerator ResetPositionCoroutine()
    {
        yield return null;
        CinemachineCore.Instance.OnTargetObjectWarped(m_Camera.Follow, (transform.parent.position + transform.parent.forward * m_StartingZ) - transform.position);
    }
}
