using System;
using System.Collections.Generic;
using _Project.Application.Configs;
using _Project.Application.Localization;

namespace _Project.Infrastructure.Steam
{
    public class SteamLocalizationService : ILocalizationService, ILanguageSettings
    {
        private const string LanguageKey = "language.russian";

        private readonly LocalizationConfig _config;
        private readonly LocalSaveFile _save;

        private Dictionary<string, string> _texts;
        private bool _russian;
        private bool _built;

        public SteamLocalizationService(LocalizationConfig config, LocalSaveFile save)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _save = save ?? throw new ArgumentNullException(nameof(save));
        }

        public bool Russian
        {
            get
            {
                EnsureBuilt();
                return _russian;
            }
            set
            {
                EnsureBuilt();

                if (_russian == value)
                {
                    return;
                }

                _save.SetBool(LanguageKey, value);
                _save.Save();
                _built = false;
                EnsureBuilt();
            }
        }

        public string Get(string key)
        {
            EnsureBuilt();
            return _texts.TryGetValue(key, out string value) ? value : key;
        }

        public string Localize(string russian, string english)
        {
            EnsureBuilt();
            return _russian ? russian : english;
        }

        private void EnsureBuilt()
        {
            if (_built)
            {
                return;
            }

            _russian = _save.GetBool(LanguageKey, false);
            _texts = new Dictionary<string, string>();

            for (int i = 0; i < _config.Count; i++)
            {
                LocalizationConfig.Entry entry = _config.GetAt(i);
                _texts[entry.Key] = _russian ? entry.Russian : entry.English;
            }

            _built = true;
        }
    }
}
