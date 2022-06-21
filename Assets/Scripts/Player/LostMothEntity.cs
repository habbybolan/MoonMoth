using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class LostMothEntity : MonoBehaviour
{
    [SerializeField] private float m_DurationInDirection = .4f;
    [SerializeField] private float m_Speed = .4f;

    private bool m_IsGoingUp = false;

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

    public void EndOfFlap()
    {
        m_IsGoingUp = !m_IsGoingUp;
    }
}
