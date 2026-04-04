using UnityEngine;
using WordSpinAlpha.Content;
using System.Collections.Generic;
using System.Linq;

namespace WordSpinAlpha.Core
{
    public class SlotManager : MonoBehaviour
    {
        [SerializeField] private Slot[] slots;
        [SerializeField] private Transform launcherTransform;
        [SerializeField] private float activationAngle = 18f;

        private int _activeSlotIndex = -1;
        private int _currentTargetSlot = -1;
        private readonly List<Slot> _slotPool = new List<Slot>();
        private Transform _slotRoot;
        private Slot _slotTemplate;

        public int ActiveSlotIndex => _activeSlotIndex;
        public int CurrentTargetSlot => _currentTargetSlot;
        public int SlotCount => slots != null ? slots.Length : 0;

        private void Awake()
        {
            CacheSlotPool();
        }

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
                if (layout != null && layout.slotCount > 0)
                {
                    EnsureSlotCount(layout.slotCount);
                }
            }

            if (slots == null || slots.Length == 0 || layout == null)
            {
                return;
            }

            EnsureSlotCount(Mathf.Max(1, layout.slotCount));
            ShapePointDefinition[] resolvedPoints = ShapeLayoutGeometry.ResolvePoints(layout);

            int count = slots.Length;
            for (int i = 0; i < count; i++)
            {
                Slot slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                Vector2 position = EvaluateShapePoint(layout, resolvedPoints, i, count);
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

        private void EnsureSlotCount(int desiredCount)
        {
            desiredCount = Mathf.Max(1, desiredCount);
            CacheSlotPool();
            if (_slotTemplate == null || _slotRoot == null)
            {
                return;
            }

            while (_slotPool.Count < desiredCount)
            {
                Slot clone = CreateSlotClone(_slotPool.Count);
                if (clone == null)
                {
                    break;
                }

                _slotPool.Add(clone);
            }

            List<Slot> activeSlots = new List<Slot>(desiredCount);
            for (int i = 0; i < _slotPool.Count; i++)
            {
                Slot slot = _slotPool[i];
                if (slot == null)
                {
                    continue;
                }

                bool shouldBeActive = i < desiredCount;
                if (slot.gameObject.activeSelf != shouldBeActive)
                {
                    slot.gameObject.SetActive(shouldBeActive);
                }

                slot.name = $"Slot_{i}";
                slot.transform.SetSiblingIndex(i);
                if (shouldBeActive)
                {
                    activeSlots.Add(slot);
                }
            }

            slots = activeSlots.ToArray();
        }

        private void CacheSlotPool()
        {
            _slotPool.Clear();

            if (slots != null && slots.Length > 0)
            {
                _slotTemplate = slots.FirstOrDefault(slot => slot != null);
                _slotRoot = _slotTemplate != null ? _slotTemplate.transform.parent : null;
            }

            if (_slotRoot == null)
            {
                return;
            }

            for (int i = 0; i < _slotRoot.childCount; i++)
            {
                Transform child = _slotRoot.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                Slot slot = child.GetComponent<Slot>();
                if (slot != null)
                {
                    _slotPool.Add(slot);
                }
            }

            _slotPool.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            if (_slotTemplate == null && _slotPool.Count > 0)
            {
                _slotTemplate = _slotPool[0];
            }
        }

        private Slot CreateSlotClone(int index)
        {
            if (_slotTemplate == null || _slotRoot == null)
            {
                return null;
            }

            GameObject cloneObject = Object.Instantiate(_slotTemplate.gameObject, _slotRoot);
            cloneObject.name = $"Slot_{index}";
            cloneObject.transform.SetSiblingIndex(index);
            cloneObject.SetActive(true);

            Slot slot = cloneObject.GetComponent<Slot>();
            if (slot != null)
            {
                slot.ClearAttachedPins();
            }

            return slot;
        }

        private static Vector2 EvaluateShapePoint(ShapeLayoutDefinition layout, ShapePointDefinition[] resolvedPoints, int index, int count)
        {
            if (resolvedPoints != null && resolvedPoints.Length > 0)
            {
                ShapePointDefinition customPoint = resolvedPoints[index % resolvedPoints.Length];
                return new Vector2(customPoint.x, customPoint.y);
            }

            return ShapeLayoutGeometry.EvaluateLayoutPoint(layout, index, count);
        }
    }
}
