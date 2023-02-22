using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLoseScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject LoseUI;
    [SerializeField] private GameObject WinUI; 

    private void Awake()
    {
        if (GameState.PropertyInstance.GameStateEnum == GameStateEnum.WON)
        {
            WinUI.SetActive(true);
        } else
        {
            LoseUI.SetActive(true);
        }
    }
}
