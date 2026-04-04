using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Content;

namespace WordSpinAlpha.Editor
{
    [Serializable]
    public class LocalizedValueSet
    {
        public string tr = string.Empty;
        public string en = string.Empty;
        public string es = string.Empty;
        public string de = string.Empty;

        public string Get(string languageCode)
        {
            switch ((languageCode ?? Lang.TR).ToLowerInvariant())
            {
                case Lang.EN:
                    return en ?? string.Empty;
                case Lang.ES:
                    return es ?? string.Empty;
                case Lang.DE:
                    return de ?? string.Empty;
                default:
                    return tr ?? string.Empty;
            }
        }

        public void Set(string languageCode, string value)
        {
            switch ((languageCode ?? Lang.TR).ToLowerInvariant())
            {
                case Lang.EN:
                    en = value ?? string.Empty;
                    break;
                case Lang.ES:
                    es = value ?? string.Empty;
                    break;
                case Lang.DE:
                    de = value ?? string.Empty;
                    break;
                default:
                    tr = value ?? string.Empty;
                    break;
            }
        }
    }

    [Serializable]
    public class LevelQuestionEditorEntry
    {
        public string questionId = string.Empty;
        public string packId = "alpha25_pack";
        public string themeId = "base_default";
        public string difficultyTier = "hook";
        public string infoCardId = string.Empty;
        public string imageResourcePath = string.Empty;
        public LocalizedValueSet questionText = new LocalizedValueSet();
        public LocalizedValueSet answerText = new LocalizedValueSet();
        public LocalizedValueSet infoTitle = new LocalizedValueSet();
        public LocalizedValueSet infoBody = new LocalizedValueSet();

        public LevelQuestionEditorEntry CloneWithNewIds(string newQuestionId, string newInfoCardId)
        {
            return new LevelQuestionEditorEntry
            {
                questionId = newQuestionId,
                packId = packId,
                themeId = themeId,
                difficultyTier = difficultyTier,
                infoCardId = newInfoCardId,
                imageResourcePath = imageResourcePath,
                questionText = CloneValueSet(questionText),
                answerText = CloneValueSet(answerText),
                infoTitle = CloneValueSet(infoTitle),
                infoBody = CloneValueSet(infoBody)
            };
        }

        private static LocalizedValueSet CloneValueSet(LocalizedValueSet source)
        {
            return new LocalizedValueSet
            {
                tr = source != null ? source.tr : string.Empty,
                en = source != null ? source.en : string.Empty,
                es = source != null ? source.es : string.Empty,
                de = source != null ? source.de : string.Empty
            };
        }
    }

    [Serializable]
    public class LevelContentEditorEntry
    {
        public int levelId;
        public string campaignId = "alpha_main";
        public string themeId = "base_default";
        public string difficultyProfileId = "hook";
        public string difficultyTierId = "intro_perfect";
        public string rhythmProfileId = "hook";
        public string shape = "circle";
        public string shapeLayoutId = "circle_classic";
        public float rotationSpeed = 40f;
        public bool clockwise = true;
        public bool randomSlots;
        public bool skipAllowed;
        public List<ObstacleDefinition> obstacles = new List<ObstacleDefinition>();
        public List<LevelQuestionEditorEntry> questions = new List<LevelQuestionEditorEntry>();

        public LevelContentEditorEntry CloneWithNewIds(int newLevelId, Func<string> nextQuestionId, Func<string> nextInfoCardId)
        {
            LevelContentEditorEntry clone = new LevelContentEditorEntry
            {
                levelId = newLevelId,
                campaignId = campaignId,
                themeId = themeId,
                difficultyProfileId = difficultyProfileId,
                difficultyTierId = difficultyTierId,
                rhythmProfileId = rhythmProfileId,
                shape = shape,
                shapeLayoutId = shapeLayoutId,
                rotationSpeed = rotationSpeed,
                clockwise = clockwise,
                randomSlots = randomSlots,
                skipAllowed = skipAllowed,
                obstacles = obstacles != null
                    ? obstacles.Select(obstacle => new ObstacleDefinition
                    {
                        obstacleType = obstacle.obstacleType,
                        angleOffset = obstacle.angleOffset,
                        severity = obstacle.severity
                    }).ToList()
                    : new List<ObstacleDefinition>()
            };

            if (questions != null)
            {
                for (int i = 0; i < questions.Count; i++)
                {
                    clone.questions.Add(questions[i].CloneWithNewIds(nextQuestionId(), nextInfoCardId()));
                }
            }

            return clone;
        }
    }

    [Serializable]
    public class ContentEditorValidationIssue
    {
        public string severity = "Uyari";
        public string area = string.Empty;
        public string message = string.Empty;
    }

    [Serializable]
    public class WordSpinContentEditorDocument
    {
        public List<LevelContentEditorEntry> levels = new List<LevelContentEditorEntry>();
        public List<ShapeLayoutDefinition> shapeLayouts = new List<ShapeLayoutDefinition>();
        public string[] campaignIds = Array.Empty<string>();
        public string[] themeIds = Array.Empty<string>();
        public string[] difficultyProfileIds = Array.Empty<string>();
        public string[] difficultyTierIds = Array.Empty<string>();
        public string[] rhythmProfileIds = Array.Empty<string>();
        public string[] questionDifficultyBandIds = Array.Empty<string>();
    }

    public static class WordSpinAlphaContentEditorRepository
    {
        private const string RootContentPath = "Assets/WordSpinAlpha/Resources/Content";
        private const string LocalesRootPath = RootContentPath + "/Locales";
        private const string RootLevelsPath = RootContentPath + "/levels.json";
        private const string RootQuestionsPath = RootContentPath + "/questions.json";
        private const string RootInfoCardsPath = RootContentPath + "/info_cards.json";
        private const string ShapeLayoutsPath = RootContentPath + "/shape_layouts.json";
        private const string CampaignsPath = RootContentPath + "/campaigns.json";
        private const string ThemesPath = RootContentPath + "/themes.json";
        private const string DifficultyProfilesPath = RootContentPath + "/difficulty_profiles.json";
        private const string DifficultyTiersPath = RootContentPath + "/difficulty_tiers.json";
        private const string RhythmProfilesPath = RootContentPath + "/rhythm_profiles.json";

