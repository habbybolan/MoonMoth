using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private float m_CameraFovChangeRate = 2f;
    [SerializeField] private float m_BaseFov = 60f;
    [Header("Dash")]
    [Tooltip("Amount to zoom while dashing, positive for zooming out")]
    [Range(-30, 30)]
    [SerializeField] private float m_DashFovOffset = 20f;

    [Header("Aim Mode")]
    [Tooltip("Amount to zoom while in aim mode, negative for zooming in")]
    [Range (-30, 30)]
    [SerializeField] private float m_AimModeFovOffset = -20;

    private float m_TargetFov;
    private CinemachineVirtualCamera m_Camera;

    private void Start()
    {
        m_Camera = GetComponent<CinemachineVirtualCamera>();
        m_TargetFov = m_BaseFov;
    }

    private void Update()
    {
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

    public void ResetZoom()
    {
        m_TargetFov = m_BaseFov;
    }
}
