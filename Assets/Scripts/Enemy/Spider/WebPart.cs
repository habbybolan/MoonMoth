using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebPart : MonoBehaviour
{
    [SerializeField] private CharacterJoint m_CharacterJoint;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") && m_CharacterJoint.connectedBody != null)
        {
            m_CharacterJoint.connectedBody = null;
        }
    }
}
