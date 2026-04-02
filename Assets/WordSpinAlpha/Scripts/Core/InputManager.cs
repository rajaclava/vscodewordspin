using System;
using UnityEngine;
using UnityEngine.EventSystems;
using WordSpinAlpha.Content;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Core
{
    public class InputManager : Singleton<InputManager>
    {
        [SerializeField] private bool allowPhysicalKeyboard = true;
        [SerializeField] private float minSwipeDistance = 30f;

        private Vector2 _swipeStart;
        private bool _trackingSwipe;
        private KeyboardConfigDefinition _keyboardConfig;

        public event Action<char, Vector3> LetterPressed;
        public event Action SwipeUpRequested;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            EnsureKeyboardConfig();
        }

        private void Update()
        {
            HandleKeyboardInput();
            HandleSwipeInput();
        }

        public string[] GetLayout(string languageCode)
        {
            EnsureKeyboardConfig();

            if (_keyboardConfig == null || _keyboardConfig.layouts == null)
            {
                return Array.Empty<string>();
            }

            foreach (KeyboardLayoutDefinition layout in _keyboardConfig.layouts)
            {
                if (string.Equals(layout.languageCode, languageCode, StringComparison.OrdinalIgnoreCase))
                {
                    return layout.keys ?? Array.Empty<string>();
                }
            }

            return Array.Empty<string>();
        }

        private void EnsureKeyboardConfig()
        {
            if (_keyboardConfig == null && ContentService.Instance != null)
            {
                _keyboardConfig = ContentService.Instance.LoadKeyboardConfig();
            }
        }

        public void RefreshKeyboardConfig()
        {
            _keyboardConfig = ContentService.Instance != null ? ContentService.Instance.LoadKeyboardConfig() : null;
        }

        public void ProcessLetterButton(char letter, Vector3 worldPosition)
        {
            LetterPressed?.Invoke(char.ToUpperInvariant(letter), worldPosition);
        }

        private void HandleKeyboardInput()
        {
            if (!allowPhysicalKeyboard)
            {
                return;
            }

            for (KeyCode keyCode = KeyCode.A; keyCode <= KeyCode.Z; keyCode++)
            {
                if (!Input.GetKeyDown(keyCode))
                {
                    continue;
                }

                char letter = (char)('A' + (keyCode - KeyCode.A));
                LetterPressed?.Invoke(letter, Vector3.zero);
            }
        }

        private void HandleSwipeInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    _swipeStart = touch.position;
                    _trackingSwipe = true;
                }
                else if (touch.phase == TouchPhase.Ended && _trackingSwipe)
                {
                    ResolveSwipe(touch.position);
                    _trackingSwipe = false;
                }

                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    _trackingSwipe = false;
                    return;
                }

                _swipeStart = Input.mousePosition;
                _trackingSwipe = true;
            }
            else if (Input.GetMouseButtonUp(0) && _trackingSwipe)
            {
                ResolveSwipe(Input.mousePosition);
                _trackingSwipe = false;
            }
        }

        private void ResolveSwipe(Vector2 endPosition)
        {
            float deltaY = endPosition.y - _swipeStart.y;
            float deltaX = Mathf.Abs(endPosition.x - _swipeStart.x);
            if (deltaY >= minSwipeDistance && deltaY > deltaX)
            {
                SwipeUpRequested?.Invoke();
            }
        }
    }
}
