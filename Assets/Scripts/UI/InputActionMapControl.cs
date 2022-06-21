using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionMapControl : MonoBehaviour
{
    // current player input component
    [SerializeField] private PlayerInput _playerInput;

    private void Start()
    {
        // find player input if not plugged into inspector
        if (_playerInput == null)
        {
            _playerInput = FindObjectOfType<PlayerInput>();
        }
    }

    public void SetActionMap(string mapName)
    {
        _playerInput.SwitchCurrentActionMap(mapName);
    }
}
