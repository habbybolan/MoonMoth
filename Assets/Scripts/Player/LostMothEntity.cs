using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class LostMothEntity : MonoBehaviour
{
    [SerializeField] private float m_DurationInDirection = .4f;
    [SerializeField] private float m_Speed = .4f;

    private bool m_IsGoingUp = true;

    private void Start()
    {
        StartCoroutine(TempFlapCoroutine());
    }

    // TODO: Delete after implementing with animation
    private IEnumerator TempFlapCoroutine()
    {
        float currDuration = 0;
        while (true)
        {
            currDuration += Time.deltaTime;
            if (currDuration > m_DurationInDirection)
            {
                m_IsGoingUp = !m_IsGoingUp;
                currDuration = 0;
            }

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
            yield return null;
        }
    }

    public void EndOfFlap()
    {
        m_IsGoingUp = !m_IsGoingUp;
    }
}
