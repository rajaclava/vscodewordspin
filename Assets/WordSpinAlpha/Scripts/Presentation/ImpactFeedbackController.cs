using System.Collections;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    public class ImpactFeedbackController : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private ImpactFeelProfile profile;
        [SerializeField] private bool vibrationEnabled = true;

        private Coroutine _timeScaleRoutine;
        private bool _hitStopActive;
        private float _hitStopRestoreScale = 1f;
        private Vector3 _basePosition;
        private float _kickTimeRemaining;
        private float _kickDuration;
        private float _kickStrength;
        private Vector3 _kickDirection;

        private void Awake()
        {
            ResolveCamera();
            if (gameplayCamera != null)
            {
                _basePosition = gameplayCamera.transform.position;
            }
        }

        private void OnEnable()
        {
            GameEvents.ImpactOccurred += HandleImpactOccurred;
        }

        private void OnDisable()
        {
            GameEvents.ImpactOccurred -= HandleImpactOccurred;
            if (_hitStopActive)
            {
                Time.timeScale = _hitStopRestoreScale;
                _hitStopActive = false;
            }
        }

        private void Update()
        {
            if (gameplayCamera == null)
            {
                ResolveCamera();
                return;
            }

            if (_kickTimeRemaining <= 0f)
            {
                return;
            }

            _kickTimeRemaining -= Time.unscaledDeltaTime;
            float t = 1f - Mathf.Clamp01(_kickTimeRemaining / Mathf.Max(0.0001f, _kickDuration));
            float rise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.25f));
            float fall = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.25f) / 0.75f));
            float envelope = rise * fall;
            Vector3 offset = _kickDirection * (_kickStrength * envelope);
            gameplayCamera.transform.position = _basePosition + offset;

            if (_kickTimeRemaining <= 0f)
            {
                gameplayCamera.transform.position = _basePosition;
            }
        }

        private void HandleImpactOccurred(ImpactEventData impact)
        {
            if (!ResolveEntry(impact, out ImpactFeelProfile.Entry entry))
            {
                return;
            }

            float speedFactor = Mathf.Clamp01(impact.impactSpeed / 12f);
            float accuracyFactor = Mathf.Clamp01(impact.accuracy);
            float comboFactor = Mathf.Clamp01(impact.combo / 6f);

            float kick = entry.cameraKick * Mathf.Lerp(0.92f, 1.12f, speedFactor);
            if (impact.impactType == HitResultType.Perfect)
            {
                kick *= Mathf.Lerp(1f, 1.18f, comboFactor);
            }

            float settleMs = entry.settleMs * Mathf.Lerp(0.96f, 1.08f, 1f - accuracyFactor);
            float hitStopMs = entry.hitStopMs;
            if (impact.impactType == HitResultType.Perfect)
            {
                hitStopMs *= Mathf.Lerp(1f, 1.12f, comboFactor);
            }

            GameEvents.RaiseImpactFeelResolved(new ResolvedImpactFeelData
            {
                impactType = impact.impactType,
                impactWorldPos = impact.impactWorldPos,
                hitStopMs = hitStopMs,
                hapticIntensity = entry.hapticIntensity,
                hapticSharpness = entry.hapticSharpness,
                audioAttack = entry.audioAttack,
                cameraKick = kick,
                settleMs = settleMs,
                flashScale = entry.flashScale,
                particleBurst = entry.particleBurst,
                accuracy = impact.accuracy,
                impactSpeed = impact.impactSpeed,
                combo = impact.combo
            });

            PlayKick(kick, settleMs / 1000f, impact);
            PlayHitStop(hitStopMs / 1000f);
            TriggerVibration(entry, impact);
        }

        private bool ResolveEntry(ImpactEventData impact, out ImpactFeelProfile.Entry entry)
        {
            if (profile != null && profile.TryGetEntry(impact.impactType, out entry))
            {
                return true;
            }

            entry = BuildFallbackEntry(impact.impactType);
            return true;
        }

        private void PlayKick(float strength, float duration, ImpactEventData impact)
        {
            ResolveCamera();
            if (gameplayCamera == null)
            {
                return;
            }

            _basePosition = gameplayCamera.transform.position;
            _kickStrength = strength;
            _kickDuration = Mathf.Max(0.04f, duration);
            _kickTimeRemaining = _kickDuration;
            _kickDirection = ResolveKickDirection(impact);
        }

        private void PlayHitStop(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            if (!_hitStopActive)
            {
                _hitStopRestoreScale = Time.timeScale;
                _hitStopActive = true;
            }

            if (_timeScaleRoutine != null)
            {
                StopCoroutine(_timeScaleRoutine);
            }

            _timeScaleRoutine = StartCoroutine(HitStopRoutine(duration, _hitStopRestoreScale));
        }

        private void TriggerVibration(ImpactFeelProfile.Entry entry, ImpactEventData impact)
        {
            if (!vibrationEnabled)
            {
                return;
            }

            if (impact.impactType == HitResultType.NearMiss && entry.hapticIntensity < 0.4f)
            {
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        private Vector3 ResolveKickDirection(ImpactEventData impact)
        {
            if (gameplayCamera == null)
            {
                return Vector3.down;
            }

            Vector3 fromCamera = (impact.impactWorldPos - gameplayCamera.transform.position);
            fromCamera.z = 0f;
            if (fromCamera.sqrMagnitude < 0.0001f)
            {
                return Vector3.down;
            }

            return (-fromCamera).normalized;
        }

        private void ResolveCamera()
        {
            if (gameplayCamera == null)
            {
                gameplayCamera = Camera.main;
            }

            if (gameplayCamera == null)
            {
                gameplayCamera = FindObjectOfType<Camera>();
            }
        }

        private IEnumerator HitStopRoutine(float duration, float restoreScale)
        {
            Time.timeScale = 0.10f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = restoreScale;
            _hitStopActive = false;
            _timeScaleRoutine = null;
        }

        private static ImpactFeelProfile.Entry BuildFallbackEntry(HitResultType type)
        {
            switch (type)
            {
                case HitResultType.Perfect:
                    return new ImpactFeelProfile.Entry
                    {
                        impactType = type,
                        hitStopMs = 0f,
                        hapticIntensity = 0.85f,
                        hapticSharpness = 0.9f,
                        audioAttack = 1.08f,
                        cameraKick = 0.16f,
                        settleMs = 120f,
                        flashScale = 1.12f,
                        particleBurst = 1.15f
                    };
                case HitResultType.Tolerated:
                    return new ImpactFeelProfile.Entry
                    {
                        impactType = type,
                        hitStopMs = 0f,
                        hapticIntensity = 0.6f,
                        hapticSharpness = 0.68f,
                        audioAttack = 1.02f,
                        cameraKick = 0.11f,
                        settleMs = 105f,
                        flashScale = 1.06f,
                        particleBurst = 0.8f
                    };
                case HitResultType.NearMiss:
                    return new ImpactFeelProfile.Entry
                    {
                        impactType = type,
                        hitStopMs = 0f,
                        hapticIntensity = 0.35f,
                        hapticSharpness = 0.5f,
                        audioAttack = 0.96f,
                        cameraKick = 0.09f,
                        settleMs = 95f,
                        flashScale = 1.02f,
                        particleBurst = 0.25f
                    };
                case HitResultType.WrongSlot:
                    return new ImpactFeelProfile.Entry
                    {
                        impactType = type,
                        hitStopMs = 0f,
                        hapticIntensity = 0.9f,
                        hapticSharpness = 0.95f,
                        audioAttack = 0.92f,
                        cameraKick = 0.21f,
                        settleMs = 155f,
                        flashScale = 1.1f,
                        particleBurst = 1.35f
                    };
                case HitResultType.WrongLetter:
                    return new ImpactFeelProfile.Entry
                    {
                        impactType = type,
                        hitStopMs = 0f,
                        hapticIntensity = 0.62f,
                        hapticSharpness = 0.82f,
                        audioAttack = 0.94f,
                        cameraKick = 0.17f,
                        settleMs = 130f,
                        flashScale = 1.04f,
                        particleBurst = 0f
                    };
                default:
                    return new ImpactFeelProfile.Entry
                    {
                        impactType = type,
                        hitStopMs = 10f,
                        hapticIntensity = 0.55f,
                        hapticSharpness = 0.7f,
                        audioAttack = 0.94f,
                        cameraKick = 0.14f,
                        settleMs = 115f,
                        flashScale = 1.03f,
                        particleBurst = 0f
                    };
            }
        }
    }
}
