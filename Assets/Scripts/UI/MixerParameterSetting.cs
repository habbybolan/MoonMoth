using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu]
public class MixerParameterSetting : FloatSetting
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string _parameterName = "Parameter Name";

    public override void SetValue(float newValue)
    {
        base.SetValue(newValue);

        _audioMixer.SetFloat(_parameterName, newValue);
    }
}
