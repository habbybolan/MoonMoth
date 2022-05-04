using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterSpriteFlipper : MonoBehaviour
{
    private CharacterMovement _movement;
    private SpriteRenderer _renderer;

    private void Start()
    {
        _movement = GetComponent<CharacterMovement>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!_movement.HasMoveInput) return;
        _renderer.flipX = _movement.MoveInput.x < 0f;
    }
}
