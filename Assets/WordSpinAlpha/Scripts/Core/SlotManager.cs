using UnityEngine;
using WordSpinAlpha.Content;

namespace WordSpinAlpha.Core
{
    public class SlotManager : MonoBehaviour
    {
        [SerializeField] private Slot[] slots;
        [SerializeField] private Transform launcherTransform;
        [SerializeField] private float activationAngle = 18f;

        private int _activeSlotIndex = -1;
        private int _currentTargetSlot = -1;

        public int ActiveSlotIndex => _activeSlotIndex;
        public int CurrentTargetSlot => _currentTargetSlot;
        public int SlotCount => slots != null ? slots.Length : 0;

        private void Update()
        {
            if (slots == null || slots.Length == 0 || launcherTransform == null)
            {
                return;
            }

            DetectActiveSlot();
        }

        public void ConfigureSlots(char[] answerLetters)
        {
            if (slots == null)
            {
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].ClearAttachedPins();
                char letter = i < answerLetters.Length ? answerLetters[i] : '\0';
                slots[i].Configure(i, letter);
            }

            _activeSlotIndex = -1;
            _currentTargetSlot = -1;
        }

        public void ApplyShapeLayout(ShapeLayoutDefinition layout)
        {
            if (slots == null || slots.Length == 0 || layout == null)
            {
                return;
            }

            int count = slots.Length;
            for (int i = 0; i < count; i++)
            {
                Slot slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                Vector2 position = EvaluateShapePoint(layout, i, count);
                slot.transform.localPosition = new Vector3(position.x, position.y, 0f);

                Vector2 outward = position.sqrMagnitude > 0.0001f ? position.normalized : Vector2.up;
                float rotationZ = layout.useTangentialRotation
                    ? Mathf.Atan2(-outward.x, outward.y) * Mathf.Rad2Deg
                    : Mathf.Atan2(outward.y, outward.x) * Mathf.Rad2Deg - 90f;
                slot.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
                slot.ApplyShapeLayout(
                    new Vector2(Mathf.Max(0.14f, layout.plaqueWidth), Mathf.Max(0.10f, layout.plaqueHeight)),
                    layout.perfectWidthScale,
                    layout.perfectHeightScale,
                    layout.nearMissPadding);
            }
        }

        public void SetTargetSlot(int slotIndex, char targetLetter)
        {
            _currentTargetSlot = slotIndex;
            for (int i = 0; i < slots.Length; i++)
            {
                if (i == slotIndex)
                {
                    slots[i].Activate(targetLetter);
                }
                else
                {
                    slots[i].Deactivate();
                }
            }
        }

        public bool TryGetSlot(int slotIndex, out Slot slot)
        {
            slot = null;
            if (slots == null || slotIndex < 0 || slotIndex >= slots.Length)
            {
                return false;
            }

            slot = slots[slotIndex];
            return slot != null;
        }

        public Slot GetTargetSlot()
        {
            return TryGetSlot(_currentTargetSlot, out Slot slot) ? slot : null;
        }

        public bool TryFindPlaqueCandidate(Vector3 worldPoint, out Slot candidateSlot)
        {
            candidateSlot = null;
            if (slots == null)
            {
                return false;
            }

            float bestDistance = float.MaxValue;
            for (int i = 0; i < slots.Length; i++)
            {
                Slot slot = slots[i];
                if (slot == null || !slot.IsInsideNearMissZone(worldPoint))
                {
                    continue;
                }

                float distance = Vector3.SqrMagnitude(slot.transform.position - worldPoint);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    candidateSlot = slot;
                }
            }

