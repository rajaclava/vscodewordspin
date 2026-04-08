using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class TargetRotator : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 45f;
        [SerializeField] private bool clockwise = true;
        [SerializeField] private float rhythmAssistUntil;
        [SerializeField] private float rhythmAssistSpeedMultiplier = 1f;
        private float _baseRotationSpeed = 45f;
        private bool _baseClockwise = true;

        public float RotationSpeed => rotationSpeed;
        public bool Clockwise => clockwise;
        public float BaseRotationSpeed => _baseRotationSpeed;
        public bool BaseClockwise => _baseClockwise;
        public float RhythmAssistSpeedMultiplier => rhythmAssistSpeedMultiplier;
        public float RhythmAssistRemainingSeconds => Mathf.Max(0f, rhythmAssistUntil - Time.time);

        public void ApplyLevelSettings(float speed, bool rotateClockwise)
        {
            _baseRotationSpeed = speed;
            _baseClockwise = rotateClockwise;
            rotationSpeed = speed;
            clockwise = rotateClockwise;
        }

        public void ApplyEditorBaseTuning(float speed, bool rotateClockwise)
        {
            ApplyLevelSettings(speed, rotateClockwise);
        }

        public void ApplyRhythmAssist(bool rotateClockwise, float speedMultiplier, float durationSeconds)
        {
            clockwise = rotateClockwise;
            rhythmAssistSpeedMultiplier = Mathf.Max(1f, speedMultiplier);
            rhythmAssistUntil = Time.time + Mathf.Max(0f, durationSeconds);
            rotationSpeed = _baseRotationSpeed * rhythmAssistSpeedMultiplier;
        }

        public void ClearRhythmAssist()
        {
            rhythmAssistUntil = 0f;
            rhythmAssistSpeedMultiplier = 1f;
            rotationSpeed = _baseRotationSpeed;
            clockwise = _baseClockwise;
        }

        private void Update()
        {
            if (rhythmAssistUntil > 0f && Time.time >= rhythmAssistUntil)
            {
                rhythmAssistUntil = 0f;
                rhythmAssistSpeedMultiplier = 1f;
                rotationSpeed = _baseRotationSpeed;
                clockwise = _baseClockwise;
            }

            float direction = clockwise ? -1f : 1f;
            transform.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime);
        }
    }
}
