using System;
using UnityEngine;

namespace WordSpinAlpha.Core
{
    [CreateAssetMenu(fileName = "ScoreTuningProfile", menuName = "WordSpin Alpha/Score Tuning Profile")]
    public class ScoreTuningProfile : ScriptableObject
    {
        [Serializable]
        public struct PerfectMultiplierTier
        {
            public int requiredPerfectStreak;
            public float multiplier;
        }

        [Serializable]
        public struct BonusWindow
        {
            public float maxSeconds;
            public int bonusPoints;
        }

        [Header("Hit Points")]
        [SerializeField] private int perfectBasePoints = 100;
        [SerializeField] private int goodBasePoints = 65;
        [SerializeField] private bool goodUsesCurrentMultiplier = true;
        [SerializeField] private bool goodResetsPerfectChain = true;

        [Header("Perfect Chain")]
        [SerializeField] private PerfectMultiplierTier[] perfectMultiplierTiers =
        {
            new PerfectMultiplierTier { requiredPerfectStreak = 1, multiplier = 1f },
            new PerfectMultiplierTier { requiredPerfectStreak = 2, multiplier = 1.5f },
            new PerfectMultiplierTier { requiredPerfectStreak = 3, multiplier = 2f },
            new PerfectMultiplierTier { requiredPerfectStreak = 5, multiplier = 3f }
        };

        [Header("Reaction Bonus")]
        [SerializeField] private BonusWindow[] reactionBonusWindows =
        {
            new BonusWindow { maxSeconds = 0.75f, bonusPoints = 40 },
            new BonusWindow { maxSeconds = 1.40f, bonusPoints = 20 }
        };

        [Header("Level Completion Bonus")]
        [SerializeField] private int levelClearBonus = 300;
        [SerializeField] private int noMistakeBonus = 200;
        [SerializeField] private int allPerfectBonus = 400;
        [SerializeField] private BonusWindow[] clearTimeBonusWindows =
        {
            new BonusWindow { maxSeconds = 10f, bonusPoints = 320 },
            new BonusWindow { maxSeconds = 16f, bonusPoints = 180 },
            new BonusWindow { maxSeconds = 24f, bonusPoints = 100 }
        };

        [Header("Future Timed Mode")]
        [SerializeField] private float defaultTimedModeLimitSeconds;
        [SerializeField] private int remainingTimeBonusPerSecond = 25;

        public int PerfectBasePoints => perfectBasePoints;
        public int GoodBasePoints => goodBasePoints;
        public bool GoodUsesCurrentMultiplier => goodUsesCurrentMultiplier;
        public bool GoodResetsPerfectChain => goodResetsPerfectChain;
        public int LevelClearBonus => levelClearBonus;
        public int NoMistakeBonus => noMistakeBonus;
        public int AllPerfectBonus => allPerfectBonus;
        public float DefaultTimedModeLimitSeconds => defaultTimedModeLimitSeconds;
        public int RemainingTimeBonusPerSecond => remainingTimeBonusPerSecond;

        public float ResolvePerfectMultiplier(int perfectStreak)
        {
            float multiplier = 1f;
            for (int i = 0; i < perfectMultiplierTiers.Length; i++)
            {
                PerfectMultiplierTier tier = perfectMultiplierTiers[i];
                if (perfectStreak >= tier.requiredPerfectStreak)
                {
                    multiplier = Mathf.Max(1f, tier.multiplier);
                }
            }

            return multiplier;
        }

        public int ResolveReactionBonus(float secondsSinceTargetShown)
        {
            return ResolveBonusFromWindow(reactionBonusWindows, secondsSinceTargetShown);
        }

        public int ResolveClearTimeBonus(float clearSeconds)
        {
            return ResolveBonusFromWindow(clearTimeBonusWindows, clearSeconds);
        }

        public int ResolveRemainingTimeBonus(float elapsedSeconds, float timedModeLimitSeconds)
        {
            float limit = timedModeLimitSeconds > 0f ? timedModeLimitSeconds : defaultTimedModeLimitSeconds;
            if (limit <= 0f || remainingTimeBonusPerSecond <= 0)
            {
                return 0;
            }

            float remaining = Mathf.Max(0f, limit - elapsedSeconds);
            return Mathf.RoundToInt(remaining * remainingTimeBonusPerSecond);
        }

        private static int ResolveBonusFromWindow(BonusWindow[] windows, float seconds)
        {
            if (windows == null || windows.Length == 0)
            {
                return 0;
            }

            float clampedSeconds = Mathf.Max(0f, seconds);
            for (int i = 0; i < windows.Length; i++)
            {
                BonusWindow window = windows[i];
                if (window.maxSeconds > 0f && clampedSeconds <= window.maxSeconds)
                {
                    return Mathf.Max(0, window.bonusPoints);
                }
            }

            return 0;
        }
    }
}
