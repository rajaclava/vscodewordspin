using UnityEngine;
using UnityEngine.SceneManagement;

namespace WordSpinAlpha.Core
{
    public sealed class GlobalMusicManager : Singleton<GlobalMusicManager>
    {
        // Demo mode currently uses one cross-scene music track everywhere.
        // Later, keep scene-specific path resolution here so gameplay can own a dedicated track
        // without rewriting scene presenters or bootstrap wiring.
        private const bool UseSingleDemoTrackEverywhere = true;
        private const string DefaultDemoMusicResourcePath = "Audio/Music/demo_main";
        private const string FutureGameplayMusicResourcePath = "Audio/Music/gameplay_main";

        private AudioSource _musicSource;
        private AudioClip _runtimeFallbackClip;
        private float _targetPitch = 1f;
        private float _gameplayFlowPitchBias;
        private float _gameplayJudgementPitchBias;

        public bool OwnsBackgroundMusic => true;
        public bool AllowsSceneOwnedBackgroundMusic => !OwnsBackgroundMusic;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            EnsureAudioSource();
            RefreshForScene(SceneManager.GetActiveScene());
        }

        private void OnEnable()
        {
            if (Instance != this)
            {
                return;
            }

            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            if (Instance != this)
            {
                return;
            }

            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Update()
        {
            if (_musicSource == null)
            {
                return;
            }

            _musicSource.pitch = Mathf.Lerp(_musicSource.pitch, _targetPitch, Time.unscaledDeltaTime * 3f);
        }

        public void NotifyGameplayFlowState(float flowIntensity, int momentumLevel)
        {
            if (UseSingleDemoTrackEverywhere)
            {
                _gameplayFlowPitchBias = 0f;
                RecomputeTargetPitch();
                return;
            }

            // Future gameplay-only mode can drive the dedicated gameplay BGM here.
            _gameplayFlowPitchBias = Mathf.Clamp01(flowIntensity) * 0.06f + Mathf.Clamp(momentumLevel, 0, 3) * 0.02f;
            RecomputeTargetPitch();
        }

        public void NotifyGameplayHit(HitResultType resultType)
        {
            if (UseSingleDemoTrackEverywhere)
            {
                _gameplayJudgementPitchBias = 0f;
                RecomputeTargetPitch();
                return;
            }

            // Future gameplay-only mode will use judgement quality to bend the gameplay track.
            _gameplayJudgementPitchBias = resultType switch
            {
                HitResultType.Perfect => 0.025f,
                HitResultType.Tolerated => 0.012f,
                HitResultType.NearMiss => -0.010f,
                HitResultType.WrongSlot => -0.018f,
                HitResultType.WrongLetter => -0.018f,
                HitResultType.Miss => -0.024f,
                _ => 0f
            };

            RecomputeTargetPitch();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshForScene(scene);
        }

        private void RefreshForScene(Scene scene)
        {
            EnsureAudioSource();

            AudioClip clip = LoadClip(ResolveResourcePath(scene));
            if (clip == null)
            {
                clip = EnsureFallbackClip();
            }

            if (_musicSource.clip != clip)
            {
                _musicSource.clip = clip;
            }

            if (!_musicSource.isPlaying)
            {
                _musicSource.Play();
            }

            _gameplayFlowPitchBias = 0f;
            _gameplayJudgementPitchBias = 0f;
            RecomputeTargetPitch();
        }

        private static string ResolveResourcePath(Scene scene)
        {
            if (UseSingleDemoTrackEverywhere)
            {
                return DefaultDemoMusicResourcePath;
            }

            if (scene.name == GameConstants.SceneGameplay)
            {
                return FutureGameplayMusicResourcePath;
            }

            return DefaultDemoMusicResourcePath;
        }

        private void EnsureAudioSource()
        {
            if (_musicSource == null)
            {
                _musicSource = GetComponent<AudioSource>();
            }

            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
            }

            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;
            _musicSource.volume = 0.32f;
            _musicSource.ignoreListenerPause = true;
        }

        private static AudioClip LoadClip(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            return Resources.Load<AudioClip>(resourcePath);
        }

        private AudioClip EnsureFallbackClip()
        {
            if (_runtimeFallbackClip != null)
            {
                return _runtimeFallbackClip;
            }

            const int sampleRate = 44100;
            const float durationSeconds = 6f;
            int sampleCount = Mathf.RoundToInt(sampleRate * durationSeconds);
            float[] samples = new float[sampleCount];
            float[] chord = { 220f, 277.18f, 329.63f, 440f };

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float phrase = Mathf.Repeat(time, 3f) / 3f;
                float envelope = 0.75f + Mathf.Sin(phrase * Mathf.PI * 2f) * 0.08f;
                float signal = 0f;

                for (int noteIndex = 0; noteIndex < chord.Length; noteIndex++)
                {
                    signal += Mathf.Sin(time * chord[noteIndex] * Mathf.PI * 2f + noteIndex * 0.35f);
                }

                signal /= chord.Length;
                samples[i] = signal * envelope * 0.08f;
            }

            _runtimeFallbackClip = AudioClip.Create("global_demo_music_fallback", sampleCount, 1, sampleRate, false);
            _runtimeFallbackClip.SetData(samples, 0);
            return _runtimeFallbackClip;
        }

        private void RecomputeTargetPitch()
        {
            _targetPitch = Mathf.Clamp(1f + _gameplayFlowPitchBias + _gameplayJudgementPitchBias, 0.9f, 1.08f);
        }
    }
}
