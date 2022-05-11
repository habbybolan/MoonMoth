using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firefly : MonoBehaviour
{
    private bool m_IsActive;    // If the firefly has been within range of the player and starts moving along spline

    private void Start()
    {
        m_IsActive = false;
    }

    private void Update()
    {
        // TODO:
        // If not active, check if within range of player
        // otherwise, perform shoot/death/collision logic
    }

    private void CheckIsInRangeOfPlayer()
    {
        // TODO:
        // Used for checking if should activate, or to control the speed of the firefly if too far/close to player
    }
}
