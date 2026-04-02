using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Editor
{
    public static class WordSpinAlphaLevelGenerator
    {
        private const string QuestionsPath = "Assets/WordSpinAlpha/Resources/Content/questions.json";
        private const string LevelsPath = "Assets/WordSpinAlpha/Resources/Content/levels.json";
        private const string BackupPath = "Assets/WordSpinAlpha/Resources/Content/levels.autobackup.json";
        private const string LocalesRoot = "Assets/WordSpinAlpha/Resources/Content/Locales";

        [MenuItem("Tools/WordSpin Alpha/Generate Levels From Questions")]
        public static void GenerateLevelsFromQuestions() => GenerateLevelsForLocale(GameConstants.DefaultLanguageCode);

        [MenuItem("Tools/WordSpin Alpha/Generate Levels/For Turkish")]
        public static void GenerateLevelsForTurkish() => GenerateLevelsForLocale("tr");

        [MenuItem("Tools/WordSpin Alpha/Generate Levels/For English")]
        public static void GenerateLevelsForEnglish() => GenerateLevelsForLocale("en");

        [MenuItem("Tools/WordSpin Alpha/Generate Levels/For Spanish")]
        public static void GenerateLevelsForSpanish() => GenerateLevelsForLocale("es");

        [MenuItem("Tools/WordSpin Alpha/Generate Levels/For German")]
        public static void GenerateLevelsForGerman() => GenerateLevelsForLocale("de");

        private static void GenerateLevelsForLocale(string languageCode)
        {
            string normalized = GameConstants.NormalizeLanguageCode(languageCode);
            string questionsPath = normalized == GameConstants.DefaultLanguageCode ? QuestionsPath : $"{LocalesRoot}/{normalized}/questions.json";
            string levelsPath = normalized == GameConstants.DefaultLanguageCode ? LevelsPath : $"{LocalesRoot}/{normalized}/levels.json";
            string backupPath = normalized == GameConstants.DefaultLanguageCode ? BackupPath : $"{LocalesRoot}/{normalized}/levels.autobackup.json";

            TextAsset questionsAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(questionsPath);
            if (questionsAsset == null)
            {
                Debug.LogError($"[WordSpinAlpha] Questions file not found: {questionsPath}");
                return;
            }

            QuestionCatalog questions = JsonUtility.FromJson<QuestionCatalog>(questionsAsset.text);
            if (questions == null || questions.questions == null || questions.questions.Length == 0)
            {
                Debug.LogError($"[WordSpinAlpha] Question catalog is empty for locale '{normalized}'. Level generation cancelled.");
                return;
            }

            EnsureFolder(Path.GetDirectoryName(levelsPath)?.Replace("\\", "/"));
            if (File.Exists(levelsPath))
            {
                File.Copy(levelsPath, backupPath, true);
            }

            LevelCatalog generated = new LevelCatalog
            {
                levels = new LevelDefinition[questions.questions.Length]
            };

            for (int i = 0; i < questions.questions.Length; i++)
            {
                int levelId = i + 1;
                QuestionDefinition question = questions.questions[i];
                generated.levels[i] = BuildLevelDefinition(levelId, question);
            }

            File.WriteAllText(levelsPath, JsonUtility.ToJson(generated, true));
            AssetDatabase.Refresh();
            Debug.Log($"[WordSpinAlpha] Generated {generated.levels.Length} levels for locale '{normalized}'. Backup: {backupPath}");
        }

        private static LevelDefinition BuildLevelDefinition(int levelId, QuestionDefinition question)
        {
            string difficultyBand = NormalizeDifficultyBand(question != null ? question.difficultyTier : string.Empty);
            string shapeLayoutId = ResolveShapeLayoutId(difficultyBand, levelId);

            return new LevelDefinition
            {
                levelId = levelId,
                campaignId = ResolveCampaignId(question),
                themeId = string.IsNullOrWhiteSpace(question != null ? question.themeId : string.Empty) ? "base_default" : question.themeId,
                difficultyProfileId = ResolveDifficultyProfileId(difficultyBand),
                difficultyTierId = ResolveDifficultyTierId(difficultyBand, levelId),
                rhythmProfileId = ResolveRhythmProfileId(difficultyBand),
                shape = ResolveLegacyShape(shapeLayoutId),
                shapeLayoutId = shapeLayoutId,
                rotationSpeed = ResolveRotationSpeed(difficultyBand, levelId),
                clockwise = levelId % 2 == 1,
                randomSlots = UsesRandomSlots(difficultyBand, levelId),
                skipAllowed = AllowsSkip(difficultyBand),
                questionIds = new[] { question.questionId },
                obstacles = BuildObstacles(difficultyBand, levelId)
            };
        }

        private static string ResolveCampaignId(QuestionDefinition question)
        {
            if (question == null || string.IsNullOrWhiteSpace(question.themeId))
            {
                return "alpha_main";
            }

            switch (question.themeId)
            {
                case "myth_hades":
                    return "alpha_main";
                default:
                    return "alpha_main";
            }
        }

        private static string NormalizeDifficultyBand(string raw)
        {
            string value = string.IsNullOrWhiteSpace(raw) ? "hook" : raw.Trim().ToLowerInvariant();
            switch (value)
            {
                case "hook":
                case "rhythm":
                case "variation":
                case "pressure":
                case "showcase":
                    return value;
                default:
                    return "variation";
            }
        }

        private static string ResolveDifficultyProfileId(string difficultyBand)
        {
            switch (difficultyBand)
            {
                case "hook":
                    return "hook";
                case "rhythm":
                    return "rhythm";
                case "variation":
                    return "variation";
                case "pressure":
                    return "pressure";
                case "showcase":
                    return "showcase";
                default:
                    return "variation";
            }
        }

        private static string ResolveDifficultyTierId(string difficultyBand, int levelId)
        {
            switch (difficultyBand)
            {
                case "hook":
                    return "intro_perfect";
                case "rhythm":
                    return levelId <= 10 ? "intro_perfect" : "early_flow";
                case "variation":
                    return "early_flow";
                case "pressure":
                    return "mid_pressure";
                case "showcase":
                    return levelId >= 21 ? "late_variation" : "mid_pressure";
                default:
                    return "early_flow";
            }
        }

        private static string ResolveRhythmProfileId(string difficultyBand)
        {
            switch (difficultyBand)
            {
                case "hook":
                    return "hook";
                case "rhythm":
                    return "rhythm";
                case "variation":
                    return "variation";
                case "pressure":
                    return "pressure";
                case "showcase":
                    return "showcase";
                default:
                    return "variation";
            }
        }

        private static string ResolveShapeLayoutId(string difficultyBand, int levelId)
        {
            switch (difficultyBand)
            {
                case "hook":
                    return SelectByLevel(levelId, "circle_classic", "oval_flow", "shield_guard", "diamond_drive", "square_lock");
                case "rhythm":
                    return SelectByLevel(levelId, "hex_bloom", "crown_arc", "oval_flow", "shield_guard", "diamond_drive");
                case "variation":
                    return SelectByLevel(levelId, "diamond_drive", "square_lock", "hex_bloom", "crown_arc", "oval_flow");
                case "pressure":
                    return SelectByLevel(levelId, "shield_guard", "diamond_drive", "square_lock", "hex_bloom", "crown_arc");
                case "showcase":
                    return SelectByLevel(levelId, "oval_flow", "circle_classic", "shield_guard", "diamond_drive", "hex_bloom", "crown_arc", "square_lock");
                default:
                    return "circle_classic";
            }
        }

        private static string SelectByLevel(int levelId, params string[] options)
        {
            if (options == null || options.Length == 0)
            {
                return "circle_classic";
            }

            int index = Mathf.Abs(levelId - 1) % options.Length;
            return options[index];
        }

        private static string ResolveLegacyShape(string shapeLayoutId)
        {
            switch (shapeLayoutId)
            {
                case "circle_classic":
                    return "circle";
                case "oval_flow":
                    return "oval";
                case "diamond_drive":
                    return "diamond";
                case "square_lock":
                    return "square";
                case "hex_bloom":
                    return "hex";
                case "shield_guard":
                    return "shield";
                case "crown_arc":
                    return "crown";
                default:
                    return "circle";
            }
        }

        private static float ResolveRotationSpeed(string difficultyBand, int levelId)
        {
            switch (difficultyBand)
            {
                case "hook":
                    return Mathf.Clamp(38f + (levelId * 2f), 38f, 52f);
                case "rhythm":
                    return Mathf.Clamp(48f + (levelId * 1.8f), 48f, 64f);
                case "variation":
                    return Mathf.Clamp(58f + (levelId * 1.7f), 58f, 76f);
                case "pressure":
                    return Mathf.Clamp(68f + (levelId * 1.6f), 68f, 88f);
                case "showcase":
                    return Mathf.Clamp(80f + ((levelId - 20) * 1.9f), 80f, 96f);
                default:
                    return 60f;
            }
        }

        private static bool UsesRandomSlots(string difficultyBand, int levelId)
        {
            switch (difficultyBand)
            {
                case "variation":
                case "pressure":
                case "showcase":
                    return levelId == 12 || levelId == 15 || levelId == 18 || levelId == 21 || levelId == 24;
                default:
                    return false;
            }
        }

        private static bool AllowsSkip(string difficultyBand)
        {
            return difficultyBand != "hook";
        }

        private static ObstacleDefinition[] BuildObstacles(string difficultyBand, int levelId)
        {
            return Array.Empty<ObstacleDefinition>();
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
