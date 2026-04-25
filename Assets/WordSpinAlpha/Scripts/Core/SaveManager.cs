using System;
using System.IO;
using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class SaveManager : Singleton<SaveManager>
    {
        private const float SaveThrottleSeconds = 0.35f;
        private const string TempFileSuffix = ".tmp";
        private const string BackupFileSuffix = ".bak";

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
                _data.EnsureSessionLocalizationMigrated();
                WriteToDisk();
                return;
            }

            try
            {
                string json = File.ReadAllText(_savePath);
                _data = JsonUtility.FromJson<PlayerSaveData>(json);
                if (_data == null)
                {
                    BackupCorruptedSave();
                    _data = new PlayerSaveData();
                    WriteToDisk();
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveManager] Failed to load save, resetting. {exception.Message}");
                BackupCorruptedSave();
                _data = new PlayerSaveData();
                WriteToDisk();
            }

            _data.languageCode = GameConstants.NormalizeLanguageCode(_data.languageCode);
            _data.progress.EnsureLanguageProgressMigrated(_data.languageCode);
            _data.EnsureSessionLocalizationMigrated();
        }

        public void Save()
        {
            if (_data == null)
            {
                _data = new PlayerSaveData();
            }

            _data.EnsureSessionLocalizationMigrated();
            _data.SyncLegacySessionFromCurrentLanguage();
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
            _data.EnsureSessionLocalizationMigrated();
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
            string directory = Path.GetDirectoryName(_savePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempPath = _savePath + TempFileSuffix;
            string backupPath = _savePath + BackupFileSuffix;

            File.WriteAllText(tempPath, json);

            if (File.Exists(_savePath))
            {
                try
                {
                    File.Replace(tempPath, _savePath, backupPath, true);
                }
                catch (PlatformNotSupportedException)
                {
                    File.Copy(tempPath, _savePath, true);
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
            }
            else
            {
                File.Move(tempPath, _savePath);
            }

            _savePending = false;
            _nextSaveAllowedAt = Time.unscaledTime + SaveThrottleSeconds;
        }

        private void BackupCorruptedSave()
        {
            if (string.IsNullOrWhiteSpace(_savePath) || !File.Exists(_savePath))
            {
                return;
            }

            try
            {
                string directory = Path.GetDirectoryName(_savePath);
                string fileName = Path.GetFileNameWithoutExtension(_savePath);
                string extension = Path.GetExtension(_savePath);
                string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
                string corruptedCopyPath = Path.Combine(directory ?? string.Empty, $"{fileName}.corrupt_{timestamp}{extension}");
                File.Copy(_savePath, corruptedCopyPath, false);
            }
            catch (Exception backupException)
            {
                Debug.LogWarning($"[SaveManager] Failed to backup corrupted save. {backupException.Message}");
            }
        }
    }
}
