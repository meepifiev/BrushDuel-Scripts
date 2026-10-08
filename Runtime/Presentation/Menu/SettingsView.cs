using System;
using System.Collections.Generic;
using _Project.Application.Localization;
using _Project.Application.SceneManagement;
using _Project.Application.Settings;
using _Project.Presentation.Popups;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Menu
{
    [RequireComponent(typeof(PopupView))]
    public class SettingsView : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _languageDropdown;
        [SerializeField] private TMP_Dropdown _fullscreenDropdown;
        [SerializeField] private TMP_Dropdown _vsyncDropdown;
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private GameObject _languageRow;
        [SerializeField] private bool _allowLanguageChange = true;

        private PopupView _popup;
        private IDisplaySettings _display;
        private ILanguageSettings _language;
        private ILocalizationService _localization;
        private ISceneLoader _sceneLoader;

        private bool _open;
        private bool _russian;
        private bool _fullscreen;
        private bool _vsync;
        private int _resolutionIndex;

        [Inject]
        public void Construct(
            IDisplaySettings display,
            ILanguageSettings language,
            ILocalizationService localization,
            ISceneLoader sceneLoader)
        {
            _display = display ?? throw new ArgumentNullException(nameof(display));
            _language = language ?? throw new ArgumentNullException(nameof(language));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
        }

        private void Awake()
        {
            _popup = GetComponent<PopupView>();
            _languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            _fullscreenDropdown.onValueChanged.AddListener(OnFullscreenChanged);
            _vsyncDropdown.onValueChanged.AddListener(OnVSyncChanged);
            _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        private void OnEnable()
        {
            _popup.Shown += OnShown;
            _popup.Hidden += OnHidden;
        }

        private void OnDisable()
        {
            _popup.Shown -= OnShown;
            _popup.Hidden -= OnHidden;
        }

        private void OnShown()
        {
            _open = true;
            _russian = _language.Russian;
            _fullscreen = _display.FullScreen;
            _vsync = _display.VSync;
            _resolutionIndex = _display.ResolutionIndex;

            if (_languageRow != null)
            {
                _languageRow.SetActive(_allowLanguageChange);
            }

            SetOptions(_languageDropdown, new List<string> { "Русский", "English" }, _russian ? 0 : 1);
            List<string> toggle = new List<string> { _localization.Get("common.on"), _localization.Get("common.off") };
            SetOptions(_fullscreenDropdown, toggle, _fullscreen ? 0 : 1);
            SetOptions(_vsyncDropdown, new List<string>(toggle), _vsync ? 0 : 1);

            List<string> resolutions = new List<string>();

            for (int i = 0; i < _display.ResolutionCount; i++)
            {
                resolutions.Add(_display.ResolutionLabel(i));
            }

            SetOptions(_resolutionDropdown, resolutions, _resolutionIndex);
        }

        private void OnHidden()
        {
            if (_open == false)
            {
                return;
            }

            _open = false;
            _display.Apply(_fullscreen, _vsync, _resolutionIndex);

            if (_allowLanguageChange && _russian != _language.Russian)
            {
                _language.Russian = _russian;
                _sceneLoader.LoadAsync(SceneId.MainMenu).Forget();
            }
        }

        private void OnLanguageChanged(int value)
        {
            _russian = value == 0;
        }

        private void OnFullscreenChanged(int value)
        {
            _fullscreen = value == 0;
        }

        private void OnVSyncChanged(int value)
        {
            _vsync = value == 0;
        }

        private void OnResolutionChanged(int value)
        {
            _resolutionIndex = value;
        }

        private static void SetOptions(TMP_Dropdown dropdown, List<string> options, int value)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.SetValueWithoutNotify(Mathf.Clamp(value, 0, options.Count - 1));
            dropdown.RefreshShownValue();
        }
    }
}
