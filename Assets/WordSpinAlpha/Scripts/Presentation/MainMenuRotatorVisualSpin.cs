using UnityEngine;

namespace WordSpinAlpha.Presentation
{
    public sealed class MainMenuRotatorVisualSpin : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float degreesPerSecond = 10f;

        private float _angle;

        private void Reset()
        {
            target = transform as RectTransform;
        }

        private void Awake()
        {
            if (target == null)
                target = transform as RectTransform;

            if (target != null)
            {
                _angle = 0f;
                target.localRotation = Quaternion.AngleAxis(0f, Vector3.forward);
            }
        }

        private void Update()
        {
            if (target == null || Mathf.Approximately(degreesPerSecond, 0f))
                return;

            _angle += degreesPerSecond * Time.unscaledDeltaTime;
            target.localRotation = Quaternion.AngleAxis(_angle, Vector3.forward);
        }
    }
}
