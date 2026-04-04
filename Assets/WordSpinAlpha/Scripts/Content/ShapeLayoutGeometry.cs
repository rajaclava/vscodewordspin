using System;
using System.Collections.Generic;
using UnityEngine;

namespace WordSpinAlpha.Content
{
    public struct ShapePlaqueVisualLayoutInfo
    {
        public Vector2 plaqueSize;
        public Vector2 innerSize;
        public Vector2 runeSize;
        public Vector2 seatSize;
        public Vector2 glowSize;
        public float outwardOffset;
        public float localRotationDegrees;
    }

    public static class ShapeLayoutGeometry
    {
        public static ShapePointDefinition[] ResolvePoints(ShapeLayoutDefinition layout)
        {
            if (layout == null)
            {
                return Array.Empty<ShapePointDefinition>();
            }

            int slotCount = Mathf.Max(1, layout.slotCount);
            ShapePointDefinition[] customPoints = PrepareCustomPoints(layout, slotCount);
            if (customPoints != null && customPoints.Length > 0)
            {
                return AutoFitForGameplay(layout, customPoints);
            }

            return AutoFitForGameplay(layout, GenerateFallbackPoints(layout, slotCount));
        }

        public static ShapePointDefinition[] PrepareCustomPoints(ShapeLayoutDefinition layout, int slotCount)
        {
            if (layout == null || layout.customPoints == null || layout.customPoints.Length == 0)
            {
                return null;
            }

            ShapePointDefinition[] points = ClonePoints(layout.customPoints);
            if (ContainsInvalid(points))
            {
                return null;
            }

            if (points.Length != slotCount)
            {
                points = ResamplePoints(points, slotCount);
            }

            if (LooksCorrupted(points, layout.radiusX, layout.radiusY) || HasTinyExtent(points))
            {
                points = NormalizePoints(points, layout.radiusX, layout.radiusY);
            }

            return points;
        }

        public static ShapePointDefinition[] GenerateFallbackPoints(ShapeLayoutDefinition layout, int slotCount)
        {
            slotCount = Mathf.Max(1, slotCount);
            ShapePointDefinition[] points = new ShapePointDefinition[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                Vector2 point = EvaluateLayoutPoint(layout, i, slotCount);
                points[i] = new ShapePointDefinition { x = point.x, y = point.y };
            }

            return points;
        }

        public static ShapePointDefinition[] AutoFitForGameplay(ShapeLayoutDefinition layout, ShapePointDefinition[] source)
        {
            if (layout == null || source == null || source.Length == 0)
            {
                return source ?? Array.Empty<ShapePointDefinition>();
            }

            bool isCustomLayout = string.Equals(layout.shapeFamily, "custom", StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrWhiteSpace(layout.editorReferenceImagePath);
            if (!isCustomLayout)
            {
                return source;
            }

            ShapePointDefinition[] points = ClonePoints(source);
            if (!layout.gameplayAutoFit)
            {
                return points;
            }

            points = NormalizePoints(points, layout.radiusX, layout.radiusY);

            float minSpacing = ComputeMinimumPairDistance(points);
            float targetSpacing = ComputeTargetSpacing(layout);
            if (minSpacing <= 0.0001f)
            {
                return points;
            }

            if (minSpacing < targetSpacing)
            {
                float scale = Mathf.Clamp(targetSpacing / minSpacing, 1f, 2.8f);
                points = ScaleAroundCenter(points, scale);
            }

            return points;
        }

        public static ShapePointDefinition[] GenerateAutoShapePoints(string shapeFamily, int slotCount, float radiusX, float radiusY, float rotationOffsetDegrees)
        {
            ShapeLayoutDefinition layout = new ShapeLayoutDefinition
            {
                shapeFamily = shapeFamily,
                slotCount = Mathf.Max(1, slotCount),
                radiusX = radiusX,
                radiusY = radiusY,
                rotationOffsetDegrees = rotationOffsetDegrees
            };

            return GenerateFallbackPoints(layout, layout.slotCount);
        }

        public static ShapePointDefinition[] ClonePoints(ShapePointDefinition[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<ShapePointDefinition>();
            }

            ShapePointDefinition[] clone = new ShapePointDefinition[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                ShapePointDefinition point = source[i] ?? new ShapePointDefinition();
                clone[i] = new ShapePointDefinition { x = point.x, y = point.y };
            }

            return clone;
        }

        public static ShapePointDefinition[] NormalizePoints(ShapePointDefinition[] source, float radiusX, float radiusY)
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

            ShapePointDefinition[] normalized = new ShapePointDefinition[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                float x = ((source[i].x - center.x) / maxAbsX) * targetRadiusX;
                float y = ((source[i].y - center.y) / maxAbsY) * targetRadiusY;
                normalized[i] = new ShapePointDefinition { x = x, y = y };
            }

            return normalized;
        }

