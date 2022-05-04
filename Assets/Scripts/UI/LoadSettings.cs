using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSettings : MonoBehaviour
{
    [SerializeField] private List<FloatSetting> _toLoad;

    private void Start()
    {
        foreach (FloatSetting setting in _toLoad)
        {
            float value = setting.GetValue();
            setting.SetValue(value);
        }
    }
}
