using System;
using _Project.Application.Settings;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Audio
{
    public class VolumeSettingsHandler : MonoBehaviour
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private Button _saveButton;

        private DecibelConverter _converter;
        private ISettingsStorage _storage;

        [Inject]
        public void Construct(ISettingsStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        private void Awake()
        {
            _converter = new DecibelConverter();
        }

        private void OnEnable()
        {
            _saveButton.onClick.AddListener(OnSaveClicked);
        }

        private void Start()
        {
            ApplyVolume(AudioMixerParameters.MusicVolume);
            ApplyVolume(AudioMixerParameters.SfxVolume);
        }

        private void OnDisable()
        {
            _saveButton.onClick.RemoveListener(OnSaveClicked);
        }

        private void ApplyVolume(AudioMixerParameters parameter)
        {
            _mixer.SetFloat(parameter.ToString(), _converter.ConvertLinearToDecibels(_storage.GetVolume(parameter)));
        }

        private void SaveVolume(AudioMixerParameters parameter)
        {
            if (_mixer.GetFloat(parameter.ToString(), out float decibels) == false)
            {
                throw new InvalidOperationException(nameof(parameter));
            }

            _storage.SetVolume(parameter, _converter.ConvertDecibelsToLinear(decibels));
        }

        private void OnSaveClicked()
        {
            SaveVolume(AudioMixerParameters.MusicVolume);
            SaveVolume(AudioMixerParameters.SfxVolume);
            _storage.Save();
        }
    }
}