        public static ShapePointDefinition[] ResamplePoints(ShapePointDefinition[] source, int targetCount)
        {
            targetCount = Mathf.Max(1, targetCount);
            if (source == null || source.Length == 0)
            {
                return Array.Empty<ShapePointDefinition>();
            }

            if (source.Length == 1)
            {
                ShapePointDefinition[] single = new ShapePointDefinition[targetCount];
                for (int i = 0; i < targetCount; i++)
                {
                    single[i] = new ShapePointDefinition { x = source[0].x, y = source[0].y };
                }

                return single;
            }

            if (source.Length == targetCount)
            {
                return ClonePoints(source);
            }

            Vector2[] vectors = new Vector2[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                vectors[i] = new Vector2(source[i].x, source[i].y);
            }

            float[] cumulative = new float[source.Length + 1];
            cumulative[0] = 0f;
            for (int i = 0; i < source.Length; i++)
            {
                Vector2 a = vectors[i];
                Vector2 b = vectors[(i + 1) % source.Length];
                cumulative[i + 1] = cumulative[i] + Vector2.Distance(a, b);
            }

            float perimeter = cumulative[source.Length];
            if (perimeter <= 0.0001f)
            {
                return ClonePoints(source);
            }

            ShapePointDefinition[] resampled = new ShapePointDefinition[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                float targetDistance = (perimeter * i) / targetCount;
                int segmentIndex = 0;
                while (segmentIndex < source.Length - 1 && cumulative[segmentIndex + 1] < targetDistance)
                {
                    segmentIndex++;
                }

                float segmentStart = cumulative[segmentIndex];
                float segmentEnd = cumulative[segmentIndex + 1];
                float segmentLength = Mathf.Max(0.0001f, segmentEnd - segmentStart);
                float t = Mathf.Clamp01((targetDistance - segmentStart) / segmentLength);
                Vector2 a = vectors[segmentIndex];
                Vector2 b = vectors[(segmentIndex + 1) % source.Length];
                Vector2 point = Vector2.Lerp(a, b, t);
                resampled[i] = new ShapePointDefinition { x = point.x, y = point.y };
            }

            return resampled;
        }

