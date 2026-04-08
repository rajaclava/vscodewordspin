using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WordSpinAlpha.Editor
{
    internal static class WordSpinAlphaEditorUiTuningUtility
    {
        public static T ResolveReference<T>(SerializedObject source, string propertyName) where T : Object
        {
            if (source == null)
            {
                return null;
            }

            SerializedProperty property = source.FindProperty(propertyName);
            return property != null ? property.objectReferenceValue as T : null;
        }

        public static bool DrawRectTransformBlock(string title, RectTransform rectTransform, bool includeAnchors = true, bool includePivot = true, bool includeScale = true)
        {
            if (rectTransform == null)
            {
                EditorGUILayout.HelpBox($"{title} bulunamadi.", MessageType.Warning);
                return false;
            }

            bool changed = false;
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            if (includeAnchors)
            {
                changed |= SetVector2(rectTransform, "Anchor Min", rectTransform.anchorMin, value => rectTransform.anchorMin = value);
                changed |= SetVector2(rectTransform, "Anchor Max", rectTransform.anchorMax, value => rectTransform.anchorMax = value);
            }

            changed |= SetVector2(rectTransform, "Anchored Position", rectTransform.anchoredPosition, value => rectTransform.anchoredPosition = value);
            changed |= SetVector2(rectTransform, "Size Delta", rectTransform.sizeDelta, value => rectTransform.sizeDelta = value);

            if (includePivot)
            {
                changed |= SetVector2(rectTransform, "Pivot", rectTransform.pivot, value => rectTransform.pivot = value);
            }

            if (includeScale)
            {
                changed |= SetVector3(rectTransform, "Local Scale", rectTransform.localScale, value => rectTransform.localScale = value);
            }

            return changed;
        }

        public static bool DrawTextBlock(string title, TextMeshProUGUI text, bool includeRect = true)
        {
            if (text == null)
            {
                EditorGUILayout.HelpBox($"{title} bulunamadi.", MessageType.Warning);
                return false;
            }

            bool changed = false;
            if (includeRect)
            {
                changed |= DrawRectTransformBlock($"{title} / Yerlesim", text.rectTransform, includeAnchors: true, includePivot: false, includeScale: false);
            }

            EditorGUILayout.LabelField($"{title} / Metin", EditorStyles.boldLabel);
            changed |= SetFloat(text, "Font Size", text.fontSize, value => text.fontSize = value);
            changed |= SetBool(text, "Auto Size", text.enableAutoSizing, value => text.enableAutoSizing = value);
            if (text.enableAutoSizing)
            {
                changed |= SetFloat(text, "Min Font Size", text.fontSizeMin, value => text.fontSizeMin = value);
                changed |= SetFloat(text, "Max Font Size", text.fontSizeMax, value => text.fontSizeMax = value);
            }

            changed |= SetColor(text, "Renk", text.color, value => text.color = value);
            changed |= SetEnum(text, "Alignment", text.alignment, value => text.alignment = value);
            changed |= SetBool(text, "Word Wrap", text.enableWordWrapping, value => text.enableWordWrapping = value);
            changed |= SetFloat(text, "Line Spacing", text.lineSpacing, value => text.lineSpacing = value);
            changed |= SetFloat(text, "Character Spacing", text.characterSpacing, value => text.characterSpacing = value);
            changed |= SetEnum(text, "Overflow", text.overflowMode, value => text.overflowMode = value);
            return changed;
        }

        public static bool DrawImageBlock(string title, Image image)
        {
            if (image == null)
            {
                EditorGUILayout.HelpBox($"{title} bulunamadi.", MessageType.Warning);
                return false;
            }

            bool changed = DrawRectTransformBlock($"{title} / Yerlesim", image.rectTransform, includeAnchors: true, includePivot: false, includeScale: false);
            EditorGUILayout.LabelField($"{title} / Gorsel", EditorStyles.boldLabel);
            changed |= SetColor(image, "Renk", image.color, value => image.color = value);
            return changed;
        }

        public static bool DrawButtonBlock(string title, Button button, TextMeshProUGUI label)
        {
            if (button == null)
            {
                EditorGUILayout.HelpBox($"{title} bulunamadi.", MessageType.Warning);
                return false;
            }

            bool changed = false;
            changed |= DrawRectTransformBlock($"{title} / Buton", button.transform as RectTransform, includeAnchors: true, includePivot: false, includeScale: false);
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                changed |= SetColor(image, "Buton Rengi", image.color, value => image.color = value);
            }

            if (label != null)
            {
                changed |= DrawTextBlock($"{title} / Yazi", label, includeRect: false);
            }

            return changed;
        }

        private static bool SetVector2(Object target, string label, Vector2 currentValue, System.Action<Vector2> setter)
        {
            Vector2 value = EditorGUILayout.Vector2Field(label, currentValue);
            if (value == currentValue)
            {
                return false;
            }

            Touch(target, label);
            setter(value);
            return true;
        }

        private static bool SetVector3(Object target, string label, Vector3 currentValue, System.Action<Vector3> setter)
        {
            Vector3 value = EditorGUILayout.Vector3Field(label, currentValue);
            if (value == currentValue)
            {
                return false;
            }

            Touch(target, label);
            setter(value);
            return true;
        }

        private static bool SetFloat(Object target, string label, float currentValue, System.Action<float> setter)
        {
            float value = EditorGUILayout.FloatField(label, currentValue);
            if (Mathf.Approximately(value, currentValue))
            {
                return false;
            }

            Touch(target, label);
            setter(value);
            return true;
        }

        private static bool SetBool(Object target, string label, bool currentValue, System.Action<bool> setter)
        {
            bool value = EditorGUILayout.Toggle(label, currentValue);
            if (value == currentValue)
            {
                return false;
            }

            Touch(target, label);
            setter(value);
            return true;
        }

        private static bool SetColor(Object target, string label, Color currentValue, System.Action<Color> setter)
        {
            Color value = EditorGUILayout.ColorField(label, currentValue);
            if (value == currentValue)
            {
                return false;
            }

            Touch(target, label);
            setter(value);
            return true;
        }

        private static bool SetEnum<TEnum>(Object target, string label, TEnum currentValue, System.Action<TEnum> setter) where TEnum : System.Enum
        {
            TEnum value = (TEnum)EditorGUILayout.EnumPopup(label, currentValue);
            if (System.Collections.Generic.EqualityComparer<TEnum>.Default.Equals(value, currentValue))
            {
                return false;
            }

            Touch(target, label);
            setter(value);
            return true;
        }

        private static void Touch(Object target, string actionName)
        {
            Undo.RecordObject(target, $"UI Tuning {actionName}");
            EditorUtility.SetDirty(target);
            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.Scene);
        }
    }
}