            return candidateSlot != null;
        }

        public HitData EvaluatePlaqueHit(Slot slot, char letter, Vector3 pinTipWorldPoint, float perfectScale, float magnetScale, float nearMissScale)
        {
            int slotIndex = slot != null ? slot.SlotIndex : -1;
            HitData hit = new HitData
            {
                enteredLetter = char.ToUpperInvariant(letter),
                expectedLetter = _currentTargetSlot >= 0 && _currentTargetSlot < slots.Length ? slots[_currentTargetSlot].TargetLetter : '\0',
                slotIndex = slotIndex,
                expectedSlotIndex = _currentTargetSlot
            };

            if (slot == null)
            {
                hit.resultType = HitResultType.Miss;
                return hit;
            }

            if (slotIndex != _currentTargetSlot)
            {
                hit.resultType = HitResultType.WrongSlot;
                return hit;
            }

            if (hit.enteredLetter != hit.expectedLetter)
            {
                hit.resultType = HitResultType.WrongLetter;
                return hit;
            }

            if (slot.IsInsidePerfectZone(pinTipWorldPoint, perfectScale))
            {
                hit.resultType = HitResultType.Perfect;
                hit.precisionScore = 1f;
            }
            else if (slot.IsInsideMagnetZone(pinTipWorldPoint, magnetScale))
            {
                hit.resultType = HitResultType.Tolerated;
                Vector2 local = slot.GetPlaqueLocalPoint(pinTipWorldPoint);
                float x = Mathf.Abs(local.x) / Mathf.Max(0.001f, (slot.PlaqueSize.x * magnetScale) * 0.5f);
                float y = Mathf.Abs(local.y) / Mathf.Max(0.001f, (slot.PlaqueSize.y * magnetScale) * 0.5f);
                float edge = Mathf.Clamp01(Mathf.Max(x, y));
                hit.precisionScore = Mathf.Lerp(0.55f, 0.9f, 1f - edge);
            }
            else if (slot.IsInsideNearMissZone(pinTipWorldPoint, nearMissScale))
            {
                hit.resultType = HitResultType.NearMiss;
                hit.precisionScore = 0.1f;
            }
            else
            {
                hit.resultType = HitResultType.Miss;
                hit.precisionScore = 0f;
            }

            return hit;
        }

        public float GetImpactAngle(Slot slot)
        {
            if (slot == null || launcherTransform == null)
            {
                return 999f;
            }

            Vector3 toSlot = (slot.transform.position - launcherTransform.position).normalized;
            return Vector3.Angle(launcherTransform.up, toSlot);
        }

        public float EstimateArrivalTime(int slotIndex, float rotationSpeedDegrees, bool rotateClockwise)
        {
            if (!TryGetSlot(slotIndex, out Slot slot) || launcherTransform == null || rotationSpeedDegrees <= 0.001f)
            {
                return float.MaxValue;
            }

            Vector2 launcherUp = launcherTransform.up;
            Vector2 toSlot = ((Vector2)slot.transform.position - (Vector2)launcherTransform.position).normalized;
            float signedAngle = Vector2.SignedAngle(launcherUp, toSlot);
            float travelDegrees;

            if (rotateClockwise)
            {
                travelDegrees = signedAngle >= 0f ? signedAngle : 360f + signedAngle;
            }
            else
            {
                travelDegrees = signedAngle <= 0f ? -signedAngle : 360f - signedAngle;
            }

            return travelDegrees / rotationSpeedDegrees;
        }

        private void DetectActiveSlot()
        {
            int bestIndex = -1;
            float bestAngle = activationAngle;

            for (int i = 0; i < slots.Length; i++)
            {
                Vector3 toSlot = (slots[i].transform.position - launcherTransform.position).normalized;
                float angle = Vector3.Angle(launcherTransform.up, toSlot);
                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    bestIndex = i;
                }
            }

            _activeSlotIndex = bestIndex;
        }

        private static Vector2 EvaluateShapePoint(ShapeLayoutDefinition layout, int index, int count)
        {
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

        private static Vector2 EvaluateDiamondPoint(int index, int count, float radiusX, float radiusY, float rotationOffsetDegrees)
        {
            float t = (index / (float)count);
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

        private static Vector2 Rotate(Vector2 point, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(point.x * cos - point.y * sin, point.x * sin + point.y * cos);
        }
    }
}
