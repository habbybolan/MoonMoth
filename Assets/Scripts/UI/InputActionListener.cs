using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputActionListener : MonoBehaviour
{
    [SerializeField] private InputActionReference _action;
    [SerializeField] private Button _activateButton;

    public UnityEvent OnPerformed;

    private void OnEnable()
    {
        if (_action != null)
            _action.action.performed += Performed;
    }

    private void OnDisable()
    {
        if (_action != null)
            _action.action.performed -= Performed;
    }

    private void Performed(InputAction.CallbackContext ctx)
    {
        OnPerformed.Invoke();
        _activateButton?.onClick.Invoke();
    }

    // force execution of OnPerformed without input from user
    public void ForcePerform()
    {
        OnPerformed.Invoke();
    }
}
