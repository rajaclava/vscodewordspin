using System;
using UnityEngine;

namespace WordSpinAlpha.Core
{
    public enum HitResultType
    {
        Perfect,
        Tolerated,
        NearMiss,
        WrongSlot,
        WrongLetter,
        Miss
    }

    [Serializable]
    public struct LevelContext
    {
        public int levelId;
        public string campaignId;
        public string themeId;
        public string difficultyProfileId;
        public string rhythmProfileId;
        public string languageCode;
        public int questionIndex;
        public int totalQuestions;
        public int obstacleBudget;
        public bool dopamineSpike;
        public bool breathLevel;
    }

    [Serializable]
    public struct QuestionContext
    {
        public string questionId;
        public int levelId;
        public int questionIndex;
        public string languageCode;
        public int totalLetters;
    }

    [Serializable]
    public struct HitData
    {
        public HitResultType resultType;
        public int slotIndex;
        public int expectedSlotIndex;
        public char enteredLetter;
        public char expectedLetter;
        public float exactAngle;
        public float precisionScore;
    }

    [Serializable]
    public struct ImpactEventData
    {
        public HitResultType impactType;
        public Vector3 impactWorldPos;
        public float impactSpeed;
        public float accuracy;
        public bool correctTarget;
        public bool correctLetter;
        public int combo;
        public int slotIndex;
        public int expectedSlotIndex;
    }

    [Serializable]
    public struct ResolvedImpactFeelData
    {
        public HitResultType impactType;
        public Vector3 impactWorldPos;
        public float hitStopMs;
        public float hapticIntensity;
        public float hapticSharpness;
        public float audioAttack;
        public float cameraKick;
        public float settleMs;
        public float flashScale;
        public float particleBurst;
        public float accuracy;
        public float impactSpeed;
        public int combo;
    }

    [Serializable]
    public struct RhythmFlowStateData
    {
        public float flowIntensity;
        public int perfectMomentumLevel;
        public bool isPlayerArmed;
        public bool isPerfectChainWindowActive;
    }

    [Serializable]
    public struct ScoreStateData
    {
        public int totalScore;
        public int lastAward;
        public int basePoints;
        public int reactionBonus;
        public int clearBonus;
        public float multiplier;
        public int perfectStreak;
        public HitResultType lastHitType;
    }

    [Serializable]
    public struct LevelScoreSummaryData
    {
        public int levelId;
        public int totalScore;
        public int hitScore;
        public int clearBonus;
        public int noMistakeBonus;
        public int allPerfectBonus;
        public int timeBonus;
        public float bestMultiplier;
        public int perfectHits;
        public int goodHits;
        public int mistakeCount;
        public float elapsedSeconds;
    }

    [Serializable]
    public struct LevelEconomySummaryData
    {
        public int levelId;
        public int starsEarned;
        public int baseCoinReward;
        public int coinReward;
        public int adBonusCoins;
        public bool adBonusEligible;
        public bool firstClear;
        public bool continueUsed;
        public int completionCount;
    }

    [Serializable]
    public struct FailModalContext
    {
        public int levelId;
        public int questionIndex;
        public int restoreHeartsOnContinue;
        public bool premiumContinueAvailable;
    }

    public static class GameEvents
    {
        public static event Action<LevelContext> LevelStarted;
        public static event Action<QuestionContext, string> QuestionStarted;
        public static event Action<int, char> LetterRevealed;
        public static event Action<int, int, char> TargetSlotUpdated;
        public static event Action<char> PinLoaded;
        public static event Action<char> PinFired;
        public static event Action PinReleased;
        public static event Action<HitData> HitEvaluated;
        public static event Action<ImpactEventData> ImpactOccurred;
        public static event Action<ResolvedImpactFeelData> ImpactFeelResolved;
        public static event Action<RhythmFlowStateData> RhythmFlowStateChanged;
        public static event Action<ScoreStateData> ScoreChanged;
        public static event Action<LevelScoreSummaryData> LevelScoreFinalized;
        public static event Action<LevelEconomySummaryData> LevelEconomyFinalized;
        public static event Action<int> QuestionHeartsChanged;
        public static event Action QuestionFailed;
        public static event Action<QuestionContext> QuestionCompleted;
        public static event Action<LevelContext> LevelCompleted;
        public static event Action<string> InfoCardRequested;
        public static event Action InfoCardClosed;
        public static event Action<FailModalContext> FailModalRequested;
        public static event Action<int, int> EntryEnergyChanged;
        public static event Action<int, int> SoftCurrencyChanged;
        public static event Action<string> ThemeUnlocked;
        public static event Action<bool> MembershipChanged;
        public static event Action<bool> LevelContinueUsed;
        public static event Action<string, string> MetricEventRaised;
        public static event Action<string> LanguageChanged;

