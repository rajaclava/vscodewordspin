using System;
using System.Collections.Generic;
using UnityEngine;

namespace WordSpinAlpha.Content
{
    public static class Lang
    {
        public const string TR = "tr";
        public const string EN = "en";
        public const string ES = "es";
        public const string DE = "de";

        public static readonly string[] All = { TR, EN, ES, DE };
    }

    [Serializable]
    public class LangPack
    {
        public string tr;
        public string en;
        public string es;
        public string de;

        public string Get(string languageCode)
        {
            switch ((languageCode ?? TR).ToLowerInvariant())
            {
                case EN:
                    return string.IsNullOrEmpty(en) ? tr ?? string.Empty : en;
                case ES:
                    return string.IsNullOrEmpty(es) ? tr ?? string.Empty : es;
                case DE:
                    return string.IsNullOrEmpty(de) ? tr ?? string.Empty : de;
                default:
                    return string.IsNullOrEmpty(tr) ? en ?? string.Empty : tr;
            }
        }

        private const string TR = Lang.TR;
        private const string EN = Lang.EN;
        private const string ES = Lang.ES;
        private const string DE = Lang.DE;
    }

    [Serializable]
    public class ObstacleDefinition
    {
        public string obstacleType;
        public float angleOffset;
        public float severity;
    }

    [Serializable]
    public class QuestionDefinition
    {
        public string questionId;
        public string packId;
        public string themeId;
        public string difficultyTier;
        public LangPack text;
        public LangPack answer;
        public string infoCardId;

        public string GetQuestion(string languageCode) => text != null ? text.Get(languageCode) : string.Empty;
        public string GetAnswer(string languageCode) => answer != null ? answer.Get(languageCode) : string.Empty;

        public char[] Letters(string languageCode)
        {
            string raw = GetAnswer(languageCode);
            if (string.IsNullOrEmpty(raw))
            {
                return Array.Empty<char>();
            }

            List<char> letters = new List<char>(raw.Length);
            foreach (char character in raw)
            {
                if (!char.IsWhiteSpace(character))
                {
                    letters.Add(char.ToUpperInvariant(character));
                }
            }

            return letters.ToArray();
        }
    }

    [Serializable]
    public class LevelDefinition
    {
        public int levelId;
        public string campaignId;
        public string themeId;
        public string difficultyProfileId;
        public string difficultyTierId;
        public string rhythmProfileId;
        public string shape;
        public string shapeLayoutId;
        public float rotationSpeed;
        public bool clockwise;
        public bool randomSlots;
        public bool skipAllowed;
        public string[] questionIds;
        public ObstacleDefinition[] obstacles;
    }

    [Serializable]
    public class DifficultyProfileDefinition
    {
        public string difficultyProfileId;
        public string displayName;
        public float rotationSpeedMultiplier;
        public float perfectAngle;
        public float toleranceAngle;
        public int obstacleBudget;
        public int maxQuestionLength;
        public bool enableRandomSlots;
        public bool dopamineSpike;
        public bool breathLevel;
    }

    [Serializable]
    public class DifficultyTierDefinition
    {
        public string difficultyTierId;
        public string displayName;
        public float perfectWindowScale = 1f;
        public float goodWindowScale = 1f;
        public float nearMissScale = 1f;
        public float rotationSpeedScale = 1f;
        public float armedAssistScale = 1f;
        public float perfectChainAssistScale = 1f;
        public float waitCapScale = 1f;
    }

    [Serializable]
    public class RhythmProfileDefinition
    {
        public string rhythmProfileId;
        public string displayName;
        public float baseRotationSpeed;
        public float speedVariance;
        public string directionPattern;
        public float targetWindowLeadTime;
        public float postHitRetargetDelay;
        public float easyFlowAssist;
        public float musicSyncStrength;
        public float lightPulseStrength;
    }

    [Serializable]
    public class ShapeLayoutDefinition
    {
        public string shapeLayoutId;
        public string displayName;
        public string shapeFamily;
        public string visualPrefabResourcePath;
        public int slotCount;
        public float radiusX = 1f;
        public float radiusY = 1f;
        public float rotationOffsetDegrees;
        public float plaqueWidth = 0.30f;
        public float plaqueHeight = 0.18f;
        public float perfectWidthScale = 0.46f;
        public float perfectHeightScale = 0.45f;
        public float nearMissPadding = 0.08f;
        public bool useTangentialRotation = true;
        public float[] angleOverrides;
        public float[] pointRadiusScales;
    }

