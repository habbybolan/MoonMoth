using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LostMoth : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            PlayerManager.PropertyInstance.PlayerController.LostMothCollected();
            gameObject.SetActive(false);
        }
    }
}
