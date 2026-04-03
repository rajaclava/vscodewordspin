using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    public class DebugRewardedAdPresenter : Singleton<DebugRewardedAdPresenter>
    {
        private Canvas _canvas;
        private GameObject _root;
        private TextMeshProUGUI _titleLabel;
        private TextMeshProUGUI _bodyLabel;
        private TextMeshProUGUI _countdownLabel;
        private Coroutine _activeRoutine;
        private Action _onCompleted;

        protected override bool PersistAcrossScenes => true;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            EnsureUi();
            HideImmediate();
        }

        public void ShowCountdown(int seconds, Action onCompleted)
        {
            EnsureUi();
            HideImmediate();

            _onCompleted = onCompleted;
            if (_root != null)
            {
                _root.SetActive(true);
            }

            _activeRoutine = StartCoroutine(RunCountdown(Mathf.Max(1, seconds)));
        }

        private IEnumerator RunCountdown(int seconds)
        {
            for (int remaining = seconds; remaining > 0; remaining--)
            {
                if (_countdownLabel != null)
                {
                    _countdownLabel.text = $"{remaining}s";
                }

                if (_bodyLabel != null)
                {
                    _bodyLabel.text = GetLocalized("body");
                }

                yield return new WaitForSecondsRealtime(1f);
            }

            Action callback = _onCompleted;
            HideImmediate();
            callback?.Invoke();
        }

        private void HideImmediate()
        {
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }

            _onCompleted = null;
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private void EnsureUi()
        {
            if (_root != null && _titleLabel != null && _bodyLabel != null && _countdownLabel != null)
            {
                RefreshLocalizedText();
                return;
            }

            if (_canvas == null)
            {
                GameObject canvasObject = new GameObject("DebugRewardedAdCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObject.transform.SetParent(transform, false);
                _canvas = canvasObject.GetComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 4000;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 1f;
            }

            if (_root == null)
            {
                _root = new GameObject("DebugRewardedAdRoot", typeof(RectTransform), typeof(Image));
                _root.transform.SetParent(_canvas.transform, false);
                RectTransform rootRect = _root.GetComponent<RectTransform>();
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
                Image background = _root.GetComponent<Image>();
                background.color = new Color(0.03f, 0.03f, 0.05f, 0.92f);
                background.raycastTarget = true;
            }

            GameObject panel = CreateOrFindChild(_root.transform, "Panel", typeof(RectTransform), typeof(Image));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(760f, 520f);
            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.12f, 0.12f, 0.16f, 0.98f);

            _titleLabel = EnsureLabel(panel.transform, "Title", new Vector2(0f, 154f), new Vector2(620f, 56f), 38f);
            _bodyLabel = EnsureLabel(panel.transform, "Body", new Vector2(0f, 18f), new Vector2(620f, 160f), 28f);
            _countdownLabel = EnsureLabel(panel.transform, "Countdown", new Vector2(0f, -136f), new Vector2(300f, 82f), 54f);

            _titleLabel.color = new Color(1f, 0.85f, 0.60f);
            _bodyLabel.color = Color.white;
            _bodyLabel.enableWordWrapping = true;
            _countdownLabel.color = new Color(0.96f, 0.76f, 0.36f);
            RefreshLocalizedText();
        }

        private void RefreshLocalizedText()
        {
            if (_titleLabel != null)
            {
                _titleLabel.text = GetLocalized("title");
            }

            if (_bodyLabel != null)
            {
                _bodyLabel.text = GetLocalized("body");
            }
        }

        private static GameObject CreateOrFindChild(Transform parent, string name, params Type[] components)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = new GameObject(name, components);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static TextMeshProUGUI EnsureLabel(Transform parent, string name, Vector2 position, Vector2 size, float fontSize)
        {
            GameObject go = CreateOrFindChild(parent, name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            return label;
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
                        "title" => "Rewarded Ad Test",
                        "body" => "This is a fake rewarded-ad flow for free-player testing. Countdown will finish automatically.",
                        _ => key
                    };
                case "es":
                    return key switch
                    {
                        "title" => "Prueba de anuncio recompensado",
                        "body" => "Este es un flujo falso de anuncio recompensado para probar al jugador free. La cuenta atras terminara sola.",
                        _ => key
                    };
                case "de":
                    return key switch
                    {
                        "title" => "Belohnungsanzeigen-Test",
                        "body" => "Dies ist ein falscher Rewarded-Ad-Ablauf fuer Free-Spieler-Tests. Der Countdown endet automatisch.",
                        _ => key
                    };
                default:
                    return key switch
                    {
                        "title" => "Odullu Reklam Testi",
                        "body" => "Bu ekran free oyuncu akisini test etmek icin sahte reklam akisidir. Sayac otomatik biter.",
                        _ => key
                    };
            }
        }
    }
}
