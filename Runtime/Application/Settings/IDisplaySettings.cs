namespace _Project.Application.Settings
{
    public interface IDisplaySettings
    {
        bool FullScreen { get; }
        bool VSync { get; }
        int ResolutionIndex { get; }
        int ResolutionCount { get; }

        string ResolutionLabel(int index);

        void Apply(bool fullScreen, bool vSync, int resolutionIndex);
    }
}
