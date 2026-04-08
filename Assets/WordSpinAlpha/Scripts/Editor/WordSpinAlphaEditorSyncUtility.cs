using System;

namespace WordSpinAlpha.Editor
{
    [Flags]
    internal enum WordSpinAlphaEditorSyncKind
    {
        None = 0,
        Scene = 1 << 0,
        RuntimeConfig = 1 << 1,
        Content = 1 << 2,
        ScriptableAssets = 1 << 3,
        Telemetry = 1 << 4
    }

    internal struct WordSpinAlphaEditorSyncStamp
    {
        public int sceneRevision;
        public int runtimeConfigRevision;
        public int contentRevision;
        public int scriptableAssetRevision;
        public int telemetryRevision;
    }

    internal static class WordSpinAlphaEditorSyncUtility
    {
        private static int _sceneRevision;
        private static int _runtimeConfigRevision;
        private static int _contentRevision;
        private static int _scriptableAssetRevision;
        private static int _telemetryRevision;

        public static WordSpinAlphaEditorSyncStamp CaptureCurrentStamp()
        {
            return new WordSpinAlphaEditorSyncStamp
            {
                sceneRevision = _sceneRevision,
                runtimeConfigRevision = _runtimeConfigRevision,
                contentRevision = _contentRevision,
                scriptableAssetRevision = _scriptableAssetRevision,
                telemetryRevision = _telemetryRevision
            };
        }

        public static void NotifyChanged(WordSpinAlphaEditorSyncKind kind)
        {
            if ((kind & WordSpinAlphaEditorSyncKind.Scene) != 0)
            {
                _sceneRevision++;
            }

            if ((kind & WordSpinAlphaEditorSyncKind.RuntimeConfig) != 0)
            {
                _runtimeConfigRevision++;
            }

            if ((kind & WordSpinAlphaEditorSyncKind.Content) != 0)
            {
                _contentRevision++;
            }

            if ((kind & WordSpinAlphaEditorSyncKind.ScriptableAssets) != 0)
            {
                _scriptableAssetRevision++;
            }

            if ((kind & WordSpinAlphaEditorSyncKind.Telemetry) != 0)
            {
                _telemetryRevision++;
            }
        }

        public static bool ConsumeChanges(WordSpinAlphaEditorSyncKind watchedKinds, ref WordSpinAlphaEditorSyncStamp stamp)
        {
            bool changed = false;

            if ((watchedKinds & WordSpinAlphaEditorSyncKind.Scene) != 0 && stamp.sceneRevision != _sceneRevision)
            {
                stamp.sceneRevision = _sceneRevision;
                changed = true;
            }

            if ((watchedKinds & WordSpinAlphaEditorSyncKind.RuntimeConfig) != 0 && stamp.runtimeConfigRevision != _runtimeConfigRevision)
            {
                stamp.runtimeConfigRevision = _runtimeConfigRevision;
                changed = true;
            }

            if ((watchedKinds & WordSpinAlphaEditorSyncKind.Content) != 0 && stamp.contentRevision != _contentRevision)
            {
                stamp.contentRevision = _contentRevision;
                changed = true;
            }

            if ((watchedKinds & WordSpinAlphaEditorSyncKind.ScriptableAssets) != 0 && stamp.scriptableAssetRevision != _scriptableAssetRevision)
            {
                stamp.scriptableAssetRevision = _scriptableAssetRevision;
                changed = true;
            }

            if ((watchedKinds & WordSpinAlphaEditorSyncKind.Telemetry) != 0 && stamp.telemetryRevision != _telemetryRevision)
            {
                stamp.telemetryRevision = _telemetryRevision;
                changed = true;
            }

            return changed;
        }
    }
}
