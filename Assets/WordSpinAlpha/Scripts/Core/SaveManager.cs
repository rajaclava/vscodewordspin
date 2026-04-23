using System;
using System.IO;
using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class SaveManager : Singleton<SaveManager>
    {
        private const float SaveThrottleSeconds = 0.35f;

        private PlayerSaveData _data;
        private string _savePath;
        private bool _savePending;
        private float _nextSaveAllowedAt;

        public PlayerSaveData Data => _data;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            _savePath = Path.Combine(Application.persistentDataPath, GameConstants.SaveFileName);
            Load();
        }

        private void Update()
        {
            if (!_savePending || Time.unscaledTime < _nextSaveAllowedAt)
            {
                return;
            }

            WriteToDisk();
        }

        public void Load()
        {
            if (!File.Exists(_savePath))
            {
                _data = new PlayerSaveData();
                _data.progress.EnsureLanguageProgressMigrated(_data.languageCode);
                WriteToDisk();
                return;
            }

            try
            {
                string json = File.ReadAllText(_savePath);
                _data = JsonUtility.FromJson<PlayerSaveData>(json);
                if (_data == null)
                {
                    _data = new PlayerSaveData();
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveManager] Failed to load save, resetting. {exception.Message}");
                _data = new PlayerSaveData();
                WriteToDisk();
            }

            _data.languageCode = GameConstants.NormalizeLanguageCode(_data.languageCode);
            _data.progress.EnsureLanguageProgressMigrated(_data.languageCode);
        }

        public void Save()
        {
            if (_data == null)
            {
                _data = new PlayerSaveData();
            }

            _savePending = true;
            if (Time.unscaledTime >= _nextSaveAllowedAt)
            {
                WriteToDisk();
            }
            else if (_nextSaveAllowedAt <= 0f)
            {
                _nextSaveAllowedAt = Time.unscaledTime + SaveThrottleSeconds;
            }
        }

        public void ReplaceData(PlayerSaveData data)
        {
            _data = data ?? new PlayerSaveData();
            _data.languageCode = GameConstants.NormalizeLanguageCode(_data.languageCode);
            _data.progress.EnsureLanguageProgressMigrated(_data.languageCode);
            Save();
        }

        public void FlushNow()
        {
            WriteToDisk();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                WriteToDisk();
            }
        }

        private void OnApplicationQuit()
        {
            WriteToDisk();
        }

        private void WriteToDisk()
        {
            if (_data == null)
            {
                _data = new PlayerSaveData();
            }

            string json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(_savePath, json);
            _savePending = false;
            _nextSaveAllowedAt = Time.unscaledTime + SaveThrottleSeconds;
        }
    }
}
