using UnityEngine;
using System.Collections;
using WordSpinAlpha.Content;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Core
{
    public class PinLauncher : Singleton<PinLauncher>
    {
        [SerializeField] private Pin pinPrefab;
        [SerializeField] private Transform pinSpawnPoint;
        [SerializeField] private FireGate fireGate;
        [SerializeField] private InputBuffer inputBuffer;
        [SerializeField] private float loadTweenDuration = 0.30f;
        [SerializeField] private float pinVisualScale = 1.18f;

        public float LoadTweenDuration => loadTweenDuration;
        public float PinVisualScale => pinVisualScale;

        private Pin _loadedPin;
        private Coroutine _loadRoutine;
        private ThemeCatalog _themeCatalog;

        protected override bool PersistAcrossScenes => false;

        private void Start()
        {
            ResolveReferences();
            TryHookInput();
        }

        private void OnEnable()
        {
            ResolveReferences();
            TryHookInput();
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.LetterPressed -= HandleLetterPressed;
                InputManager.Instance.SwipeUpRequested -= HandleSwipeUp;
            }
        }

        private void TryHookInput()
        {
            if (InputManager.Instance == null)
            {
                return;
            }

            InputManager.Instance.LetterPressed -= HandleLetterPressed;
            InputManager.Instance.SwipeUpRequested -= HandleSwipeUp;
            InputManager.Instance.LetterPressed += HandleLetterPressed;
            InputManager.Instance.SwipeUpRequested += HandleSwipeUp;
        }

        private void ResolveReferences()
        {
            if (fireGate == null)
            {
                fireGate = FindObjectOfType<FireGate>();
            }

            if (inputBuffer == null)
            {
                inputBuffer = FindObjectOfType<InputBuffer>();
            }
        }

        private void HandleLetterPressed(char letter, Vector3 startPosition)
        {
            if (_loadedPin != null && _loadedPin.IsLoaded)
            {
                return;
            }

            if (inputBuffer != null && inputBuffer.IsWrongLetter(letter))
            {
                GameManager.Instance?.HandleWrongLetterInput(letter);
                GameEvents.RaisePinReleased();
                return;
            }

            Vector3 spawnPosition = ResolveSpawnPosition(startPosition);
            _loadedPin = Instantiate(pinPrefab, spawnPosition, pinSpawnPoint.rotation);
            _loadedPin.transform.localScale = Vector3.one * Mathf.Max(1.18f, pinVisualScale);
            ApplyCurrentThemeSkin(_loadedPin);
            _loadedPin.Load(letter);
            if (_loadRoutine != null)
            {
                StopCoroutine(_loadRoutine);
            }

            _loadRoutine = StartCoroutine(AnimateLoadedPin(_loadedPin));
        }

        private void HandleSwipeUp()
        {
            if (_loadedPin == null || !fireGate.RequestFire())
            {
                return;
            }

            _loadedPin.transform.SetParent(null, true);
            char firedLetter = _loadedPin.CarryingLetter;
            _loadedPin.Fire(pinSpawnPoint.up);
            GameEvents.RaisePinFired(firedLetter);
            GameEvents.RaiseMetric("pinFired", $"{{\"letter\":\"{firedLetter}\"}}");
            GameEvents.RaisePinReleased();
            _loadedPin = null;
            fireGate.SetPinLoaded(false);
        }

        public void ClearLoadedPin()
        {
            if (_loadedPin == null)
            {
                return;
            }

            Destroy(_loadedPin.gameObject);
            _loadedPin = null;
            fireGate.SetPinLoaded(false);
            GameEvents.RaisePinReleased();
        }

        private IEnumerator AnimateLoadedPin(Pin pinInstance)
        {
            fireGate.SetPinLoaded(false);

            Vector3 from = pinInstance.transform.position;
            Vector3 to = pinSpawnPoint.position;
            float elapsed = 0f;

            while (elapsed < loadTweenDuration && pinInstance != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / loadTweenDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                Vector3 position = Vector3.LerpUnclamped(from, to, eased);
                position += Vector3.up * (Mathf.Sin(t * Mathf.PI) * 0.22f);
                pinInstance.transform.position = position;
                yield return null;
            }

            if (pinInstance == null)
            {
                yield break;
            }

            pinInstance.transform.position = to;
            fireGate.SetPinLoaded(true);
            GameEvents.RaisePinLoaded(pinInstance.CarryingLetter);
            _loadRoutine = null;
        }

        public void ApplyTuning(float tweenDuration, float visualScale)
        {
            loadTweenDuration = Mathf.Max(0.05f, tweenDuration);
            pinVisualScale = Mathf.Max(0.1f, visualScale);

            if (_loadedPin != null)
            {
                _loadedPin.transform.localScale = Vector3.one * Mathf.Max(1.18f, pinVisualScale);
            }
        }

        private Vector3 ResolveSpawnPosition(Vector3 sourcePosition)
        {
            if (sourcePosition == Vector3.zero)
            {
                return pinSpawnPoint.position;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                return pinSpawnPoint.position;
            }

            float depth = Mathf.Abs(camera.transform.position.z - pinSpawnPoint.position.z);
            Vector3 world = camera.ScreenToWorldPoint(new Vector3(sourcePosition.x, sourcePosition.y, depth));
            world.z = pinSpawnPoint.position.z;
            return world;
        }

        private void ApplyCurrentThemeSkin(Pin pinInstance)
        {
            if (pinInstance == null)
            {
                return;
            }

            if (_themeCatalog == null && ContentService.Instance != null)
            {
                _themeCatalog = ContentService.Instance.LoadThemes();
            }

            string themeId = FindObjectOfType<LevelFlowController>()?.CurrentLevel?.themeId;
            if (string.IsNullOrWhiteSpace(themeId) || _themeCatalog == null || _themeCatalog.themes == null)
            {
                return;
            }

            foreach (ThemePackDefinition theme in _themeCatalog.themes)
            {
                if (theme.themeId == themeId)
                {
                    pinInstance.ApplyThemeSkin(theme);
                    return;
                }
            }
        }

        public void RestorePinnedPin(char letter, Slot slot, Vector3 localPosition, float localRotationZ)
        {
            if (pinPrefab == null || slot == null)
            {
                return;
            }

            Pin restoredPin = Instantiate(pinPrefab, slot.transform);
            restoredPin.transform.localPosition = localPosition;
            restoredPin.transform.localRotation = Quaternion.Euler(0f, 0f, localRotationZ);
            restoredPin.transform.localScale = Vector3.one * Mathf.Max(1.18f, pinVisualScale);
            ApplyCurrentThemeSkin(restoredPin);
            restoredPin.Load(letter);
            restoredPin.RestoreStuckPose(slot.transform, localPosition, localRotationZ);
        }
    }
}
