using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAutoSelect : MonoBehaviour
{
    private void Start()
    {
        if (TryGetComponent(out Button button))
        {
            EventSystem.current.SetSelectedGameObject(null);
            button.Select();
        }
    }

    private void OnEnable()
    {
        if (TryGetComponent(out Button button))
        {
            EventSystem.current.SetSelectedGameObject(null);
            button.Select();
        }
    }
}