        public static bool LooksCorrupted(ShapePointDefinition[] points, float radiusX, float radiusY)
        {
            if (points == null || points.Length == 0)
            {
                return false;
            }

            float maxExpected = Mathf.Max(Mathf.Abs(radiusX), Mathf.Abs(radiusY));
            maxExpected = Mathf.Max(1f, maxExpected) * 8f;
            for (int i = 0; i < points.Length; i++)
            {
                if (Mathf.Abs(points[i].x) > maxExpected || Mathf.Abs(points[i].y) > maxExpected)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasTinyExtent(ShapePointDefinition[] points)
        {
            if (points == null || points.Length == 0)
            {
                return true;
            }

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                minX = Mathf.Min(minX, points[i].x);
                maxX = Mathf.Max(maxX, points[i].x);
                minY = Mathf.Min(minY, points[i].y);
                maxY = Mathf.Max(maxY, points[i].y);
            }

            return (maxX - minX) < 0.05f || (maxY - minY) < 0.05f;
        }

        public static float ComputeMinimumPairDistance(ShapePointDefinition[] points)
        {
            if (points == null || points.Length < 2)
            {
                return 0f;
            }

            float minDistance = float.MaxValue;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 a = new Vector2(points[i].x, points[i].y);
                for (int j = i + 1; j < points.Length; j++)
                {
                    Vector2 b = new Vector2(points[j].x, points[j].y);
                    float distance = Vector2.Distance(a, b);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }

            return minDistance == float.MaxValue ? 0f : minDistance;
        }

        public static Vector2 EvaluateLayoutPoint(ShapeLayoutDefinition layout, int index, int count)
        {
            layout = layout ?? new ShapeLayoutDefinition();
            count = Mathf.Max(1, count);

            float angle;
            if (layout.angleOverrides != null && index < layout.angleOverrides.Length)
            {
                angle = layout.angleOverrides[index] * Mathf.Deg2Rad;
            }
            else
            {
                angle = ((index / (float)count) * Mathf.PI * 2f) + (layout.rotationOffsetDegrees * Mathf.Deg2Rad);
            }

            Vector2 point;
            switch ((layout.shapeFamily ?? "circle").ToLowerInvariant())
            {
                case "oval":
                case "ellipse":
                    point = new Vector2(Mathf.Sin(angle) * layout.radiusX, Mathf.Cos(angle) * layout.radiusY);
                    break;

                case "diamond":
                    point = EvaluateDiamondPoint(index, count, layout.radiusX, layout.radiusY, layout.rotationOffsetDegrees);
                    break;

                case "hex":
                    point = EvaluatePolygonPoint(index, count, 6, layout.radiusX, layout.radiusY, layout.rotationOffsetDegrees);
                    break;

                case "square":
                    point = EvaluatePolygonPoint(index, count, 4, layout.radiusX, layout.radiusY, layout.rotationOffsetDegrees);
                    break;

                case "star":
                    point = EvaluateStarPoint(index / (float)count, layout.radiusX, layout.radiusY, layout.rotationOffsetDegrees * Mathf.Deg2Rad);
                    break;

                case "heart":
                    point = EvaluateHeartPoint(index / (float)count, layout.radiusX, layout.radiusY);
                    point = Rotate(point, layout.rotationOffsetDegrees);
                    break;

                default:
                    point = new Vector2(Mathf.Sin(angle) * layout.radiusX, Mathf.Cos(angle) * layout.radiusY);
                    break;
            }

            if (layout.pointRadiusScales != null && index < layout.pointRadiusScales.Length)
            {
                point *= Mathf.Max(0.35f, layout.pointRadiusScales[index]);
            }

            return point;
        }

        public static ShapePlaqueVisualLayoutInfo ResolvePlaqueVisualLayout(ShapeLayoutDefinition layout, ShapePointDefinition[] points, int index, int count)
        {
            Vector2 basePlaqueSize = new Vector2(
                Mathf.Max(0.14f, layout != null ? layout.plaqueWidth : 0.30f),
                Mathf.Max(0.10f, layout != null ? layout.plaqueHeight : 0.18f)) * 1.55f;
            ShapePlaqueVisualLayoutInfo info = CreateDefaultPlaqueVisualLayout(basePlaqueSize);
            if (layout == null || points == null || points.Length == 0)
            {
                return info;
            }

            count = Mathf.Max(1, points.Length);
            index = Mod(index, count);
            if (!ShouldAdaptPlaqueVisuals(layout))
            {
                return info;
            }

            Vector2 previous = ToVector2(points[Mod(index - 1, count)]);
            Vector2 current = ToVector2(points[index]);
            Vector2 next = ToVector2(points[Mod(index + 1, count)]);

            float previousDistance = Vector2.Distance(previous, current);
            float nextDistance = Vector2.Distance(current, next);
            float averageAdjacentDistance = Mathf.Max(0.0001f, ComputeAverageAdjacentDistance(points));
            float neighbourMinimum = Mathf.Max(0.0001f, Mathf.Min(previousDistance, nextDistance));
            float usableWidth = neighbourMinimum - Mathf.Max(0f, layout.plaqueVisualPadding);

            float minWidthScale = Mathf.Clamp(layout.plaqueVisualMinWidthScale, 0.35f, 1.25f);
            float maxWidthScale = Mathf.Max(minWidthScale, layout.plaqueVisualMaxWidthScale);
            float minHeightScale = Mathf.Clamp(layout.plaqueVisualMinHeightScale, 0.35f, 1.25f);
            float maxHeightScale = Mathf.Max(minHeightScale, layout.plaqueVisualMaxHeightScale);

            float spacingRatio = Mathf.Clamp01(neighbourMinimum / averageAdjacentDistance);
            Vector2 incoming = (current - previous).normalized;
            Vector2 outgoing = (next - current).normalized;
            if (incoming.sqrMagnitude < 0.0001f)
            {
                incoming = (current.sqrMagnitude > 0.0001f ? current : Vector2.up).normalized;
            }

            if (outgoing.sqrMagnitude < 0.0001f)
            {
                outgoing = incoming;
            }

            float curvature = Mathf.Clamp01(Vector2.Angle(incoming, outgoing) / 110f);
            float tightness = Mathf.Clamp01(((1f - spacingRatio) * 0.70f) + (curvature * 0.45f));

            float targetWidth = Mathf.Clamp(usableWidth, basePlaqueSize.x * minWidthScale, basePlaqueSize.x * maxWidthScale);
            float heightScale = Mathf.Lerp(maxHeightScale, minHeightScale, tightness);
            float targetHeight = basePlaqueSize.y * heightScale;

            info.plaqueSize = new Vector2(targetWidth, targetHeight);
            info.innerSize = new Vector2(info.plaqueSize.x * 0.78f, info.plaqueSize.y * 0.76f);
            info.runeSize = new Vector2(Mathf.Max(0.08f, info.innerSize.x * 0.16f), Mathf.Max(0.10f, info.innerSize.y * 0.58f));
            info.seatSize = info.plaqueSize + new Vector2(0.10f + ((targetWidth / Mathf.Max(0.0001f, basePlaqueSize.x)) * 0.06f), 0.10f + ((targetHeight / Mathf.Max(0.0001f, basePlaqueSize.y)) * 0.04f));
            info.glowSize = info.plaqueSize + new Vector2(0.10f, 0.10f);
            info.outwardOffset = tightness * Mathf.Max(0f, layout.plaqueVisualOutwardOffset);
            info.localRotationDegrees = ComputeContourFollowRotation(layout, points, index, count, current, previous, next) + ResolveManualPlaqueAngleOffset(layout, index, count);
            return info;
        }

        public static bool UsesAdaptivePlaqueVisuals(ShapeLayoutDefinition layout)
        {
            return ShouldAdaptPlaqueVisuals(layout);
        }

        public static float ResolveManualPlaqueAngleOffset(ShapeLayoutDefinition layout, int index, int count)
        {
            if (layout == null || layout.plaqueVisualAngleOffsets == null || layout.plaqueVisualAngleOffsets.Length == 0)
            {
                return 0f;
            }

            count = Mathf.Max(1, count);
            index = Mod(index, count);
            if (layout.plaqueVisualAngleOffsets.Length != count)
            {
                int mappedIndex = Mathf.Clamp(Mathf.RoundToInt((index / (float)count) * (layout.plaqueVisualAngleOffsets.Length - 1)), 0, layout.plaqueVisualAngleOffsets.Length - 1);
                return layout.plaqueVisualAngleOffsets[mappedIndex];
            }

            return layout.plaqueVisualAngleOffsets[index];
        }

        private static bool ContainsInvalid(ShapePointDefinition[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (float.IsNaN(points[i].x) || float.IsInfinity(points[i].x) || float.IsNaN(points[i].y) || float.IsInfinity(points[i].y))
                {
                    return true;
                }
            }

            return false;
        }

        private static ShapePlaqueVisualLayoutInfo CreateDefaultPlaqueVisualLayout(Vector2 basePlaqueSize)
        {
            return new ShapePlaqueVisualLayoutInfo
            {
                plaqueSize = basePlaqueSize,
                innerSize = basePlaqueSize * new Vector2(0.78f, 0.78f),
                runeSize = new Vector2(basePlaqueSize.x * 0.14f, basePlaqueSize.y * 0.48f),
                seatSize = basePlaqueSize + new Vector2(0.16f, 0.14f),
                glowSize = basePlaqueSize + new Vector2(0.10f, 0.10f),
                outwardOffset = 0f,
                localRotationDegrees = 0f
            };
        }

        private static bool ShouldAdaptPlaqueVisuals(ShapeLayoutDefinition layout)
        {
            if (layout == null)
            {
                return false;
            }

            if (layout.adaptivePlaqueVisuals)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(layout.editorReferenceImagePath))
            {
                return true;
            }

            string family = layout.shapeFamily ?? string.Empty;
            return string.Equals(family, "custom", StringComparison.OrdinalIgnoreCase)
                || string.Equals(family, "heart", StringComparison.OrdinalIgnoreCase)
                || string.Equals(family, "star", StringComparison.OrdinalIgnoreCase);
        }

