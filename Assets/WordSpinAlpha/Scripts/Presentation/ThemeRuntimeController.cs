using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Presentation
{
    public class ThemeRuntimeController : MonoBehaviour
    {
        [SerializeField] private Image topBar;
        [SerializeField] private Image questionPanel;
        [SerializeField] private Image bottomBar;
        [SerializeField] private Image keyboardSkinFrame;
        [SerializeField] private TextMeshProUGUI levelLabel;
        [SerializeField] private TextMeshProUGUI currencyLabel;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private SpriteRenderer backgroundMatte;
        [SerializeField] private SpriteRenderer backgroundGlow;
        [SerializeField] private SpriteRenderer ambienceLeft;
        [SerializeField] private SpriteRenderer ambienceRight;
        [SerializeField] private SpriteRenderer orbitRing;
        [SerializeField] private SpriteRenderer rotatorArt;
        [SerializeField] private SpriteRenderer rotatorCore;
        [SerializeField] private SpriteRenderer launcherBody;
        [SerializeField] private SpriteRenderer flightLane;
        [SerializeField] private TextMeshProUGUI targetHintLabel;
        [SerializeField] private float perfectPitchStep = 0.035f;
        [SerializeField] private float perfectPitchMaxBoost = 0.22f;
        [SerializeField] private float mobileGlowAlphaScale = 0.52f;
        [SerializeField] private float mobileAmbientScale = 0.62f;
        [SerializeField] private float cameraContrastBias = 0.36f;

        private ThemeCatalog _themeCatalog;
        private ThemePackDefinition _currentTheme;
        private AudioSource _sfxSource;
        private AudioSource _hitSfxSource;
        private AudioSource _bgmSource;
        private AudioClip _loadClip;
        private AudioClip _fireClip;
        private AudioClip _perfectClip;
        private AudioClip _toleratedClip;
        private AudioClip _missClip;
        private AudioClip _questionCompleteClip;
        private AudioClip _levelCompleteClip;
        private AudioClip _themeHitClip;
        private AudioClip _themeMissClip;
        private AudioClip _themeCompletionClip;
        private AudioClip _themeBgmClip;
        private float _impactAudioAttack = 1f;
        private float _impactFlashScale = 1f;
        private float _rhythmFlowIntensity;
        private int _rhythmMomentumLevel;
        private int _consecutivePerfectPitchCount;
        private int _currentLevelId;
        private Color _baseBackgroundGlowColor;
        private Color _baseAmbienceLeftColor;
        private Color _baseAmbienceRightColor;
        private Vector3 _baseBackgroundGlowScale = Vector3.one;
        private Vector3 _baseAmbienceLeftScale = Vector3.one;
        private Vector3 _baseAmbienceRightScale = Vector3.one;

        private void Awake()
        {
            EnsureCatalog();
            EnsureAudioListener();
            EnsureAudioSources();
            EnsurePlaceholderClips();
            CacheAmbientBaseState();
        }

        private void Update()
        {
            float targetPitch = 1f + (_rhythmFlowIntensity * 0.08f) + (_rhythmMomentumLevel * 0.03f);
            if (_bgmSource != null)
            {
                _bgmSource.pitch = Mathf.Lerp(_bgmSource.pitch, targetPitch, Time.deltaTime * 6f);
            }

            float ambientBreath = 0.5f + (Mathf.Sin(Time.time * 1.45f) * 0.5f);

            float orbitPulseSpeed = 1.7f + (_rhythmFlowIntensity * 1.6f) + (_rhythmMomentumLevel * 0.45f);
            float orbitScaleAmplitude = 0.06f + (_rhythmFlowIntensity * 0.04f);
            float orbitAlphaAmplitude = 0.12f + (_rhythmFlowIntensity * 0.08f);
            ApplyPulse(orbitRing, orbitPulseSpeed, orbitScaleAmplitude, orbitAlphaAmplitude);

            float glowPulseSpeed = 1.5f + (_rhythmFlowIntensity * 1.2f) + (_rhythmMomentumLevel * 0.30f);
            ApplyPulse(backgroundGlow, glowPulseSpeed, 0.08f + (_rhythmFlowIntensity * 0.04f), 0.18f + (_rhythmFlowIntensity * 0.07f));
            ApplyPulse(ambienceLeft, 1.35f + (_rhythmFlowIntensity * 1.0f), 0.06f + (_rhythmFlowIntensity * 0.03f), 0.12f + (_rhythmFlowIntensity * 0.06f));
            ApplyPulse(ambienceRight, 1.28f + (_rhythmFlowIntensity * 0.95f), 0.06f + (_rhythmFlowIntensity * 0.03f), 0.12f + (_rhythmFlowIntensity * 0.06f));

            UpdateAmbientLighting(ambientBreath);
            UpdateAmbientTransforms(ambientBreath);
        }

        private void OnEnable()
        {
            GameEvents.LevelStarted += HandleLevelStarted;
            GameEvents.ThemeUnlocked += HandleThemeUnlocked;
            GameEvents.PinLoaded += HandlePinLoaded;
            GameEvents.PinFired += HandlePinFired;
            GameEvents.HitEvaluated += HandleHitEvaluated;
            GameEvents.ImpactFeelResolved += HandleImpactFeelResolved;
            GameEvents.RhythmFlowStateChanged += HandleRhythmFlowStateChanged;
            GameEvents.QuestionCompleted += HandleQuestionCompleted;
            GameEvents.QuestionFailed += HandleQuestionFailed;
            GameEvents.LevelCompleted += HandleLevelCompleted;
            GameEvents.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable()
        {
            GameEvents.LevelStarted -= HandleLevelStarted;
            GameEvents.ThemeUnlocked -= HandleThemeUnlocked;
            GameEvents.PinLoaded -= HandlePinLoaded;
            GameEvents.PinFired -= HandlePinFired;
            GameEvents.HitEvaluated -= HandleHitEvaluated;
            GameEvents.ImpactFeelResolved -= HandleImpactFeelResolved;
            GameEvents.RhythmFlowStateChanged -= HandleRhythmFlowStateChanged;
            GameEvents.QuestionCompleted -= HandleQuestionCompleted;
            GameEvents.QuestionFailed -= HandleQuestionFailed;
            GameEvents.LevelCompleted -= HandleLevelCompleted;
            GameEvents.LanguageChanged -= HandleLanguageChanged;
        }

        private void HandleLevelStarted(LevelContext context)
        {
            ApplyTheme(context.themeId, context);
            _consecutivePerfectPitchCount = 0;
            _currentLevelId = context.levelId;
            RefreshLevelLabel();
        }

        private void HandleThemeUnlocked(string themeId)
        {
            ApplyTheme(themeId, null);
        }

        private void ApplyTheme(string themeId, LevelContext? context)
        {
            EnsureCatalog();
            ThemePackDefinition theme = FindTheme(themeId);
            if (theme == null)
            {
                return;
            }

            if (topBar != null && ColorUtility.TryParseHtmlString(theme.uiPrimaryHex, out Color topColor))
            {
                topBar.color = topColor;
            }

            if (questionPanel != null && ColorUtility.TryParseHtmlString(theme.uiBackgroundHex, out Color questionColor))
            {
                questionPanel.color = questionColor;
            }

            if (bottomBar != null && ColorUtility.TryParseHtmlString(theme.uiAccentHex, out Color bottomColor))
            {
                bottomBar.color = bottomColor;
            }

            _currentTheme = theme;
            LoadThemeAssets(theme);
            CacheAmbientBaseState();
            ApplyWorldPalette(theme, context);
        }

        private ThemePackDefinition FindTheme(string themeId)
        {
            if (_themeCatalog == null || _themeCatalog.themes == null)
            {
                return null;
            }

            foreach (ThemePackDefinition theme in _themeCatalog.themes)
            {
                if (theme.themeId == themeId)
                {
                    return theme;
                }
            }

            return null;
        }

        private void EnsureCatalog()
        {
            if (_themeCatalog == null && ContentService.Instance != null)
            {
                _themeCatalog = ContentService.Instance.LoadThemes();
            }
        }

        private void EnsureAudioSources()
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            if (_bgmSource == null)
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    if (sources[i] != null && sources[i].loop)
                    {
                        _bgmSource = sources[i];
                        break;
                    }
                }
            }

            if (_sfxSource == null)
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    AudioSource source = sources[i];
                    if (source == null || source == _bgmSource || source.loop)
                    {
                        continue;
                    }

                    _sfxSource = source;
                    break;
                }
            }

            if (_hitSfxSource == null)
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    AudioSource source = sources[i];
                    if (source == null || source == _bgmSource || source == _sfxSource || source.loop)
                    {
                        continue;
                    }

                    _hitSfxSource = source;
                    break;
                }
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
            }

            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
            }

            if (_hitSfxSource == null)
            {
                _hitSfxSource = gameObject.AddComponent<AudioSource>();
            }

            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
            _sfxSource.spatialBlend = 0f;
            _sfxSource.volume = 1f;
            _sfxSource.ignoreListenerPause = true;

            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;
            _bgmSource.spatialBlend = 0f;
            _bgmSource.volume = 0.35f;
            _bgmSource.ignoreListenerPause = true;

            _hitSfxSource.playOnAwake = false;
            _hitSfxSource.loop = false;
            _hitSfxSource.spatialBlend = 0f;
            _hitSfxSource.volume = 1f;
            _hitSfxSource.ignoreListenerPause = true;
        }

        private void EnsureAudioListener()
        {
            if (FindObjectOfType<AudioListener>() != null)
            {
                return;
            }

            Camera targetCamera = gameplayCamera;
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }

            if (targetCamera != null && targetCamera.GetComponent<AudioListener>() == null)
            {
                targetCamera.gameObject.AddComponent<AudioListener>();
            }
        }

        private void EnsurePlaceholderClips()
        {
            if (_loadClip == null) _loadClip = CreateTone("load", 620f, 0.07f, 0.09f);
            if (_fireClip == null) _fireClip = CreateTone("fire", 380f, 0.11f, 0.14f);
            if (_perfectClip == null) _perfectClip = CreateTone("perfect", 880f, 0.14f, 0.18f);
            if (_toleratedClip == null) _toleratedClip = CreateTone("tolerated", 720f, 0.12f, 0.15f);
            if (_missClip == null) _missClip = CreateTone("miss", 360f, 0.20f, 0.24f);
            if (_questionCompleteClip == null) _questionCompleteClip = CreateTone("question_complete", 540f, 0.20f, 0.13f);
            if (_levelCompleteClip == null) _levelCompleteClip = CreateTone("level_complete", 660f, 0.34f, 0.15f);
        }

        private void LoadThemeAssets(ThemePackDefinition theme)
        {
            _themeHitClip = LoadClip(theme.hitSfxResourcePath);
            _themeMissClip = LoadClip(theme.missSfxResourcePath);
            _themeCompletionClip = LoadClip(theme.completionSfxResourcePath);
            _themeBgmClip = LoadClip(theme.bgmResourcePath);

            Sprite backgroundSprite = LoadSprite(theme.backgroundResourcePath);
            if (backgroundSprite != null && backgroundMatte != null)
            {
                backgroundMatte.sprite = backgroundSprite;
            }

            Sprite rotatorSprite = LoadSprite(theme.rotatorResourcePath);
            if (rotatorSprite != null && rotatorArt != null)
            {
                rotatorArt.sprite = rotatorSprite;
            }

            Sprite keyboardSprite = LoadSprite(theme.keyboardSkinResourcePath);
            if (keyboardSprite != null && keyboardSkinFrame != null)
            {
                keyboardSkinFrame.sprite = keyboardSprite;
                keyboardSkinFrame.type = Image.Type.Sliced;
            }

            if (_bgmSource != null)
            {
                if (_themeBgmClip != null && _bgmSource.clip != _themeBgmClip)
                {
                    _bgmSource.clip = _themeBgmClip;
                    _bgmSource.Play();
                }
                else if (_themeBgmClip == null)
                {
                    _bgmSource.Stop();
                    _bgmSource.clip = null;
                }
            }
        }

        private void ApplyWorldPalette(ThemePackDefinition theme, LevelContext? context)
        {
            if (!ColorUtility.TryParseHtmlString(theme.uiPrimaryHex, out Color primary))
            {
                primary = new Color(0.16f, 0.13f, 0.11f);
            }

            if (!ColorUtility.TryParseHtmlString(theme.uiAccentHex, out Color accent))
            {
                accent = new Color(1f, 0.56f, 0.26f);
            }

            if (!ColorUtility.TryParseHtmlString(theme.uiBackgroundHex, out Color background))
            {
                background = new Color(0.07f, 0.06f, 0.09f);
            }

            bool dopamineSpike = context.HasValue && context.Value.dopamineSpike;
            bool breathLevel = context.HasValue && context.Value.breathLevel;
            float glowBoost = dopamineSpike ? 1.4f : breathLevel ? 0.8f : 1f;
            float ambientBoost = dopamineSpike ? 1.3f : breathLevel ? 0.85f : 1f;
            float orbitBoost = dopamineSpike ? 1.35f : breathLevel ? 0.82f : 1f;

            if (gameplayCamera != null)
            {
                gameplayCamera.backgroundColor = Color.Lerp(background, Color.black, Mathf.Clamp01(cameraContrastBias));
            }

            if (backgroundGlow != null)
            {
                Color vividAccent = BoostSaturation(accent, 1.12f, 1.06f);
                _baseBackgroundGlowColor = WithAlpha(Color.Lerp(vividAccent, primary, 0.30f), 0.18f * glowBoost * mobileGlowAlphaScale);
                ApplySpriteColor(backgroundGlow, _baseBackgroundGlowColor);
                ApplyPulse(backgroundGlow, dopamineSpike ? 2.0f : breathLevel ? 0.95f : 1.35f, dopamineSpike ? 0.07f : 0.05f, dopamineSpike ? 0.12f : 0.08f);
            }

            if (ambienceLeft != null)
            {
                _baseAmbienceLeftColor = WithAlpha(BoostSaturation(accent, 1.08f, 1.02f), 0.22f * ambientBoost * mobileGlowAlphaScale);
                ApplySpriteColor(ambienceLeft, _baseAmbienceLeftColor);
                ApplyPulse(ambienceLeft, dopamineSpike ? 1.6f : breathLevel ? 0.82f : 1.15f, 0.03f, 0.06f);
            }

            if (ambienceRight != null)
            {
                _baseAmbienceRightColor = WithAlpha(BoostSaturation(accent, 1.08f, 1.02f), 0.18f * ambientBoost * mobileGlowAlphaScale);
                ApplySpriteColor(ambienceRight, _baseAmbienceRightColor);
                ApplyPulse(ambienceRight, dopamineSpike ? 1.55f : breathLevel ? 0.80f : 1.10f, 0.03f, 0.06f);
            }

            if (orbitRing != null)
            {
                ApplySpriteColor(orbitRing, WithAlpha(BoostSaturation(accent, 1.10f, 1.05f), 0.14f * orbitBoost));
                ApplyPulse(orbitRing, dopamineSpike ? 2.45f : breathLevel ? 0.92f : 1.55f, dopamineSpike ? 0.08f : 0.05f, dopamineSpike ? 0.14f : 0.08f);
            }

            if (rotatorCore != null)
            {
                ApplySpriteColor(rotatorCore, Color.Lerp(primary, background, 0.35f));
            }

            if (launcherBody != null)
            {
                ApplySpriteColor(launcherBody, Color.Lerp(primary, accent, 0.16f));
            }

            if (flightLane != null)
            {
                ApplySpriteColor(flightLane, WithAlpha(BoostSaturation(accent, 1.12f, 1.04f), 0.08f));
            }

            if (targetHintLabel != null)
            {
                targetHintLabel.color = Color.Lerp(accent, Color.white, 0.15f);
            }
        }

        private void HandlePinLoaded(char letter)
        {
            PlayClip(_loadClip, 1f);
            FlashSprite(launcherBody, WithAlpha(GetAccentColor(), 0.85f), 1.10f, 0.18f);
        }

        private void HandlePinFired(char letter)
        {
            PlayClip(_fireClip, 1f);
            FlashSprite(flightLane, WithAlpha(GetAccentColor(), 0.55f), 1.20f, 0.16f);
            FlashSprite(backgroundGlow, WithAlpha(GetAccentColor(), 0.18f), 1.06f, 0.24f);
        }

        private void HandleHitEvaluated(HitData hit)
        {
            switch (hit.resultType)
            {
                case HitResultType.Perfect:
                    _consecutivePerfectPitchCount++;
                    float pitchBoost = Mathf.Clamp((_consecutivePerfectPitchCount - 1) * Mathf.Max(0.005f, perfectPitchStep), 0f, Mathf.Max(0.05f, perfectPitchMaxBoost));
                    PlayHitClip(_themeHitClip != null ? _themeHitClip : _perfectClip, 1f + pitchBoost);
                    FlashSprite(orbitRing, new Color(1f, 0.95f, 0.70f, 0.85f), 1.14f * _impactFlashScale, 0.18f);
                    FlashSprite(rotatorCore, new Color(1f, 0.88f, 0.50f, 0.92f), 1.10f * _impactFlashScale, 0.16f);
                    FlashSprite(backgroundGlow, WithAlpha(GetAccentColor(), 0.40f), 1.16f * _impactFlashScale, 0.24f);
                    FlashSprite(ambienceLeft, WithAlpha(GetAccentColor(), 0.58f), 1.12f, 0.22f);
                    FlashSprite(ambienceRight, WithAlpha(GetAccentColor(), 0.52f), 1.12f, 0.22f);
                    FlashSprite(backgroundMatte, WithAlpha(GetAccentColor(), 0.18f), 1.03f, 0.20f);
                    break;
                case HitResultType.Tolerated:
                    _consecutivePerfectPitchCount = 0;
                    PlayHitClip(_themeHitClip != null ? _themeHitClip : _toleratedClip, 1f + (_currentTheme != null ? _currentTheme.hitPitchStep : 0.02f));
                    FlashSprite(orbitRing, new Color(1f, 0.75f, 0.35f, 0.75f), 1.08f * _impactFlashScale, 0.16f);
                    FlashSprite(rotatorCore, new Color(0.98f, 0.68f, 0.30f, 0.82f), 1.05f * _impactFlashScale, 0.16f);
                    FlashSprite(backgroundGlow, WithAlpha(GetAccentColor(), 0.28f), 1.10f * _impactFlashScale, 0.20f);
                    break;
                case HitResultType.NearMiss:
                    _consecutivePerfectPitchCount = 0;
                    PlayHitClip(_themeMissClip != null ? _themeMissClip : _missClip, 1.04f);
                    FlashSprite(orbitRing, new Color(1f, 0.48f, 0.32f, 0.65f), 1.05f * _impactFlashScale, 0.16f);
                    FlashSprite(backgroundGlow, new Color(1f, 0.42f, 0.24f, 0.14f), 1.03f, 0.14f);
                    break;
                case HitResultType.WrongSlot:
                case HitResultType.WrongLetter:
                case HitResultType.Miss:
                    _consecutivePerfectPitchCount = 0;
                    PlayHitClip(_themeMissClip != null ? _themeMissClip : _missClip, 1f);
                    FlashSprite(launcherBody, new Color(0.95f, 0.35f, 0.28f, 0.80f), 1.06f * _impactFlashScale, 0.16f);
                    FlashSprite(flightLane, new Color(0.95f, 0.35f, 0.28f, 0.30f), 1.03f * _impactFlashScale, 0.16f);
                    FlashSprite(backgroundGlow, new Color(0.95f, 0.35f, 0.28f, 0.18f), 1.05f, 0.16f);
                    break;
            }

            _impactAudioAttack = 1f;
            _impactFlashScale = 1f;
        }

        private void HandleImpactFeelResolved(ResolvedImpactFeelData feel)
        {
            _impactAudioAttack = Mathf.Max(0.85f, feel.audioAttack);
            _impactFlashScale = Mathf.Max(1f, feel.flashScale);
        }

        private void HandleRhythmFlowStateChanged(RhythmFlowStateData state)
        {
            _rhythmFlowIntensity = Mathf.Clamp01(state.flowIntensity);
            _rhythmMomentumLevel = Mathf.Clamp(state.perfectMomentumLevel, 0, 2);
        }

        private void HandleQuestionCompleted(QuestionContext context)
        {
            PlayClip(_themeCompletionClip != null ? _themeCompletionClip : _questionCompleteClip, 1f);
            FlashSprite(rotatorCore, new Color(1f, 0.86f, 0.44f, 0.90f), 1.18f, 0.26f);
            FlashSprite(backgroundGlow, WithAlpha(GetAccentColor(), 0.16f), 1.08f, 0.28f);
        }

        private void HandleQuestionFailed()
        {
            _consecutivePerfectPitchCount = 0;
            PlayClip(_themeMissClip != null ? _themeMissClip : _missClip, 0.92f);
            FlashSprite(launcherBody, new Color(0.92f, 0.38f, 0.34f, 0.75f), 1.04f, 0.20f);
        }

        private void HandleLevelCompleted(LevelContext context)
        {
            PlayClip(_themeCompletionClip != null ? _themeCompletionClip : _levelCompleteClip, 1f);
            FlashSprite(backgroundGlow, WithAlpha(GetAccentColor(), 0.24f), 1.12f, 0.38f);
            FlashSprite(rotatorCore, new Color(1f, 0.92f, 0.62f, 1f), 1.24f, 0.34f);
            FlashSprite(orbitRing, new Color(1f, 0.84f, 0.42f, 0.84f), 1.18f, 0.34f);
        }

        private void HandleLanguageChanged(string _)
        {
            RefreshLevelLabel();
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Color BoostSaturation(Color color, float saturationMultiplier, float valueMultiplier)
        {
            Color.RGBToHSV(color, out float hue, out float saturation, out float value);
            saturation = Mathf.Clamp01(saturation * saturationMultiplier);
            value = Mathf.Clamp01(value * valueMultiplier);
            return Color.HSVToRGB(hue, saturation, value);
        }

        private static void ApplySpriteColor(SpriteRenderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.color = color;
            PulseSprite pulse = renderer.GetComponent<PulseSprite>();
            if (pulse != null)
            {
                pulse.SetBaseColor(color);
            }
        }

        private static void ApplyPulse(SpriteRenderer renderer, float speed, float scaleAmplitude, float alphaAmplitude)
        {
            if (renderer == null)
            {
                return;
            }

            PulseSprite pulse = renderer.GetComponent<PulseSprite>();
            if (pulse != null)
            {
                pulse.SetPulse(speed, scaleAmplitude, alphaAmplitude);
            }
        }

        private Color GetAccentColor()
        {
            if (_currentTheme != null && ColorUtility.TryParseHtmlString(_currentTheme.uiAccentHex, out Color accent))
            {
                return accent;
            }

            return new Color(1f, 0.56f, 0.26f, 1f);
        }

        private void PlayClip(AudioClip clip, float pitch)
        {
            if (_sfxSource == null || clip == null)
            {
                return;
            }

            _sfxSource.pitch = pitch;
            _sfxSource.PlayOneShot(clip);
        }

        private void PlayHitClip(AudioClip clip, float pitch)
        {
            if (_hitSfxSource == null || clip == null)
            {
                return;
            }

            _hitSfxSource.pitch = pitch;
            _hitSfxSource.clip = clip;
            _hitSfxSource.Stop();
            _hitSfxSource.Play();
        }

        private AudioClip LoadClip(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            return Resources.Load<AudioClip>(resourcePath);
        }

        private Sprite LoadSprite(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            return Resources.Load<Sprite>(resourcePath);
        }

        private void FlashSprite(SpriteRenderer renderer, Color flashColor, float scaleMultiplier, float duration)
        {
            if (renderer == null)
            {
                return;
            }

            StartCoroutine(FlashSpriteRoutine(renderer, flashColor, scaleMultiplier, duration));
        }

        private IEnumerator FlashSpriteRoutine(SpriteRenderer renderer, Color flashColor, float scaleMultiplier, float duration)
        {
            if (renderer == null)
            {
                yield break;
            }

            Vector3 baseScale = renderer.transform.localScale;
            Color baseColor = renderer.color;
            float half = duration * 0.5f;
            float elapsed = 0f;

            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / half);
                renderer.color = Color.Lerp(baseColor, flashColor, t);
                renderer.transform.localScale = Vector3.Lerp(baseScale, baseScale * scaleMultiplier, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / half);
                renderer.color = Color.Lerp(flashColor, baseColor, t);
                renderer.transform.localScale = Vector3.Lerp(baseScale * scaleMultiplier, baseScale, t);
                yield return null;
            }

            renderer.color = baseColor;
            renderer.transform.localScale = baseScale;
        }

        private void UpdateAmbientLighting(float ambientBreath)
        {
            float breatheBoost = 0.85f + (ambientBreath * 0.75f);
            float flowBoost = 1f + (_rhythmFlowIntensity * 0.60f) + (_rhythmMomentumLevel * 0.12f);
            ApplySpriteColor(backgroundGlow, WithAlpha(_baseBackgroundGlowColor, Mathf.Clamp01(_baseBackgroundGlowColor.a * breatheBoost * flowBoost)));
            ApplySpriteColor(ambienceLeft, WithAlpha(_baseAmbienceLeftColor, Mathf.Clamp01(_baseAmbienceLeftColor.a * (0.92f + ambientBreath * 0.55f + _rhythmFlowIntensity * 0.30f))));
            ApplySpriteColor(ambienceRight, WithAlpha(_baseAmbienceRightColor, Mathf.Clamp01(_baseAmbienceRightColor.a * (0.88f + ambientBreath * 0.50f + _rhythmFlowIntensity * 0.28f))));
        }

        private static AudioClip CreateTone(string name, float frequency, float duration, float volume)
        {
            const int sampleRate = 44100;
            int sampleLength = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleLength];
            for (int i = 0; i < sampleLength; i++)
            {
                float progress = i / (float)sampleLength;
                float envelope = Mathf.Sin(progress * Mathf.PI);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(name, sampleLength, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void CacheAmbientBaseState()
        {
            if (backgroundGlow != null)
            {
                _baseBackgroundGlowScale = backgroundGlow.transform.localScale * 1.02f;
            }

            if (ambienceLeft != null)
            {
                _baseAmbienceLeftScale = ambienceLeft.transform.localScale * Mathf.Lerp(1.25f, 1.45f, Mathf.Clamp01(mobileAmbientScale));
            }

            if (ambienceRight != null)
            {
                _baseAmbienceRightScale = ambienceRight.transform.localScale * Mathf.Lerp(1.25f, 1.45f, Mathf.Clamp01(mobileAmbientScale));
            }

            PositionAmbientSurfaces();
        }

        private void UpdateAmbientTransforms(float ambientBreath)
        {
            float flowScaleBoost = 1f + (_rhythmFlowIntensity * 0.10f) + (_rhythmMomentumLevel * 0.03f);

            if (backgroundGlow != null)
            {
                float glowScale = 1f + (ambientBreath * 0.08f) + (_rhythmFlowIntensity * 0.04f);
                backgroundGlow.transform.localScale = _baseBackgroundGlowScale * glowScale * flowScaleBoost;
            }

            if (ambienceLeft != null)
            {
                float leftWave = 0.5f + (Mathf.Sin(Time.time * 1.18f + 0.45f) * 0.5f);
                ambienceLeft.transform.localScale = _baseAmbienceLeftScale * (1f + leftWave * 0.08f + _rhythmFlowIntensity * 0.03f);
            }

            if (ambienceRight != null)
            {
                float rightWave = 0.5f + (Mathf.Sin(Time.time * 1.08f + 1.25f) * 0.5f);
                ambienceRight.transform.localScale = _baseAmbienceRightScale * (1f + rightWave * 0.08f + _rhythmFlowIntensity * 0.03f);
            }
        }

        private void PositionAmbientSurfaces()
        {
            if (backgroundGlow != null)
            {
                backgroundGlow.transform.position = new Vector3(0f, -1.15f, 6f);
            }

            if (ambienceLeft != null)
            {
                ambienceLeft.transform.position = new Vector3(-4.20f, 0.10f, 2f);
            }

            if (ambienceRight != null)
            {
                ambienceRight.transform.position = new Vector3(4.20f, 0.10f, 2f);
            }
        }

        private void RefreshLevelLabel()
        {
            if (levelLabel == null || _currentLevelId <= 0)
            {
                return;
            }

            levelLabel.text = $"{GetLocalized("level")} {_currentLevelId}";
        }

        private static string GetLocalized(string key)
        {
            string language = SaveManager.Instance != null
                ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode)
                : GameConstants.DefaultLanguageCode;

            switch (language)
            {
                case "en":
                    return key switch
                    {
                        "level" => "Level",
                        _ => key
                    };
                case "es":
                    return key switch
                    {
                        "level" => "Nivel",
                        _ => key
                    };
                case "de":
                    return key switch
                    {
                        "level" => "Stufe",
                        _ => key
                    };
                default:
                    return key switch
                    {
                        "level" => "Seviye",
                        _ => key
                    };
            }
        }
    }
}
