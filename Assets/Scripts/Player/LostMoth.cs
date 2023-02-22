using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Contrainer for the lost moth. Deals with overall movement and player collecting it.
 */
public class LostMoth : MonoBehaviour
{
    [SerializeField] private float m_DurationInDirection = 1f;
    [SerializeField] private float m_Speed = 1f;

    private bool m_IsGoingUp = true;

    private void Start()
    {
        StartCoroutine(UpDownCoroutine());
    }

    private IEnumerator UpDownCoroutine()
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

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            PlayerManager.PropertyInstance.PlayerController.LostMothCollected();
            transform.gameObject.SetActive(false);
        }
    }
}