    [Serializable]
    public class ThemePackDefinition
    {
        public string themeId;
        public string displayName;
        public bool isPremium;
        public string themeCategory;
        public string campaignPackId;
        public string questionPackId;
        public string infoCardPackId;
        public string backgroundResourcePath;
        public string rotatorResourcePath;
        public string pinResourcePath;
        public string keyboardSkinResourcePath;
        public string launchVfxResourcePath;
        public string impactVfxResourcePath;
        public string completeVfxResourcePath;
        public string bgmResourcePath;
        public string hitSfxResourcePath;
        public string missSfxResourcePath;
        public string completionSfxResourcePath;
        public string uiAccentHex;
        public string uiPrimaryHex;
        public string uiBackgroundHex;
        public float hitPitchStep;
        public float entryEnergyBias;
        public float toleranceBias;
        public string mechanicalIdentity;
    }

    [Serializable]
    public class InfoCardDefinition
    {
        public string infoCardId;
        public string themeId;
        public LangPack title;
        public LangPack body;
        public string imageResourcePath;
    }

    [Serializable]
    public class CampaignPackDefinition
    {
        public string campaignId;
        public string displayName;
        public string themeId;
        public int[] levelIds;
    }

    [Serializable]
    public class ThemeCatalog
    {
        public ThemePackDefinition[] themes;
    }

    [Serializable]
    public class QuestionCatalog
    {
        public QuestionDefinition[] questions;
    }

    [Serializable]
    public class LevelCatalog
    {
        public LevelDefinition[] levels;
    }

    [Serializable]
    public class DifficultyCatalog
    {
        public DifficultyProfileDefinition[] profiles;
    }

    [Serializable]
    public class DifficultyTierCatalog
    {
        public DifficultyTierDefinition[] tiers;
    }

    [Serializable]
    public class RhythmCatalog
    {
        public RhythmProfileDefinition[] profiles;
    }

    [Serializable]
    public class InfoCardCatalog
    {
        public InfoCardDefinition[] cards;
    }

    [Serializable]
    public class CampaignCatalog
    {
        public CampaignPackDefinition[] campaigns;
    }

    [Serializable]
    public class ShapeLayoutCatalog
    {
        public ShapeLayoutDefinition[] layouts;
    }

    [Serializable]
    public class ThemePriceDefinition
    {
        public string themeId;
        public int softCurrencyPrice;
        public string iapProductId;
    }

    [Serializable]
    public class EnergyPackDefinition
    {
        public string productId;
        public int energyAmount;
        public int gemPrice;
    }

    [Serializable]
    public class HintPackDefinition
    {
        public string productId;
        public int hintAmount;
        public int gemPrice;
    }

    [Serializable]
    public class StoreCatalogDefinition
    {
        public ThemePriceDefinition[] themes;
        public EnergyPackDefinition[] energyPacks;
        public HintPackDefinition[] hintPacks;
        public string noAdsProductId;
        public string premiumMembershipProductId;
    }

    [Serializable]
    public class MembershipProfileDefinition
    {
        public string membershipId;
        public bool removeAds;
        public bool unlimitedEntryEnergy;
        public bool unlockFutureThemes;
    }

    [Serializable]
    public class EnergyConfigDefinition
    {
        public int maxEnergy;
        public int refillMinutes;
        public bool bypassForPremiumMembership;
        public int startingHints;
        public int startingSoftCurrency;
    }

    [Serializable]
    public class KeyboardLayoutDefinition
    {
        public string languageCode;
        public string[] keys;
    }

    [Serializable]
    public class KeyboardConfigDefinition
    {
        public KeyboardLayoutDefinition[] layouts;
    }

    [Serializable]
    public class RemoteContentManifestDefinition
    {
        public string manifestVersion;
        public string minClientVersion;
        public string publishedAtUtc;
        public bool remoteContentEnabled = true;
        public string[] availableCatalogs;
    }
}
