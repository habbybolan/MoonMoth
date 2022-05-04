using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SettingSlider : MonoBehaviour
{
    [SerializeField] private FloatSetting _setting;
    
    protected Slider Slider { get; private set; }

    private void Awake()
    {
        Slider = GetComponent<Slider>();
        float savedValue = _setting.GetValue();
        SetValue(savedValue);
    }

    public virtual void SetValue(float value)
    {
        Slider.value = value;
        _setting.SetValue(value);
    }
}
