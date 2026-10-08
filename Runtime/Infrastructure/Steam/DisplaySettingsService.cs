using System;
using System.Collections.Generic;
using _Project.Application.Settings;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Infrastructure.Steam
{
    public class DisplaySettingsService : IDisplaySettings, IStartable
    {
        private const string FullScreenKey = "display.fullscreen";
        private const string VSyncKey = "display.vsync";
        private const string WidthKey = "display.width";
        private const string HeightKey = "display.height";

        private readonly LocalSaveFile _save;
        private readonly List<Vector2Int> _resolutions = new List<Vector2Int>();

        public DisplaySettingsService(LocalSaveFile save)
        {
            _save = save ?? throw new ArgumentNullException(nameof(save));
        }

        public bool FullScreen => _save.GetBool(FullScreenKey, true);

        public bool VSync => _save.GetBool(VSyncKey, true);

        public int ResolutionIndex
        {
            get
            {
                BuildResolutionList();
                Vector2Int current = SavedResolution();
                int index = _resolutions.IndexOf(current);
                return index < 0 ? _resolutions.Count - 1 : index;
            }
        }

        public int ResolutionCount
        {
            get
            {
                BuildResolutionList();
                return _resolutions.Count;
            }
        }

        public string ResolutionLabel(int index)
        {
            BuildResolutionList();

            if (index < 0 || index >= _resolutions.Count)
            {
                return string.Empty;
            }

            Vector2Int size = _resolutions[index];
            return size.x + " x " + size.y;
        }

        public void Apply(bool fullScreen, bool vSync, int resolutionIndex)
        {
            BuildResolutionList();
            Vector2Int size = resolutionIndex >= 0 && resolutionIndex < _resolutions.Count
                ? _resolutions[resolutionIndex]
                : SavedResolution();

            _save.SetBool(FullScreenKey, fullScreen);
            _save.SetBool(VSyncKey, vSync);
            _save.SetInt(WidthKey, size.x);
            _save.SetInt(HeightKey, size.y);
            _save.Save();

            Screen.SetResolution(size.x, size.y, fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            QualitySettings.vSyncCount = vSync ? 1 : 0;
        }

        public void Start()
        {
            if (UnityEngine.Application.isBatchMode)
            {
                return;
            }

            BuildResolutionList();
            Vector2Int resolution = SavedResolution();
            Screen.SetResolution(resolution.x, resolution.y, FullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            QualitySettings.vSyncCount = VSync ? 1 : 0;
        }

        private Vector2Int SavedResolution()
        {
            int width = _save.GetInt(WidthKey, Display.main.systemWidth);
            int height = _save.GetInt(HeightKey, Display.main.systemHeight);
            return new Vector2Int(width, height);
        }

        private void BuildResolutionList()
        {
            if (_resolutions.Count > 0)
            {
                return;
            }

            foreach (Resolution resolution in Screen.resolutions)
            {
                Vector2Int size = new Vector2Int(resolution.width, resolution.height);

                if (_resolutions.Contains(size) == false)
                {
                    _resolutions.Add(size);
                }
            }

            if (_resolutions.Count == 0)
            {
                _resolutions.Add(new Vector2Int(Display.main.systemWidth, Display.main.systemHeight));
            }
        }
    }
}