        public static WordSpinContentEditorDocument LoadDocument()
        {
            LevelCatalog sharedLevels = LoadSharedLevels();
            Dictionary<string, QuestionDefinition>[] questionsByLanguage = BuildQuestionMaps();
            Dictionary<string, InfoCardDefinition>[] infoCardsByLanguage = BuildInfoCardMaps();

            WordSpinContentEditorDocument document = new WordSpinContentEditorDocument
            {
                levels = new List<LevelContentEditorEntry>(),
                shapeLayouts = (LoadJson<ShapeLayoutCatalog>(ShapeLayoutsPath)?.layouts ?? Array.Empty<ShapeLayoutDefinition>())
                    .Select(CloneShapeLayout)
                    .Select(SanitizeShapeLayout)
                    .OrderBy(layout => layout.displayName ?? layout.shapeLayoutId)
                    .ToList(),
                campaignIds = (LoadJson<CampaignCatalog>(CampaignsPath)?.campaigns ?? Array.Empty<CampaignPackDefinition>())
                    .Select(campaign => campaign.campaignId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .OrderBy(id => id)
                    .ToArray(),
                themeIds = (LoadJson<ThemeCatalog>(ThemesPath)?.themes ?? Array.Empty<ThemePackDefinition>())
                    .Select(theme => theme.themeId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .OrderBy(id => id)
                    .ToArray(),
                difficultyProfileIds = (LoadJson<DifficultyCatalog>(DifficultyProfilesPath)?.profiles ?? Array.Empty<DifficultyProfileDefinition>())
                    .Select(profile => profile.difficultyProfileId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .OrderBy(id => id)
                    .ToArray(),
                difficultyTierIds = (LoadJson<DifficultyTierCatalog>(DifficultyTiersPath)?.tiers ?? Array.Empty<DifficultyTierDefinition>())
                    .Select(tier => tier.difficultyTierId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .OrderBy(id => id)
                    .ToArray(),
                rhythmProfileIds = (LoadJson<RhythmCatalog>(RhythmProfilesPath)?.profiles ?? Array.Empty<RhythmProfileDefinition>())
                    .Select(profile => profile.rhythmProfileId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .OrderBy(id => id)
                    .ToArray()
            };

            SortedSet<string> questionBands = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            LevelDefinition[] levels = sharedLevels != null ? sharedLevels.levels ?? Array.Empty<LevelDefinition>() : Array.Empty<LevelDefinition>();
            for (int i = 0; i < levels.Length; i++)
            {
                LevelDefinition level = levels[i];
                if (level == null)
                {
                    continue;
                }

                LevelContentEditorEntry entry = new LevelContentEditorEntry
                {
                    levelId = level.levelId,
                    campaignId = level.campaignId ?? "alpha_main",
                    themeId = level.themeId ?? "base_default",
                    difficultyProfileId = level.difficultyProfileId ?? string.Empty,
                    difficultyTierId = level.difficultyTierId ?? string.Empty,
                    rhythmProfileId = level.rhythmProfileId ?? string.Empty,
                    shape = level.shape ?? string.Empty,
                    shapeLayoutId = level.shapeLayoutId ?? string.Empty,
                    rotationSpeed = level.rotationSpeed,
                    clockwise = level.clockwise,
                    randomSlots = level.randomSlots,
                    skipAllowed = level.skipAllowed,
                    obstacles = level.obstacles != null
                        ? level.obstacles.Select(obstacle => new ObstacleDefinition
                        {
                            obstacleType = obstacle.obstacleType,
                            angleOffset = obstacle.angleOffset,
                            severity = obstacle.severity
                        }).ToList()
                        : new List<ObstacleDefinition>(),
                    questions = new List<LevelQuestionEditorEntry>()
                };

                string[] questionIds = level.questionIds ?? Array.Empty<string>();
                for (int questionIndex = 0; questionIndex < questionIds.Length; questionIndex++)
                {
                    string questionId = questionIds[questionIndex];
                    QuestionDefinition baseQuestion = FindFirstQuestion(questionId, questionsByLanguage);
                    string infoCardId = baseQuestion != null ? baseQuestion.infoCardId : string.Empty;
                    InfoCardDefinition baseCard = FindFirstInfoCard(infoCardId, infoCardsByLanguage);

                    LevelQuestionEditorEntry questionEntry = new LevelQuestionEditorEntry
                    {
                        questionId = questionId ?? string.Empty,
                        packId = baseQuestion != null ? baseQuestion.packId ?? "alpha25_pack" : "alpha25_pack",
                        themeId = baseQuestion != null ? baseQuestion.themeId ?? entry.themeId : entry.themeId,
                        difficultyTier = baseQuestion != null ? baseQuestion.difficultyTier ?? entry.difficultyProfileId : entry.difficultyProfileId,
                        infoCardId = infoCardId ?? string.Empty,
                        imageResourcePath = baseCard != null ? baseCard.imageResourcePath ?? string.Empty : string.Empty
                    };

                    for (int langIndex = 0; langIndex < Lang.All.Length; langIndex++)
                    {
                        string languageCode = Lang.All[langIndex];
                        if (questionsByLanguage[langIndex].TryGetValue(questionId, out QuestionDefinition localizedQuestion))
                        {
                            questionEntry.questionText.Set(languageCode, localizedQuestion.GetQuestion(languageCode));
                            questionEntry.answerText.Set(languageCode, localizedQuestion.GetAnswer(languageCode));
                            if (!string.IsNullOrWhiteSpace(localizedQuestion.packId))
                            {
                                questionEntry.packId = localizedQuestion.packId;
                            }

                            if (!string.IsNullOrWhiteSpace(localizedQuestion.themeId))
                            {
                                questionEntry.themeId = localizedQuestion.themeId;
                            }

                            if (!string.IsNullOrWhiteSpace(localizedQuestion.difficultyTier))
                            {
                                questionEntry.difficultyTier = localizedQuestion.difficultyTier;
                            }

                            if (!string.IsNullOrWhiteSpace(localizedQuestion.infoCardId))
                            {
                                questionEntry.infoCardId = localizedQuestion.infoCardId;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(questionEntry.infoCardId) &&
                            infoCardsByLanguage[langIndex].TryGetValue(questionEntry.infoCardId, out InfoCardDefinition localizedCard))
                        {
                            questionEntry.infoTitle.Set(languageCode, localizedCard.title != null ? localizedCard.title.Get(languageCode) : string.Empty);
                            questionEntry.infoBody.Set(languageCode, localizedCard.body != null ? localizedCard.body.Get(languageCode) : string.Empty);
                            if (!string.IsNullOrWhiteSpace(localizedCard.imageResourcePath))
                            {
                                questionEntry.imageResourcePath = localizedCard.imageResourcePath;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(questionEntry.difficultyTier))
                    {
                        questionBands.Add(questionEntry.difficultyTier);
                    }

                    entry.questions.Add(questionEntry);
                }

                document.levels.Add(entry);
            }

            document.levels = document.levels.OrderBy(level => level.levelId).ToList();
            document.questionDifficultyBandIds = questionBands.Count > 0 ? questionBands.ToArray() : new[] { "hook", "rhythm", "variation", "pressure", "showcase" };
            return document;
        }

        public static List<ContentEditorValidationIssue> Validate(WordSpinContentEditorDocument document)
        {
            List<ContentEditorValidationIssue> issues = new List<ContentEditorValidationIssue>();
            if (document == null)
            {
                issues.Add(new ContentEditorValidationIssue { severity = "Hata", area = "Dokuman", message = "Editor dokumani bos." });
                return issues;
            }

            HashSet<int> levelIds = new HashSet<int>();
            HashSet<string> questionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> infoCardIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> seenShapeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> shapeIds = new HashSet<string>((document.shapeLayouts ?? new List<ShapeLayoutDefinition>())
                .Where(layout => layout != null && !string.IsNullOrWhiteSpace(layout.shapeLayoutId))
                .Select(layout => layout.shapeLayoutId), StringComparer.OrdinalIgnoreCase);

            foreach (LevelContentEditorEntry level in document.levels ?? new List<LevelContentEditorEntry>())
            {
                if (level == null)
                {
                    continue;
                }

                string levelArea = $"Seviye {level.levelId}";
                if (!levelIds.Add(level.levelId))
                {
                    issues.Add(new ContentEditorValidationIssue { severity = "Hata", area = levelArea, message = "Ayni levelId birden fazla kez kullaniliyor." });
                }

                if (level.questions == null || level.questions.Count == 0)
                {
                    issues.Add(new ContentEditorValidationIssue { severity = "Hata", area = levelArea, message = "Seviyede en az bir soru blogu olmali." });
                }

                if (string.IsNullOrWhiteSpace(level.shapeLayoutId) || !shapeIds.Contains(level.shapeLayoutId))
                {
                    issues.Add(new ContentEditorValidationIssue { severity = "Hata", area = levelArea, message = "Gecerli bir shape layout secilmemis." });
                }

                int availableSlotCount = 0;
                ShapeLayoutDefinition levelShape = (document.shapeLayouts ?? new List<ShapeLayoutDefinition>())
                    .FirstOrDefault(layout => layout != null && string.Equals(layout.shapeLayoutId, level.shapeLayoutId, StringComparison.OrdinalIgnoreCase));
                if (levelShape != null)
                {
                    availableSlotCount = Mathf.Max(0, levelShape.slotCount);
                }

                for (int i = 0; i < (level.questions != null ? level.questions.Count : 0); i++)
                {
                    LevelQuestionEditorEntry question = level.questions[i];
                    string questionArea = $"{levelArea} / Soru {i + 1}";

                    if (string.IsNullOrWhiteSpace(question.questionId))
                    {
                        issues.Add(new ContentEditorValidationIssue { severity = "Hata", area = questionArea, message = "QuestionId bos." });
                    }
                    else if (!questionIds.Add(question.questionId))
                    {
                        issues.Add(new ContentEditorValidationIssue { severity = "Hata", area = questionArea, message = $"Ayni questionId tekrar ediyor: {question.questionId}" });
                    }

                    if (string.IsNullOrWhiteSpace(question.infoCardId))
                    {
                        issues.Add(new ContentEditorValidationIssue { severity = "Hata", area = questionArea, message = "InfoCardId bos." });
                    }
                    else if (!infoCardIds.Add(question.infoCardId))
                    {
                        issues.Add(new ContentEditorValidationIssue { severity = "Hata", area = questionArea, message = $"Ayni infoCardId tekrar ediyor: {question.infoCardId}" });
                    }

                    foreach (string languageCode in Lang.All)
                    {
                        if (string.IsNullOrWhiteSpace(question.questionText.Get(languageCode)))
                        {
                            issues.Add(new ContentEditorValidationIssue { severity = "Uyari", area = questionArea, message = $"{languageCode.ToUpperInvariant()} soru metni bos." });
                        }

                        if (string.IsNullOrWhiteSpace(question.answerText.Get(languageCode)))
                        {
                            issues.Add(new ContentEditorValidationIssue { severity = "Uyari", area = questionArea, message = $"{languageCode.ToUpperInvariant()} cevap bos." });
                        }
                        else if (availableSlotCount > 0 && CountAnswerLetters(question.answerText.Get(languageCode)) > availableSlotCount)
                        {
                            issues.Add(new ContentEditorValidationIssue
                            {
                                severity = "Hata",
                                area = questionArea,
                                message = $"{languageCode.ToUpperInvariant()} cevap uzunlugu shape slot sayisini asiyor ({availableSlotCount})."
                            });
                        }

                        if (string.IsNullOrWhiteSpace(question.infoTitle.Get(languageCode)))
                        {
                            issues.Add(new ContentEditorValidationIssue { severity = "Uyari", area = questionArea, message = $"{languageCode.ToUpperInvariant()} bilgi karti basligi bos." });
                        }

                        if (string.IsNullOrWhiteSpace(question.infoBody.Get(languageCode)))
                        {
                            issues.Add(new ContentEditorValidationIssue { severity = "Uyari", area = questionArea, message = $"{languageCode.ToUpperInvariant()} bilgi karti govdesi bos." });
                        }
                    }
                }
            }

            foreach (ShapeLayoutDefinition shape in document.shapeLayouts ?? new List<ShapeLayoutDefinition>())
            {
                if (shape == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(shape.shapeLayoutId))
                {
                    issues.Add(new ContentEditorValidationIssue { severity = "Hata", area = "Sekil Kutuphanesi", message = "Bos shapeLayoutId bulundu." });
                }
                else if (!seenShapeIds.Add(shape.shapeLayoutId))
                {
                    issues.Add(new ContentEditorValidationIssue { severity = "Hata", area = $"Sekil {shape.shapeLayoutId}", message = "Ayni shapeLayoutId birden fazla kez kullaniliyor." });
                }

                if (shape.slotCount <= 0)
                {
                    issues.Add(new ContentEditorValidationIssue { severity = "Hata", area = $"Sekil {shape.shapeLayoutId}", message = "Slot count 1 veya daha buyuk olmali." });
                }

                if (shape.customPoints != null && shape.customPoints.Length > 0 && shape.customPoints.Length != shape.slotCount)
                {
                    issues.Add(new ContentEditorValidationIssue
                    {
                        severity = "Uyari",
                        area = $"Sekil {shape.shapeLayoutId}",
                        message = $"customPoints sayisi ({shape.customPoints.Length}) ile slotCount ({shape.slotCount}) farkli."
                    });
                }

                if (!string.IsNullOrWhiteSpace(shape.editorReferenceImagePath) && AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(shape.editorReferenceImagePath) == null)
                {
                    issues.Add(new ContentEditorValidationIssue
                    {
                        severity = "Uyari",
                        area = $"Sekil {shape.shapeLayoutId}",
                        message = "Referans gorsel yolu bulundu ama asset dosyasi artik yok."
                    });
                }

                if (shape.customPoints != null && shape.customPoints.Length > 0 && ShapeLayoutGeometry.LooksCorrupted(shape.customPoints, shape.radiusX, shape.radiusY))
                {
                    issues.Add(new ContentEditorValidationIssue
                    {
                        severity = "Uyari",
                        area = $"Sekil {shape.shapeLayoutId}",
                        message = "customPoints olcegi bozuk gorunuyor. Kayit sirasinda normalize edilecek."
                    });
                }

                ShapePointDefinition[] previewPoints = ShapeLayoutGeometry.ResolvePoints(shape);
                float minPairDistance = ShapeLayoutGeometry.ComputeMinimumPairDistance(previewPoints);
                float minVisualSeatWidth = float.MaxValue;
                for (int i = 0; i < previewPoints.Length; i++)
                {
                    ShapePlaqueVisualLayoutInfo visualLayout = ShapeLayoutGeometry.ResolvePlaqueVisualLayout(shape, previewPoints, i, previewPoints.Length);
                    minVisualSeatWidth = Mathf.Min(minVisualSeatWidth, visualLayout.seatSize.x);
                }

                if (minVisualSeatWidth == float.MaxValue)
                {
                    minVisualSeatWidth = Mathf.Max(0.14f, shape.plaqueWidth) * 1.55f;
                }

                if (previewPoints.Length > 1 && minPairDistance > 0f && minPairDistance < minVisualSeatWidth * 0.76f)
                {
                    issues.Add(new ContentEditorValidationIssue
                    {
                        severity = "Uyari",
                        area = $"Sekil {shape.shapeLayoutId}",
                        message = "Bu shape runtime'da plaque overlap riski tasiyor. Slot sayisi veya shape formu kontrol edilmeli."
                    });
                }
            }

            return issues
                .OrderByDescending(issue => string.Equals(issue.severity, "Hata", StringComparison.OrdinalIgnoreCase))
                .ThenBy(issue => issue.area)
                .ToList();
        }

        public static void SaveDocument(WordSpinContentEditorDocument document)
        {
            if (document == null)
            {
                return;
            }

            LevelCatalog levelCatalog = new LevelCatalog
            {
                levels = (document.levels ?? new List<LevelContentEditorEntry>())
                    .OrderBy(level => level.levelId)
                    .Select(BuildLevelDefinition)
                    .ToArray()
            };

            WriteJson(RootLevelsPath, levelCatalog);
            foreach (string languageCode in Lang.All)
            {
                WriteJson(GetLocalizedLevelsPath(languageCode), levelCatalog);
            }

            WriteJson(RootQuestionsPath, BuildQuestionCatalog(document, Lang.TR));
            WriteJson(RootInfoCardsPath, BuildInfoCardCatalog(document, Lang.TR));
            foreach (string languageCode in Lang.All)
            {
                WriteJson(GetLocalizedQuestionsPath(languageCode), BuildQuestionCatalog(document, languageCode));
                WriteJson(GetLocalizedInfoCardsPath(languageCode), BuildInfoCardCatalog(document, languageCode));
            }

            List<ShapeLayoutDefinition> sanitizedLayouts = (document.shapeLayouts ?? new List<ShapeLayoutDefinition>())
                .Where(layout => layout != null)
                .Select(SanitizeShapeLayout)
                .OrderBy(layout => layout.displayName ?? layout.shapeLayoutId)
                .ToList();
            document.shapeLayouts = sanitizedLayouts.Select(CloneShapeLayout).ToList();

            ShapeLayoutCatalog shapes = new ShapeLayoutCatalog
            {
                layouts = sanitizedLayouts.ToArray()
            };
            WriteJson(ShapeLayoutsPath, shapes);

            AssetDatabase.Refresh();
        }

        public static string GenerateNextQuestionId(WordSpinContentEditorDocument document)
        {
            return GenerateNextId("q", (document.levels ?? new List<LevelContentEditorEntry>())
                .SelectMany(level => level.questions ?? new List<LevelQuestionEditorEntry>())
                .Select(question => question.questionId));
        }

        public static string GenerateNextInfoCardId(WordSpinContentEditorDocument document)
        {
            return GenerateNextId("c", (document.levels ?? new List<LevelContentEditorEntry>())
                .SelectMany(level => level.questions ?? new List<LevelQuestionEditorEntry>())
                .Select(question => question.infoCardId));
        }

        public static int GenerateNextLevelId(WordSpinContentEditorDocument document)
        {
            int max = (document.levels ?? new List<LevelContentEditorEntry>()).Count == 0
                ? 0
                : document.levels.Max(level => level.levelId);
            return max + 1;
        }

        public static ShapeLayoutDefinition CreateNewShapeLayout(string shapeLayoutId, string displayName, string shapeFamily, int slotCount)
        {
            bool adaptiveVisuals =
                string.Equals(shapeFamily, "custom", StringComparison.OrdinalIgnoreCase)
                || string.Equals(shapeFamily, "heart", StringComparison.OrdinalIgnoreCase)
                || string.Equals(shapeFamily, "star", StringComparison.OrdinalIgnoreCase);

            return new ShapeLayoutDefinition
            {
                shapeLayoutId = shapeLayoutId,
                displayName = displayName,
                shapeFamily = shapeFamily,
                visualPrefabResourcePath = string.Empty,
                editorReferenceImagePath = string.Empty,
                slotCount = Mathf.Max(1, slotCount),
                radiusX = 1.10f,
                radiusY = 1.10f,
                rotationOffsetDegrees = 0f,
                plaqueWidth = 0.30f,
                plaqueHeight = 0.18f,
                perfectWidthScale = 0.45f,
                perfectHeightScale = 0.45f,
                nearMissPadding = 0.08f,
                useTangentialRotation = true,
                gameplayAutoFit = true,
                adaptivePlaqueVisuals = adaptiveVisuals,
                plaqueVisualPadding = 0.10f,
                plaqueVisualMinWidthScale = 0.58f,
                plaqueVisualMaxWidthScale = 1.00f,
                plaqueVisualMinHeightScale = 0.82f,
                plaqueVisualMaxHeightScale = 1.00f,
                plaqueVisualOutwardOffset = 0.08f,
                plaqueVisualContourFollow = 0.45f,
                plaqueVisualAngleOffsets = CreateZeroFloatArray(Mathf.Max(1, slotCount)),
                customPoints = ShapeLayoutGeometry.GenerateAutoShapePoints(shapeFamily, Mathf.Max(1, slotCount), 1.10f, 1.10f, 0f)
            };
        }

        public static ShapePointDefinition[] GenerateAutoShapePoints(string shapeFamily, int slotCount, float radiusX, float radiusY, float rotationOffsetDegrees)
        {
            return ShapeLayoutGeometry.GenerateAutoShapePoints(shapeFamily, slotCount, radiusX, radiusY, rotationOffsetDegrees);
        }

        public static ShapePointDefinition[] GeneratePreviewPoints(ShapeLayoutDefinition shape)
        {
            if (shape != null &&
                (shape.customPoints == null || shape.customPoints.Length == 0) &&
                !string.IsNullOrWhiteSpace(shape.editorReferenceImagePath) &&
                TryGeneratePointsFromReferenceImage(shape.editorReferenceImagePath, Mathf.Max(1, shape.slotCount), shape.radiusX, shape.radiusY, out ShapePointDefinition[] referencePoints))
            {
                return referencePoints;
            }

            return ShapeLayoutGeometry.ResolvePoints(shape);
        }

        public static ShapePointDefinition[] EnsureEditableCustomPoints(ShapeLayoutDefinition shape)
        {
            if (shape == null)
            {
                return Array.Empty<ShapePointDefinition>();
            }

            ShapePointDefinition[] preparedExisting = ShapeLayoutGeometry.PrepareCustomPoints(shape, Mathf.Max(1, shape.slotCount));
            if (preparedExisting != null && preparedExisting.Length == Mathf.Max(1, shape.slotCount))
            {
                shape.customPoints = preparedExisting;
                return shape.customPoints;
            }

            shape.customPoints = ShapeLayoutGeometry.ClonePoints(GeneratePreviewPoints(shape));
            return shape.customPoints ?? Array.Empty<ShapePointDefinition>();
        }

        public static float[] EnsureEditablePlaqueAngles(ShapeLayoutDefinition shape)
        {
            if (shape == null)
            {
                return Array.Empty<float>();
            }

            int count = Mathf.Max(1, shape.slotCount);
            shape.plaqueVisualAngleOffsets = ResampleFloatArrayLoop(shape.plaqueVisualAngleOffsets, count);
            return shape.plaqueVisualAngleOffsets;
        }

        public static void ResetShapeToDefault(ShapeLayoutDefinition shape)
        {
            if (shape == null)
            {
                return;
            }

            if (RegenerateShapeFromReferenceImage(shape))
            {
                return;
            }

            shape.customPoints = ShapeLayoutGeometry.GenerateFallbackPoints(shape, Mathf.Max(1, shape.slotCount));
        }

        public static bool RegenerateShapeFromReferenceImage(ShapeLayoutDefinition shape)
        {
            if (shape == null || string.IsNullOrWhiteSpace(shape.editorReferenceImagePath))
            {
                return false;
            }

            if (!TryGeneratePointsFromReferenceImage(shape.editorReferenceImagePath, Mathf.Max(1, shape.slotCount), shape.radiusX, shape.radiusY, out ShapePointDefinition[] points))
            {
                return false;
            }

            shape.shapeFamily = "custom";
            shape.customPoints = points;
            return true;
        }

        public static void ApplySlotCountChange(ShapeLayoutDefinition shape, int previousSlotCount)
        {
            if (shape == null)
            {
                return;
            }

            int nextCount = Mathf.Max(1, shape.slotCount);
            int oldCount = Mathf.Max(1, previousSlotCount);
            if (nextCount == oldCount)
            {
                return;
            }

            if (RegenerateShapeFromReferenceImage(shape))
            {
                return;
            }

            ShapeLayoutDefinition previousShape = CloneShapeLayout(shape);
            previousShape.slotCount = oldCount;
            ShapePointDefinition[] sourcePoints = GeneratePreviewPoints(previousShape);
            if (sourcePoints == null || sourcePoints.Length == 0)
            {
                sourcePoints = ShapeLayoutGeometry.GenerateFallbackPoints(previousShape, oldCount);
            }

            shape.customPoints = ShapeLayoutGeometry.ResamplePoints(sourcePoints, nextCount);
            shape.plaqueVisualAngleOffsets = ResampleFloatArrayLoop(shape.plaqueVisualAngleOffsets, nextCount);
        }

        public static Texture2D LoadReferenceTexture(ShapeLayoutDefinition shape)
        {
            if (shape == null || string.IsNullOrWhiteSpace(shape.editorReferenceImagePath))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(shape.editorReferenceImagePath);
        }

        public static void SetReferenceTexture(ShapeLayoutDefinition shape, Texture2D texture)
        {
            if (shape == null)
            {
                return;
            }

            shape.editorReferenceImagePath = texture != null ? AssetDatabase.GetAssetPath(texture) : string.Empty;
        }

        private static Dictionary<string, QuestionDefinition>[] BuildQuestionMaps()
        {
            Dictionary<string, QuestionDefinition>[] maps = new Dictionary<string, QuestionDefinition>[Lang.All.Length];
            for (int i = 0; i < Lang.All.Length; i++)
            {
                QuestionCatalog catalog = LoadLocalizedQuestionCatalog(Lang.All[i]);
                maps[i] = (catalog?.questions ?? Array.Empty<QuestionDefinition>())
                    .Where(question => question != null && !string.IsNullOrWhiteSpace(question.questionId))
                    .GroupBy(question => question.questionId)
                    .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);
            }

            return maps;
        }

        private static Dictionary<string, InfoCardDefinition>[] BuildInfoCardMaps()
        {
            Dictionary<string, InfoCardDefinition>[] maps = new Dictionary<string, InfoCardDefinition>[Lang.All.Length];
            for (int i = 0; i < Lang.All.Length; i++)
            {
                InfoCardCatalog catalog = LoadLocalizedInfoCardCatalog(Lang.All[i]);
                maps[i] = (catalog?.cards ?? Array.Empty<InfoCardDefinition>())
                    .Where(card => card != null && !string.IsNullOrWhiteSpace(card.infoCardId))
                    .GroupBy(card => card.infoCardId)
                    .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);
            }

            return maps;
        }

        private static LevelCatalog LoadSharedLevels()
        {
            LevelCatalog root = LoadJson<LevelCatalog>(RootLevelsPath);
            if (root != null && root.levels != null && root.levels.Length > 0)
            {
                return root;
            }

            for (int i = 0; i < Lang.All.Length; i++)
            {
                LevelCatalog localized = LoadJson<LevelCatalog>(GetLocalizedLevelsPath(Lang.All[i]));
                if (localized != null && localized.levels != null && localized.levels.Length > 0)
                {
                    return localized;
                }
            }

            return new LevelCatalog { levels = Array.Empty<LevelDefinition>() };
        }

        private static QuestionCatalog LoadLocalizedQuestionCatalog(string languageCode)
        {
            QuestionCatalog localized = LoadJson<QuestionCatalog>(GetLocalizedQuestionsPath(languageCode));
            if (localized != null && localized.questions != null && localized.questions.Length > 0)
            {
                return localized;
            }

            if (string.Equals(languageCode, Lang.TR, StringComparison.OrdinalIgnoreCase))
            {
                return LoadJson<QuestionCatalog>(RootQuestionsPath) ?? new QuestionCatalog { questions = Array.Empty<QuestionDefinition>() };
            }

            return localized ?? new QuestionCatalog { questions = Array.Empty<QuestionDefinition>() };
        }

        private static InfoCardCatalog LoadLocalizedInfoCardCatalog(string languageCode)
        {
            InfoCardCatalog localized = LoadJson<InfoCardCatalog>(GetLocalizedInfoCardsPath(languageCode));
            if (localized != null && localized.cards != null && localized.cards.Length > 0)
            {
                return localized;
            }

            if (string.Equals(languageCode, Lang.TR, StringComparison.OrdinalIgnoreCase))
            {
                return LoadJson<InfoCardCatalog>(RootInfoCardsPath) ?? new InfoCardCatalog { cards = Array.Empty<InfoCardDefinition>() };
            }

            return localized ?? new InfoCardCatalog { cards = Array.Empty<InfoCardDefinition>() };
        }

        private static QuestionDefinition FindFirstQuestion(string questionId, Dictionary<string, QuestionDefinition>[] maps)
        {
            if (string.IsNullOrWhiteSpace(questionId) || maps == null)
            {
                return null;
            }

            for (int i = 0; i < maps.Length; i++)
            {
                if (maps[i].TryGetValue(questionId, out QuestionDefinition question))
                {
                    return question;
                }
            }

            return null;
        }

        private static InfoCardDefinition FindFirstInfoCard(string infoCardId, Dictionary<string, InfoCardDefinition>[] maps)
        {
            if (string.IsNullOrWhiteSpace(infoCardId) || maps == null)
            {
                return null;
            }

            for (int i = 0; i < maps.Length; i++)
            {
                if (maps[i].TryGetValue(infoCardId, out InfoCardDefinition card))
                {
                    return card;
                }
            }

            return null;
        }

        private static LevelDefinition BuildLevelDefinition(LevelContentEditorEntry source)
        {
            return new LevelDefinition
            {
                levelId = source.levelId,
                campaignId = source.campaignId,
                themeId = source.themeId,
                difficultyProfileId = source.difficultyProfileId,
                difficultyTierId = source.difficultyTierId,
                rhythmProfileId = source.rhythmProfileId,
                shape = source.shape,
                shapeLayoutId = source.shapeLayoutId,
                rotationSpeed = source.rotationSpeed,
                clockwise = source.clockwise,
                randomSlots = source.randomSlots,
                skipAllowed = source.skipAllowed,
                questionIds = (source.questions ?? new List<LevelQuestionEditorEntry>())
                    .Select(question => question.questionId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToArray(),
                obstacles = source.obstacles != null
                    ? source.obstacles.Select(obstacle => new ObstacleDefinition
                    {
                        obstacleType = obstacle.obstacleType,
                        angleOffset = obstacle.angleOffset,
                        severity = obstacle.severity
                    }).ToArray()
                    : Array.Empty<ObstacleDefinition>()
            };
        }

        private static QuestionCatalog BuildQuestionCatalog(WordSpinContentEditorDocument document, string languageCode)
        {
            List<QuestionDefinition> questions = new List<QuestionDefinition>();
            foreach (LevelContentEditorEntry level in document.levels ?? new List<LevelContentEditorEntry>())
            {
                foreach (LevelQuestionEditorEntry question in level.questions ?? new List<LevelQuestionEditorEntry>())
                {
                    questions.Add(new QuestionDefinition
                    {
                        questionId = question.questionId,
                        packId = question.packId,
                        themeId = question.themeId,
                        difficultyTier = question.difficultyTier,
                        text = BuildSingleLanguagePack(languageCode, question.questionText.Get(languageCode)),
                        answer = BuildSingleLanguagePack(languageCode, question.answerText.Get(languageCode)),
                        infoCardId = question.infoCardId
                    });
                }
            }

            return new QuestionCatalog { questions = questions.ToArray() };
        }

        private static InfoCardCatalog BuildInfoCardCatalog(WordSpinContentEditorDocument document, string languageCode)
        {
            List<InfoCardDefinition> cards = new List<InfoCardDefinition>();
            foreach (LevelContentEditorEntry level in document.levels ?? new List<LevelContentEditorEntry>())
            {
                foreach (LevelQuestionEditorEntry question in level.questions ?? new List<LevelQuestionEditorEntry>())
                {
                    cards.Add(new InfoCardDefinition
                    {
                        infoCardId = question.infoCardId,
                        themeId = question.themeId,
                        title = BuildSingleLanguagePack(languageCode, question.infoTitle.Get(languageCode)),
                        body = BuildSingleLanguagePack(languageCode, question.infoBody.Get(languageCode)),
                        imageResourcePath = question.imageResourcePath
                    });
                }
            }

            return new InfoCardCatalog { cards = cards.ToArray() };
        }

        private static LangPack BuildSingleLanguagePack(string languageCode, string value)
        {
            LangPack pack = new LangPack();
            switch ((languageCode ?? Lang.TR).ToLowerInvariant())
            {
                case Lang.EN:
                    pack.en = value ?? string.Empty;
                    break;
                case Lang.ES:
                    pack.es = value ?? string.Empty;
                    break;
                case Lang.DE:
                    pack.de = value ?? string.Empty;
                    break;
                default:
                    pack.tr = value ?? string.Empty;
                    break;
            }

            return pack;
        }

        private static ShapeLayoutDefinition CloneShapeLayout(ShapeLayoutDefinition source)
        {
            return new ShapeLayoutDefinition
            {
                shapeLayoutId = source.shapeLayoutId,
                displayName = source.displayName,
                shapeFamily = source.shapeFamily,
                visualPrefabResourcePath = source.visualPrefabResourcePath,
                editorReferenceImagePath = source.editorReferenceImagePath,
                slotCount = source.slotCount,
                radiusX = source.radiusX,
                radiusY = source.radiusY,
                rotationOffsetDegrees = source.rotationOffsetDegrees,
                plaqueWidth = source.plaqueWidth,
                plaqueHeight = source.plaqueHeight,
                perfectWidthScale = source.perfectWidthScale,
                perfectHeightScale = source.perfectHeightScale,
                nearMissPadding = source.nearMissPadding,
                useTangentialRotation = source.useTangentialRotation,
                gameplayAutoFit = source.gameplayAutoFit,
                adaptivePlaqueVisuals = source.adaptivePlaqueVisuals,
                plaqueVisualPadding = source.plaqueVisualPadding,
                plaqueVisualMinWidthScale = source.plaqueVisualMinWidthScale,
                plaqueVisualMaxWidthScale = source.plaqueVisualMaxWidthScale,
                plaqueVisualMinHeightScale = source.plaqueVisualMinHeightScale,
                plaqueVisualMaxHeightScale = source.plaqueVisualMaxHeightScale,
                plaqueVisualOutwardOffset = source.plaqueVisualOutwardOffset,
                plaqueVisualContourFollow = source.plaqueVisualContourFollow,
                plaqueVisualAngleOffsets = source.plaqueVisualAngleOffsets != null ? source.plaqueVisualAngleOffsets.ToArray() : null,
                customPoints = source.customPoints != null
                    ? source.customPoints.Select(point => new ShapePointDefinition { x = point.x, y = point.y }).ToArray()
                    : null,
                angleOverrides = source.angleOverrides != null ? source.angleOverrides.ToArray() : null,
                pointRadiusScales = source.pointRadiusScales != null ? source.pointRadiusScales.ToArray() : null
            };
        }

        private static ShapeLayoutDefinition SanitizeShapeLayout(ShapeLayoutDefinition source)
        {
            ShapeLayoutDefinition clone = CloneShapeLayout(source);
            clone.slotCount = Mathf.Max(1, clone.slotCount);
            clone.radiusX = Mathf.Max(0.2f, Mathf.Abs(clone.radiusX));
            clone.radiusY = Mathf.Max(0.2f, Mathf.Abs(clone.radiusY));
            clone.plaqueWidth = Mathf.Max(0.14f, clone.plaqueWidth);
            clone.plaqueHeight = Mathf.Max(0.10f, clone.plaqueHeight);
            clone.perfectWidthScale = Mathf.Max(0.1f, clone.perfectWidthScale);
            clone.perfectHeightScale = Mathf.Max(0.1f, clone.perfectHeightScale);
            clone.nearMissPadding = Mathf.Max(0f, clone.nearMissPadding);
            clone.plaqueVisualPadding = Mathf.Max(0f, clone.plaqueVisualPadding);
            clone.plaqueVisualMinWidthScale = Mathf.Clamp(clone.plaqueVisualMinWidthScale, 0.35f, 1.25f);
            clone.plaqueVisualMaxWidthScale = Mathf.Max(clone.plaqueVisualMinWidthScale, clone.plaqueVisualMaxWidthScale);
            clone.plaqueVisualMinHeightScale = Mathf.Clamp(clone.plaqueVisualMinHeightScale, 0.35f, 1.25f);
            clone.plaqueVisualMaxHeightScale = Mathf.Max(clone.plaqueVisualMinHeightScale, clone.plaqueVisualMaxHeightScale);
            clone.plaqueVisualOutwardOffset = Mathf.Max(0f, clone.plaqueVisualOutwardOffset);
            clone.plaqueVisualContourFollow = Mathf.Clamp01(clone.plaqueVisualContourFollow);
            clone.plaqueVisualAngleOffsets = ResampleFloatArrayLoop(clone.plaqueVisualAngleOffsets, clone.slotCount);

            ShapePointDefinition[] preparedCustomPoints = ShapeLayoutGeometry.PrepareCustomPoints(clone, clone.slotCount);
            if (!string.IsNullOrWhiteSpace(clone.editorReferenceImagePath) &&
                TryGeneratePointsFromReferenceImage(clone.editorReferenceImagePath, clone.slotCount, clone.radiusX, clone.radiusY, out ShapePointDefinition[] generatedReferencePoints))
            {
                if (preparedCustomPoints == null || preparedCustomPoints.Length == 0)
                {
                    clone.shapeFamily = "custom";
                    clone.customPoints = generatedReferencePoints;
                    return clone;
                }

                if (ShouldReplaceWithReferencePoints(preparedCustomPoints, generatedReferencePoints))
                {
                    clone.shapeFamily = "custom";
                    clone.customPoints = generatedReferencePoints;
                    return clone;
                }
            }

            if (preparedCustomPoints != null && preparedCustomPoints.Length > 0)
            {
                clone.customPoints = preparedCustomPoints;
                return clone;
            }

            if (clone.customPoints != null && clone.customPoints.Length > 0)
            {
                clone.customPoints = ShapeLayoutGeometry.GenerateFallbackPoints(clone, clone.slotCount);
            }

            return clone;
        }

        private static string GenerateNextId(string prefix, IEnumerable<string> existingIds)
        {
            int max = 0;
            foreach (string existingId in existingIds ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(existingId) || !existingId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string numeric = existingId.Substring(prefix.Length);
                if (int.TryParse(numeric, out int parsed))
                {
                    max = Mathf.Max(max, parsed);
                }
            }

            return $"{prefix}{max + 1:000}";
        }

        private static int CountAnswerLetters(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                if (!char.IsWhiteSpace(raw[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private static T LoadJson<T>(string path) where T : class, new()
        {
            if (!File.Exists(path))
            {
                return new T();
            }

            string raw = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new T();
            }

            T parsed = JsonUtility.FromJson<T>(raw);
            return parsed ?? new T();
        }

        private static void WriteJson<T>(string path, T payload)
        {
            EnsureFolder(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(payload, true));
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static string GetLocalizedLevelsPath(string languageCode) => $"{LocalesRootPath}/{languageCode}/levels.json";
        private static string GetLocalizedQuestionsPath(string languageCode) => $"{LocalesRootPath}/{languageCode}/questions.json";
        private static string GetLocalizedInfoCardsPath(string languageCode) => $"{LocalesRootPath}/{languageCode}/info_cards.json";

        private static bool TryGeneratePointsFromReferenceImage(string assetPath, int slotCount, float radiusX, float radiusY, out ShapePointDefinition[] points)
        {
            points = Array.Empty<ShapePointDefinition>();
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            string fullPath = Path.GetFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                return false;
            }

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(fullPath);
            }
            catch
            {
                return false;
            }

            Texture2D tempTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tempTexture.LoadImage(bytes))
            {
                UnityEngine.Object.DestroyImmediate(tempTexture);
                return false;
            }

            try
            {
                points = GeneratePointsFromTexture(tempTexture, slotCount, radiusX, radiusY);
                return points != null && points.Length == Mathf.Max(1, slotCount);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tempTexture);
            }
        }

        private static ShapePointDefinition[] GeneratePointsFromTexture(Texture2D texture, int slotCount, float radiusX, float radiusY)
        {
            if (texture == null)
            {
                return Array.Empty<ShapePointDefinition>();
            }

            int width = Mathf.Max(1, texture.width);
            int height = Mathf.Max(1, texture.height);
            Color32[] pixels = texture.GetPixels32();
            bool[] mask = BuildShapeMask(pixels, width, height);
            List<Vector2> opaquePixels = new List<Vector2>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask[(y * width) + x])
                    {
                        opaquePixels.Add(new Vector2(x, y));
                    }
                }
            }

            if (opaquePixels.Count < 8)
            {
                return ShapeLayoutGeometry.GenerateAutoShapePoints("circle", slotCount, radiusX, radiusY, 0f);
            }

            Vector2 center = Vector2.zero;
            for (int i = 0; i < opaquePixels.Count; i++)
            {
                center += opaquePixels[i];
            }

            center /= opaquePixels.Count;
            float maxRadius = 1f;
            for (int i = 0; i < opaquePixels.Count; i++)
            {
                maxRadius = Mathf.Max(maxRadius, Vector2.Distance(center, opaquePixels[i]));
            }

            int sampleCount = Mathf.Max(Mathf.Max(1, slotCount) * 8, 64);
            ShapePointDefinition[] raw = new ShapePointDefinition[sampleCount];
            int raySteps = Mathf.Max(width, height);
            for (int i = 0; i < raw.Length; i++)
            {
                float angle = (i / (float)raw.Length) * Mathf.PI * 2f;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 lastInside = center;
                bool found = false;
                for (int step = 1; step <= raySteps; step++)
                {
                    float distance = (maxRadius * step) / raySteps;
                    Vector2 samplePoint = center + (dir * distance);
                    if (!IsOpaque(mask, width, height, samplePoint))
                    {
                        break;
                    }

                    found = true;
                    lastInside = samplePoint;
                }

                if (!found)
                {
                    lastInside = center + (dir * (maxRadius * 0.5f));
                }

                raw[i] = new ShapePointDefinition
                {
                    x = lastInside.x - center.x,
                    y = lastInside.y - center.y
                };
            }

            ShapePointDefinition[] normalized = FitPointsUniform(raw, radiusX, radiusY);
            return ShapeLayoutGeometry.ResamplePoints(normalized, Mathf.Max(1, slotCount));
        }

        private static bool IsOpaque(bool[] mask, int width, int height, Vector2 point)
        {
            int x = Mathf.RoundToInt(point.x);
            int y = Mathf.RoundToInt(point.y);
            if (x < 0 || y < 0 || x >= width || y >= height)
            {
                return false;
            }

            return mask[(y * width) + x];
        }

        private static bool ShouldReplaceWithReferencePoints(ShapePointDefinition[] currentPoints, ShapePointDefinition[] referencePoints)
        {
            if (currentPoints == null || referencePoints == null || currentPoints.Length == 0 || referencePoints.Length == 0)
            {
                return false;
            }

            ShapePointDefinition[] normalizedCurrent = ShapeLayoutGeometry.NormalizePoints(currentPoints, 1f, 1f);
            ShapePointDefinition[] normalizedReference = ShapeLayoutGeometry.NormalizePoints(referencePoints, 1f, 1f);
            float directError = CalculatePointSetError(normalizedCurrent, normalizedReference);
            float flippedError = CalculatePointSetError(FlipY(normalizedCurrent), normalizedReference);
            return flippedError < (directError * 0.55f);
        }

        private static float CalculatePointSetError(ShapePointDefinition[] a, ShapePointDefinition[] b)
        {
            int count = Mathf.Min(a != null ? a.Length : 0, b != null ? b.Length : 0);
            if (count == 0)
            {
                return float.MaxValue;
            }

            float total = 0f;
            for (int i = 0; i < count; i++)
            {
                Vector2 av = new Vector2(a[i].x, a[i].y);
                Vector2 bv = new Vector2(b[i].x, b[i].y);
                total += Vector2.Distance(av, bv);
            }

            return total / count;
        }

        private static ShapePointDefinition[] FlipY(ShapePointDefinition[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<ShapePointDefinition>();
            }

            ShapePointDefinition[] flipped = new ShapePointDefinition[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                flipped[i] = new ShapePointDefinition
                {
                    x = source[i].x,
                    y = -source[i].y
                };
            }

            return flipped;
        }

        private static bool[] BuildShapeMask(Color32[] pixels, int width, int height)
        {
            bool hasTransparentPixels = false;
            bool hasOpaquePixels = false;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a < 240)
                {
                    hasTransparentPixels = true;
                }
                else
                {
                    hasOpaquePixels = true;
                }
            }

            bool[] mask = new bool[pixels.Length];
            if (hasTransparentPixels && hasOpaquePixels)
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    mask[i] = pixels[i].a > 16;
                }

                return mask;
            }

            Color background = EstimateBorderBackground(pixels, width, height);
            for (int i = 0; i < pixels.Length; i++)
            {
                mask[i] = IsForegroundPixel(pixels[i], background);
            }

            int foregroundCount = 0;
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i])
                {
                    foregroundCount++;
                }
            }

            if (foregroundCount < 8)
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    float luminance = ComputeLuminance(pixels[i]);
                    mask[i] = luminance < 0.55f;
                }
            }

            return mask;
        }

        private static Color EstimateBorderBackground(Color32[] pixels, int width, int height)
        {
            Vector3 total = Vector3.zero;
            int count = 0;
            for (int x = 0; x < width; x++)
            {
                total += ToVector3Color(pixels[x]);
                total += ToVector3Color(pixels[((height - 1) * width) + x]);
                count += 2;
            }

            for (int y = 1; y < height - 1; y++)
            {
                total += ToVector3Color(pixels[y * width]);
                total += ToVector3Color(pixels[(y * width) + (width - 1)]);
                count += 2;
            }

            if (count <= 0)
            {
                return Color.white;
            }

            Vector3 average = total / count;
            return new Color(average.x, average.y, average.z, 1f);
        }

        private static bool IsForegroundPixel(Color32 pixel, Color background)
        {
            Vector3 color = ToVector3Color(pixel);
            Vector3 backgroundColor = new Vector3(background.r, background.g, background.b);
            float colorDistance = Vector3.Distance(color, backgroundColor);
            float luminance = ComputeLuminance(pixel);
            float backgroundLuminance = (background.r * 0.2126f) + (background.g * 0.7152f) + (background.b * 0.0722f);
            return colorDistance > 0.18f || Mathf.Abs(luminance - backgroundLuminance) > 0.18f;
        }

        private static float ComputeLuminance(Color32 pixel)
        {
            float r = pixel.r / 255f;
            float g = pixel.g / 255f;
            float b = pixel.b / 255f;
            return (r * 0.2126f) + (g * 0.7152f) + (b * 0.0722f);
        }

        private static Vector3 ToVector3Color(Color32 pixel)
        {
            return new Vector3(pixel.r / 255f, pixel.g / 255f, pixel.b / 255f);
        }

        private static ShapePointDefinition[] FitPointsUniform(ShapePointDefinition[] source, float radiusX, float radiusY)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<ShapePointDefinition>();
            }

            Vector2 center = Vector2.zero;
            for (int i = 0; i < source.Length; i++)
            {
                center.x += source[i].x;
                center.y += source[i].y;
            }

            center /= source.Length;

            float maxAbsX = 0f;
            float maxAbsY = 0f;
            for (int i = 0; i < source.Length; i++)
            {
                maxAbsX = Mathf.Max(maxAbsX, Mathf.Abs(source[i].x - center.x));
                maxAbsY = Mathf.Max(maxAbsY, Mathf.Abs(source[i].y - center.y));
            }

            maxAbsX = Mathf.Max(0.0001f, maxAbsX);
            maxAbsY = Mathf.Max(0.0001f, maxAbsY);
            float targetRadiusX = Mathf.Max(0.2f, Mathf.Abs(radiusX));
            float targetRadiusY = Mathf.Max(0.2f, Mathf.Abs(radiusY));
            float scale = Mathf.Min(targetRadiusX / maxAbsX, targetRadiusY / maxAbsY);

            ShapePointDefinition[] fitted = new ShapePointDefinition[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                Vector2 offset = new Vector2(source[i].x - center.x, source[i].y - center.y) * scale;
                fitted[i] = new ShapePointDefinition { x = offset.x, y = offset.y };
            }

            return fitted;
        }

        private static float[] CreateZeroFloatArray(int count)
        {
            return new float[Mathf.Max(1, count)];
        }

        private static float[] ResampleFloatArrayLoop(float[] source, int targetCount)
        {
            targetCount = Mathf.Max(1, targetCount);
            if (source == null || source.Length == 0)
            {
                return CreateZeroFloatArray(targetCount);
            }

            if (source.Length == targetCount)
            {
                return source.ToArray();
            }

            if (source.Length == 1)
            {
                float[] single = new float[targetCount];
                for (int i = 0; i < targetCount; i++)
                {
                    single[i] = source[0];
                }

                return single;
            }

            float[] result = new float[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                float sample = (i / (float)targetCount) * source.Length;
                int startIndex = Mathf.FloorToInt(sample) % source.Length;
                int endIndex = (startIndex + 1) % source.Length;
                float t = sample - Mathf.Floor(sample);
                result[i] = Mathf.LerpAngle(source[startIndex], source[endIndex], t);
            }

            return result;
        }
    }
}
