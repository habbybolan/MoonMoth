using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMovement : MonoBehaviour
{
    

    [Tooltip("PlayerMovement component")]
    [SerializeField] private PlayerMovement player;

    [Header("Dash")]
    [Tooltip("The duration for the camera to zoom in/out")]
    [SerializeField] private float m_DashZoomTime = 0.35f;
    [Tooltip("Percentage the camera zooms in relation to its offset to the parent")]
    [Range(0, 1)]
    [SerializeField] private float m_CameraZoomAmount = 0.8f;
    [Range(-30, 30)]
    [SerializeField] private float m_DashFovOffset = 15f;

    [Header("Aim Mode")]
    [Range (-30, 30)]
    [SerializeField] private float m_AimModeFovOffset = -15;

    private float m_BaseFov;                    // starting FOV of the camera
    private CinemachineVirtualCamera m_Camera;

    private void Start()
    {
        m_Camera = GetComponent<CinemachineVirtualCamera>();
        m_BaseFov = m_Camera.m_Lens.FieldOfView;
    }

    public void PerformCameraZoom(float duration)
    {
        StartCoroutine(CameraZoomForDuration(duration));
    }

    IEnumerator CameraZoomForDuration(float duration)
    {
        float currDuration = 0f;
        float zStartingLoc = transform.localPosition.z;

        // Length the zoom takes
        float targetEndZoomTime = m_DashZoomTime;

        while (currDuration < duration)
        {
            LerpToZoomPosition(zStartingLoc, player.CameraOffset / m_CameraZoomAmount, currDuration / targetEndZoomTime);
            currDuration += Time.deltaTime;
            yield return null;
        }

        currDuration = 0f;
        zStartingLoc = transform.localPosition.z;
        while (currDuration < targetEndZoomTime)
        {
            LerpToZoomPosition(zStartingLoc, player.CameraOffset, currDuration / targetEndZoomTime);
            currDuration += Time.deltaTime;
            yield return null;
        }
    }

    private void LerpToZoomPosition(float zStart, float zEnd, float t)
    {
        float zPos = Mathf.Lerp(zStart, zEnd, t);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, zPos);
    }
}
