namespace _Project.Application.Settings
{
    public interface ISettingsStorage
    {
        float GetVolume(AudioMixerParameters parameter);
        void SetVolume(AudioMixerParameters parameter, float volume);
        void Save();
    }
}