        private static float ComputeAverageAdjacentDistance(ShapePointDefinition[] points)
        {
            if (points == null || points.Length < 2)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 current = ToVector2(points[i]);
                Vector2 next = ToVector2(points[(i + 1) % points.Length]);
                total += Vector2.Distance(current, next);
            }

            return total / points.Length;
        }

        private static float ComputeContourFollowRotation(ShapeLayoutDefinition layout, ShapePointDefinition[] points, int index, int count, Vector2 current, Vector2 previous, Vector2 next)
        {
            if (layout == null || !layout.useTangentialRotation)
            {
                return 0f;
            }

            float contourFollow = Mathf.Clamp01(layout.plaqueVisualContourFollow);
            if (contourFollow <= 0.0001f)
            {
                return 0f;
            }

            Vector2 contourTangent = (next - previous).normalized;
            if (contourTangent.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            Vector2 outward = current.sqrMagnitude > 0.0001f ? current.normalized : Vector2.up;
            float radialRotation = Mathf.Atan2(-outward.x, outward.y) * Mathf.Rad2Deg;
            float contourRotation = Mathf.Atan2(contourTangent.y, contourTangent.x) * Mathf.Rad2Deg;
            float delta = Mathf.DeltaAngle(radialRotation, contourRotation);
            return Mathf.Clamp(delta * contourFollow, -18f, 18f);
        }

        private static Vector2 ToVector2(ShapePointDefinition point)
        {
            return point == null ? Vector2.zero : new Vector2(point.x, point.y);
        }

        private static int Mod(int value, int modulo)
        {
            if (modulo <= 0)
            {
                return 0;
            }

            int result = value % modulo;
            return result < 0 ? result + modulo : result;
        }

        private static float ComputeTargetSpacing(ShapeLayoutDefinition layout)
        {
            float plaqueVisualWidth = Mathf.Max(0.14f, layout.plaqueWidth) * 1.55f;
            float plaqueVisualHeight = Mathf.Max(0.10f, layout.plaqueHeight) * 1.55f;
            float seatWidth = plaqueVisualWidth + 0.16f;
            float seatHeight = plaqueVisualHeight + 0.14f;
            return Mathf.Max(seatWidth * 0.92f, seatHeight * 1.18f);
        }

        private static ShapePointDefinition[] ScaleAroundCenter(ShapePointDefinition[] points, float scale)
        {
            if (points == null || points.Length == 0)
            {
                return Array.Empty<ShapePointDefinition>();
            }

            Vector2 center = Vector2.zero;
            for (int i = 0; i < points.Length; i++)
            {
                center.x += points[i].x;
                center.y += points[i].y;
            }

            center /= points.Length;

            ShapePointDefinition[] scaled = new ShapePointDefinition[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 vector = new Vector2(points[i].x, points[i].y) - center;
                vector *= scale;
                vector += center;
                scaled[i] = new ShapePointDefinition { x = vector.x, y = vector.y };
            }

            return scaled;
        }

        private static Vector2 EvaluateDiamondPoint(int index, int count, float radiusX, float radiusY, float rotationOffsetDegrees)
        {
            float t = index / (float)count;
            Vector2 point;
            if (t < 0.25f)
            {
                point = Vector2.Lerp(new Vector2(0f, radiusY), new Vector2(radiusX, 0f), t / 0.25f);
            }
            else if (t < 0.5f)
            {
                point = Vector2.Lerp(new Vector2(radiusX, 0f), new Vector2(0f, -radiusY), (t - 0.25f) / 0.25f);
            }
            else if (t < 0.75f)
            {
                point = Vector2.Lerp(new Vector2(0f, -radiusY), new Vector2(-radiusX, 0f), (t - 0.5f) / 0.25f);
            }
            else
            {
                point = Vector2.Lerp(new Vector2(-radiusX, 0f), new Vector2(0f, radiusY), (t - 0.75f) / 0.25f);
            }

            return Rotate(point, rotationOffsetDegrees);
        }

        private static Vector2 EvaluatePolygonPoint(int index, int count, int sides, float radiusX, float radiusY, float rotationOffsetDegrees)
        {
            float t = index / (float)count;
            float scaled = t * sides;
            int side = Mathf.FloorToInt(scaled) % sides;
            float localT = scaled - Mathf.Floor(scaled);
            float startAngle = rotationOffsetDegrees * Mathf.Deg2Rad;
            float angleA = startAngle + ((side / (float)sides) * Mathf.PI * 2f);
            float angleB = startAngle + (((side + 1) / (float)sides) * Mathf.PI * 2f);
            Vector2 a = new Vector2(Mathf.Sin(angleA) * radiusX, Mathf.Cos(angleA) * radiusY);
            Vector2 b = new Vector2(Mathf.Sin(angleB) * radiusX, Mathf.Cos(angleB) * radiusY);
            return Vector2.Lerp(a, b, localT);
        }

        private static Vector2 EvaluateStarPoint(float t, float radiusX, float radiusY, float rotation)
        {
            float angle = (t * Mathf.PI * 2f) + rotation;
            float modulation = 0.62f + (0.38f * Mathf.Cos(angle * 5f));
            return new Vector2(Mathf.Sin(angle) * radiusX * modulation, Mathf.Cos(angle) * radiusY * modulation);
        }

        private static Vector2 EvaluateHeartPoint(float t, float radiusX, float radiusY)
        {
            float angle = (t * Mathf.PI * 2f) + Mathf.PI;
            float x = 16f * Mathf.Pow(Mathf.Sin(angle), 3f);
            float y = (13f * Mathf.Cos(angle)) - (5f * Mathf.Cos(2f * angle)) - (2f * Mathf.Cos(3f * angle)) - Mathf.Cos(4f * angle);
            return new Vector2((x / 16f) * radiusX, (y / 17f) * radiusY * 0.95f);
        }

        private static Vector2 Rotate(Vector2 point, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2((point.x * cos) - (point.y * sin), (point.x * sin) + (point.y * cos));
        }
    }
}
