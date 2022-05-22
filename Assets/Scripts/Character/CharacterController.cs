using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base controller class that controls any Character (Enemy, player) in the world
// Acts as the hub for the character
public abstract class CharacterController<T> : MonoBehaviour where T : Health 
{
    [SerializeField] protected T m_Health;

    protected virtual void Start()
    {
        
    }

    protected abstract void ApplyEffect(DamageInfo.HIT_EFFECT effect);

    public abstract void Death();
}
