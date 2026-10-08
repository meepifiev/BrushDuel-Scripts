namespace _Project.Presentation.Audio
{
    public interface IAudioService
    {
        void PlayOneShot(AudioAsset asset);
        void PlayMusic(AudioAsset asset);
    }
}