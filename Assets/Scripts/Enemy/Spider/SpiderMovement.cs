using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    [SerializeField] private float m_RotateSpeed = 50f;
    private PlayerController m_PlayerController;
    private Rigidbody m_Rigidbody;

    private void Start()
    {
        m_PlayerController = PlayerManager.PropertyInstance.PlayerController;
        m_Rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public void LookTowardPlayer()
    {
        Quaternion rot = Quaternion.LookRotation(m_PlayerController.transform.position - transform.position, Vector3.up);
        m_Rigidbody.MoveRotation(rot);
    }
}
