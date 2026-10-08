using System;
using _Project.Application.Settings;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace _Project.Presentation.Audio
{
    [RequireComponent(typeof(Slider))]
    public class VolumeSlider : MonoBehaviour
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerParameters _parameter;

        private Slider _slider;
        private DecibelConverter _converter;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _converter = new DecibelConverter();
        }

        private void OnEnable()
        {
            if (_mixer.GetFloat(_parameter.ToString(), out float decibels) == false)
            {
                throw new InvalidOperationException(nameof(_parameter));
            }

            _slider.SetValueWithoutNotify(Mathf.Lerp(_slider.minValue, _slider.maxValue, _converter.ConvertDecibelsToLinear(decibels)));
            _slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(float value)
        {
            _mixer.SetFloat(_parameter.ToString(), _converter.ConvertLinearToDecibels(_slider.normalizedValue));
        }
    }
}
