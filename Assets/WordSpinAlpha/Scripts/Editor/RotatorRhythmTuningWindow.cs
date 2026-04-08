using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Editor
{
    public class RotatorRhythmTuningWindow : EditorWindow
    {
        private Vector2 _scroll;
        private bool _showLiveRotator = true;
        private bool _showRhythms = true;
        private bool _showDifficultyProfiles = true;
        private bool _showDifficultyTiers = true;
        private readonly List<bool> _rhythmFoldouts = new List<bool>();
        private readonly List<bool> _difficultyFoldouts = new List<bool>();
        private readonly List<bool> _difficultyTierFoldouts = new List<bool>();

        private float _liveBaseSpeed = 45f;
        private bool _liveClockwise = true;
        private float _previewAssistMultiplier = 1.15f;
        private float _previewAssistDuration = 1.2f;
        private bool _previewAssistClockwise = true;
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        private RhythmCatalog _rhythmCatalog;
        private DifficultyCatalog _difficultyCatalog;
        private DifficultyTierCatalog _difficultyTierCatalog;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Ritmik Donus ve Zorluk Ayarlari")]
        public static void Open()
        {
            GetWindow<RotatorRhythmTuningWindow>("Ritim ve Donus");
        }

        private void OnEnable()
        {
            Reload();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("Ritmik Donus ve Zorluk Ayarlari", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Aktif rotator davranisini ve rhythm/difficulty kataloglarini tek yerden ayarlar. Play sirasinda rotator onizleme yapabilir.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Yeniden Yukle", GUILayout.Height(28f)))
                {
                    Reload();
                }

                string saveLabel = Application.isPlaying ? "Kaydet ve Uygula" : "Kaydet";
                if (GUILayout.Button(saveLabel, GUILayout.Height(28f)))
                {
                    Save();
                }
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            _showLiveRotator = EditorGUILayout.BeginFoldoutHeaderGroup(_showLiveRotator, "Aktif Rotator");
            if (_showLiveRotator)
            {
                DrawLiveRotator();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showRhythms = EditorGUILayout.BeginFoldoutHeaderGroup(_showRhythms, "Rhythm Profilleri");
            if (_showRhythms)
            {
                DrawRhythmProfiles();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showDifficultyProfiles = EditorGUILayout.BeginFoldoutHeaderGroup(_showDifficultyProfiles, "Difficulty Profilleri");
            if (_showDifficultyProfiles)
            {
                DrawDifficultyProfiles();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showDifficultyTiers = EditorGUILayout.BeginFoldoutHeaderGroup(_showDifficultyTiers, "Difficulty Tier Katlari");
            if (_showDifficultyTiers)
            {
                DrawDifficultyTiers();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
        }

        private void Reload()
        {
            _rhythmCatalog = WordSpinAlphaRuntimeConfigRepository.LoadRhythmProfiles() ?? new RhythmCatalog();
            _difficultyCatalog = WordSpinAlphaRuntimeConfigRepository.LoadDifficultyProfiles() ?? new DifficultyCatalog();
            _difficultyTierCatalog = WordSpinAlphaRuntimeConfigRepository.LoadDifficultyTiers() ?? new DifficultyTierCatalog();

            if (_rhythmCatalog.profiles == null)
            {
                _rhythmCatalog.profiles = Array.Empty<RhythmProfileDefinition>();
            }

            if (_difficultyCatalog.profiles == null)
            {
                _difficultyCatalog.profiles = Array.Empty<DifficultyProfileDefinition>();
            }

            if (_difficultyTierCatalog.tiers == null)
            {
                _difficultyTierCatalog.tiers = Array.Empty<DifficultyTierDefinition>();
            }

            SyncFoldouts(_rhythmFoldouts, _rhythmCatalog.profiles.Length);
            SyncFoldouts(_difficultyFoldouts, _difficultyCatalog.profiles.Length);
            SyncFoldouts(_difficultyTierFoldouts, _difficultyTierCatalog.tiers.Length);

            TargetRotator rotator = UnityEngine.Object.FindObjectOfType<TargetRotator>(true);
            if (rotator != null)
            {
                _liveBaseSpeed = rotator.BaseRotationSpeed;
                _liveClockwise = rotator.BaseClockwise;
                _previewAssistClockwise = rotator.Clockwise;
                _previewAssistMultiplier = Mathf.Max(1f, rotator.RhythmAssistSpeedMultiplier);
                _previewAssistDuration = Mathf.Max(0.2f, rotator.RhythmAssistRemainingSeconds > 0f ? rotator.RhythmAssistRemainingSeconds : 1.2f);
            }
        }

        private void TryAutoRefresh()
        {
            if (!WordSpinAlphaEditorSyncUtility.ConsumeChanges(WordSpinAlphaEditorSyncKind.Scene | WordSpinAlphaEditorSyncKind.RuntimeConfig, ref _syncStamp))
            {
                return;
            }

            Reload();
            Repaint();
        }

        private void DrawLiveRotator()
        {
            TargetRotator rotator = UnityEngine.Object.FindObjectOfType<TargetRotator>(true);
            if (rotator == null)
            {
                EditorGUILayout.HelpBox("Sahnede TargetRotator bulunamadi.", MessageType.Warning);
                return;
            }

            _liveBaseSpeed = EditorGUILayout.FloatField("Baz Donus Hizi", _liveBaseSpeed);
            _liveClockwise = EditorGUILayout.Toggle("Baz Yon Saat Yonunde", _liveClockwise);
            EditorGUILayout.Space(4f);
            _previewAssistMultiplier = EditorGUILayout.FloatField("Preview Assist Carpani", _previewAssistMultiplier);
            _previewAssistDuration = EditorGUILayout.FloatField("Preview Assist Suresi", _previewAssistDuration);
            _previewAssistClockwise = EditorGUILayout.Toggle("Preview Assist Yon Saat Yonunde", _previewAssistClockwise);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Baz Donusu Uygula"))
                {
                    rotator.ApplyEditorBaseTuning(Mathf.Max(1f, _liveBaseSpeed), _liveClockwise);
                    EditorUtility.SetDirty(rotator);
                    WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.Scene);
                    if (Application.isPlaying)
                    {
                        WordSpinAlphaEditorRuntimeRefreshUtility.RefreshCurrentTargetState();
                    }
                }

                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    if (GUILayout.Button("Assist Preview"))
                    {
                        rotator.ApplyRhythmAssist(_previewAssistClockwise, Mathf.Max(1f, _previewAssistMultiplier), Mathf.Max(0f, _previewAssistDuration));
                    }

                    if (GUILayout.Button("Assist Temizle"))
                    {
                        rotator.ClearRhythmAssist();
                    }
                }
            }
        }

        private void DrawRhythmProfiles()
        {
            for (int i = 0; i < _rhythmCatalog.profiles.Length; i++)
            {
                RhythmProfileDefinition profile = _rhythmCatalog.profiles[i] ?? new RhythmProfileDefinition();
                _rhythmCatalog.profiles[i] = profile;

                EditorGUILayout.BeginVertical("box");
                _rhythmFoldouts[i] = EditorGUILayout.Foldout(_rhythmFoldouts[i], $"{profile.displayName ?? "Rhythm"} | {profile.rhythmProfileId}", true);
                if (_rhythmFoldouts[i])
                {
                    profile.rhythmProfileId = EditorGUILayout.TextField("Rhythm Id", profile.rhythmProfileId);
                    profile.displayName = EditorGUILayout.TextField("Gorunen Ad", profile.displayName);
                    profile.baseRotationSpeed = EditorGUILayout.FloatField("Baz Donus Hizi", profile.baseRotationSpeed);
                    profile.speedVariance = EditorGUILayout.FloatField("Hiz Varyansi", profile.speedVariance);
                    profile.directionPattern = EditorGUILayout.TextField("Yon Deseni", profile.directionPattern);
                    profile.targetWindowLeadTime = EditorGUILayout.FloatField("Target Lead Time", profile.targetWindowLeadTime);
                    profile.postHitRetargetDelay = EditorGUILayout.FloatField("Hit Sonrasi Retarget", profile.postHitRetargetDelay);
                    profile.easyFlowAssist = EditorGUILayout.FloatField("Easy Flow Assist", profile.easyFlowAssist);
                    profile.musicSyncStrength = EditorGUILayout.FloatField("Music Sync Gucu", profile.musicSyncStrength);
                    profile.lightPulseStrength = EditorGUILayout.FloatField("Light Pulse Gucu", profile.lightPulseStrength);
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawDifficultyProfiles()
        {
            for (int i = 0; i < _difficultyCatalog.profiles.Length; i++)
            {
                DifficultyProfileDefinition profile = _difficultyCatalog.profiles[i] ?? new DifficultyProfileDefinition();
                _difficultyCatalog.profiles[i] = profile;

                EditorGUILayout.BeginVertical("box");
                _difficultyFoldouts[i] = EditorGUILayout.Foldout(_difficultyFoldouts[i], $"{profile.displayName ?? "Difficulty"} | {profile.difficultyProfileId}", true);
                if (_difficultyFoldouts[i])
                {
                    profile.difficultyProfileId = EditorGUILayout.TextField("Profile Id", profile.difficultyProfileId);
                    profile.displayName = EditorGUILayout.TextField("Gorunen Ad", profile.displayName);
                    profile.rotationSpeedMultiplier = EditorGUILayout.FloatField("Donus Hizi Carpani", profile.rotationSpeedMultiplier);
                    profile.perfectAngle = EditorGUILayout.FloatField("Perfect Acisi", profile.perfectAngle);
                    profile.toleranceAngle = EditorGUILayout.FloatField("Tolerance Acisi", profile.toleranceAngle);
                    profile.obstacleBudget = EditorGUILayout.IntField("Obstacle Butcesi", profile.obstacleBudget);
                    profile.maxQuestionLength = EditorGUILayout.IntField("Maks Soru Uzunlugu", profile.maxQuestionLength);
                    profile.enableRandomSlots = EditorGUILayout.Toggle("Random Slot Acik", profile.enableRandomSlots);
                    profile.dopamineSpike = EditorGUILayout.Toggle("Dopamine Spike", profile.dopamineSpike);
                    profile.breathLevel = EditorGUILayout.Toggle("Breath Level", profile.breathLevel);
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawDifficultyTiers()
        {
            for (int i = 0; i < _difficultyTierCatalog.tiers.Length; i++)
            {
                DifficultyTierDefinition tier = _difficultyTierCatalog.tiers[i] ?? new DifficultyTierDefinition();
                _difficultyTierCatalog.tiers[i] = tier;

                EditorGUILayout.BeginVertical("box");
                _difficultyTierFoldouts[i] = EditorGUILayout.Foldout(_difficultyTierFoldouts[i], $"{tier.displayName ?? "Tier"} | {tier.difficultyTierId}", true);
                if (_difficultyTierFoldouts[i])
                {
                    tier.difficultyTierId = EditorGUILayout.TextField("Tier Id", tier.difficultyTierId);
                    tier.displayName = EditorGUILayout.TextField("Gorunen Ad", tier.displayName);
                    tier.perfectWindowScale = EditorGUILayout.FloatField("Perfect Window Carpani", tier.perfectWindowScale);
                    tier.goodWindowScale = EditorGUILayout.FloatField("Good Window Carpani", tier.goodWindowScale);
                    tier.nearMissScale = EditorGUILayout.FloatField("Near Miss Carpani", tier.nearMissScale);
                    tier.rotationSpeedScale = EditorGUILayout.FloatField("Donus Hizi Carpani", tier.rotationSpeedScale);
                    tier.armedAssistScale = EditorGUILayout.FloatField("Armed Assist Carpani", tier.armedAssistScale);
                    tier.perfectChainAssistScale = EditorGUILayout.FloatField("Perfect Chain Assist Carpani", tier.perfectChainAssistScale);
                    tier.waitCapScale = EditorGUILayout.FloatField("Wait Cap Carpani", tier.waitCapScale);
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void Save()
        {
            WordSpinAlphaRuntimeConfigRepository.SaveRhythmProfiles(_rhythmCatalog);
            WordSpinAlphaRuntimeConfigRepository.SaveDifficultyProfiles(_difficultyCatalog);
            WordSpinAlphaRuntimeConfigRepository.SaveDifficultyTiers(_difficultyTierCatalog);
            WordSpinAlphaEditorRuntimeRefreshUtility.SaveDirtyAssets();

            TargetRotator rotator = UnityEngine.Object.FindObjectOfType<TargetRotator>(true);
            if (rotator != null)
            {
                rotator.ApplyEditorBaseTuning(Mathf.Max(1f, _liveBaseSpeed), _liveClockwise);
                EditorUtility.SetDirty(rotator);
            }

            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.ApplyContentAndConfigRefresh(true);
            }
            else
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.MarkCurrentSceneDirty();
            }

            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.RuntimeConfig | WordSpinAlphaEditorSyncKind.Scene);
        }

        private static void SyncFoldouts(List<bool> list, int count)
        {
            while (list.Count < count)
            {
                list.Add(false);
            }

            while (list.Count > count)
            {
                list.RemoveAt(list.Count - 1);
            }
        }
    }
}
