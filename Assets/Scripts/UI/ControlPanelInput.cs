using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ControlPanelInput : MonoBehaviour
{
    public UnityEvent invokeMethod;

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            invokeMethod.Invoke();
        }
    }
}
