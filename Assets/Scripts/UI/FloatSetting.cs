using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatSetting : ScriptableObject
{
    [Header("Setting")]
    [SerializeField] private string _name;
    [SerializeField] private float _default = 0f;

    public virtual void SetValue(float newValue)
    {
        PlayerPrefs.SetFloat(_name, newValue);
    }

    public float GetValue()
    {
        return PlayerPrefs.GetFloat(_name, _default);
    }

    public void Load()
    {
        SetValue(GetValue());
    }
}