        public static void RaiseLevelStarted(LevelContext context) => LevelStarted?.Invoke(context);
        public static void RaiseQuestionStarted(QuestionContext context, string answerWord) => QuestionStarted?.Invoke(context, answerWord);
        public static void RaiseLetterRevealed(int index, char value) => LetterRevealed?.Invoke(index, value);
        public static void RaiseTargetSlotUpdated(int slotIndex, int answerIndex, char letter) => TargetSlotUpdated?.Invoke(slotIndex, answerIndex, letter);
        public static void RaisePinLoaded(char letter) => PinLoaded?.Invoke(letter);
        public static void RaisePinFired(char letter) => PinFired?.Invoke(letter);
        public static void RaisePinReleased() => PinReleased?.Invoke();
        public static void RaiseHitEvaluated(HitData hit) => HitEvaluated?.Invoke(hit);
        public static void RaiseImpactOccurred(ImpactEventData impact) => ImpactOccurred?.Invoke(impact);
        public static void RaiseImpactFeelResolved(ResolvedImpactFeelData feel) => ImpactFeelResolved?.Invoke(feel);
        public static void RaiseRhythmFlowStateChanged(RhythmFlowStateData state) => RhythmFlowStateChanged?.Invoke(state);
        public static void RaiseScoreChanged(ScoreStateData state) => ScoreChanged?.Invoke(state);
        public static void RaiseLevelScoreFinalized(LevelScoreSummaryData summary) => LevelScoreFinalized?.Invoke(summary);
        public static void RaiseLevelEconomyFinalized(LevelEconomySummaryData summary) => LevelEconomyFinalized?.Invoke(summary);
        public static void RaiseQuestionHeartsChanged(int hearts) => QuestionHeartsChanged?.Invoke(hearts);
        public static void RaiseQuestionFailed() => QuestionFailed?.Invoke();
        public static void RaiseQuestionCompleted(QuestionContext context) => QuestionCompleted?.Invoke(context);
        public static void RaiseLevelCompleted(LevelContext context) => LevelCompleted?.Invoke(context);
        public static void RaiseInfoCardRequested(string infoCardId) => InfoCardRequested?.Invoke(infoCardId);
        public static void RaiseInfoCardClosed() => InfoCardClosed?.Invoke();
        public static void RaiseFailModalRequested(FailModalContext context) => FailModalRequested?.Invoke(context);
        public static void RaiseEntryEnergyChanged(int current, int max) => EntryEnergyChanged?.Invoke(current, max);
        public static void RaiseSoftCurrencyChanged(int current, int delta) => SoftCurrencyChanged?.Invoke(current, delta);
        public static void RaiseThemeUnlocked(string themeId) => ThemeUnlocked?.Invoke(themeId);
        public static void RaiseMembershipChanged(bool isActive) => MembershipChanged?.Invoke(isActive);
        public static void RaiseLevelContinueUsed(bool usedPremiumContinue) => LevelContinueUsed?.Invoke(usedPremiumContinue);
        public static void RaiseMetric(string eventName, string payload) => MetricEventRaised?.Invoke(eventName, payload);
        public static void RaiseLanguageChanged(string languageCode) => LanguageChanged?.Invoke(languageCode);
    }
}
