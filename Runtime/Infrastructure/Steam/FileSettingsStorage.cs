using System;
using _Project.Application.Settings;

namespace _Project.Infrastructure.Steam
{
    public class FileSettingsStorage : ISettingsStorage
    {
        private readonly LocalSaveFile _save;

        public FileSettingsStorage(LocalSaveFile save)
        {
            _save = save ?? throw new ArgumentNullException(nameof(save));
        }

        public float GetVolume(AudioMixerParameters parameter)
        {
            return _save.GetFloat(Key(parameter), DefaultVolume(parameter));
        }

        public void SetVolume(AudioMixerParameters parameter, float volume)
        {
            _save.SetFloat(Key(parameter), volume);
        }

        public void Save()
        {
            _save.Save();
        }

        private static float DefaultVolume(AudioMixerParameters parameter)
        {
            switch (parameter)
            {
                case AudioMixerParameters.MusicVolume:
                    return 0.5f;
                default:
                    return 1f;
            }
        }

        private static string Key(AudioMixerParameters parameter)
        {
            switch (parameter)
            {
                case AudioMixerParameters.MusicVolume:
                    return "volume.music";
                case AudioMixerParameters.SfxVolume:
                    return "volume.sfx";
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter));
            }
        }
    }
}
