using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    [SerializeField] private float m_RotationAmount = 2f;
    private PlayerController m_PlayerController; 
    private Rigidbody m_Rigidbody;

    private void Start()
    {
        m_PlayerController = PlayerManager.PropertyInstance.PlayerController;
        m_Rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public void LookTowardPlayer()
    {
        Vector3 DirectionToLook = m_PlayerController.transform.position - transform.position;
        m_Rigidbody.angularVelocity = Vector3.Cross(transform.forward, DirectionToLook).normalized * m_RotationAmount;
    }
}
