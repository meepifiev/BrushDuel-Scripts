using System.Text;
using CodeStage.AntiCheat.Storage;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace _Project.Infrastructure.Steam
{
    public class LocalSaveFile
    {
        private const string FileName = "save.json";
        private const string EncryptionPassword = "brushduel.goltech.save.v1";

        private readonly ObscuredFile _file;

        private JObject _data;

        public LocalSaveFile()
        {
            ObscuredFileSettings settings = new ObscuredFileSettings(
                new EncryptionSettings(EncryptionPassword),
                new DeviceLockSettings());
            _file = new ObscuredFile(FileName, settings);
        }

        public bool HasKey(string key)
        {
            EnsureLoaded();
            return _data.ContainsKey(key);
        }

        public int GetInt(string key, int defaultValue)
        {
            EnsureLoaded();
            return _data.TryGetValue(key, out JToken token) ? token.Value<int>() : defaultValue;
        }

        public void SetInt(string key, int value)
        {
            EnsureLoaded();
            _data[key] = value;
        }

        public float GetFloat(string key, float defaultValue)
        {
            EnsureLoaded();
            return _data.TryGetValue(key, out JToken token) ? token.Value<float>() : defaultValue;
        }

        public void SetFloat(string key, float value)
        {
            EnsureLoaded();
            _data[key] = value;
        }

        public bool GetBool(string key, bool defaultValue)
        {
            EnsureLoaded();
            return _data.TryGetValue(key, out JToken token) ? token.Value<bool>() : defaultValue;
        }

        public void SetBool(string key, bool value)
        {
            EnsureLoaded();
            _data[key] = value;
        }

        public void Save()
        {
            EnsureLoaded();
            _file.WriteAllBytes(Encoding.UTF8.GetBytes(_data.ToString()));
        }

        private void EnsureLoaded()
        {
            if (_data != null)
            {
                return;
            }

            if (_file.FileExists)
            {
                try
                {
                    ObscuredFileReadResult result = ReadSilently();

                    if (result.Success)
                    {
                        _data = JObject.Parse(Encoding.UTF8.GetString(result.Data));
                        return;
                    }

                    Debug.LogWarning("LocalSaveFile: save rejected, starting fresh - " + result);
                }
                catch (System.Exception exception)
                {
                    Debug.LogWarning("LocalSaveFile: failed to read save, starting fresh - " + exception.Message);
                }
            }

            _data = new JObject();
        }

        private ObscuredFileReadResult ReadSilently()
        {
            bool logEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;

            try
            {
                return _file.ReadAllBytes();
            }
            finally
            {
                Debug.unityLogger.logEnabled = logEnabled;
            }
        }

    }
}
