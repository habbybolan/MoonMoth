using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperModes : MonoBehaviour
{

    [SerializeField] private PlayerController m_PlayerController; 
     
    public void ToggleInvinicibility(bool isInvincible)
    {
        m_PlayerController.Health.IsInvincible = isInvincible;
    }

    public void ToggleUnlimitedMoonBar(bool isUnlimitedMoonBar)
    {
        m_PlayerController.MoonBarAbility.IsUnlimitedMoonBar = isUnlimitedMoonBar;
    }

    public void SpawnFirefly()
    {
        FireflyManager.PropertyInstance.SpawnNewFirefly();
    }

    public void ResetWorld()
    {
        m_PlayerController.Death();
    }

    public void WinLevel()
    {
        GameManager.PropertyInstance.OnAllLostMothsCollected();
    }

}
