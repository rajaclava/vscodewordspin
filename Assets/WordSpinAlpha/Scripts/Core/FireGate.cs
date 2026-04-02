using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class FireGate : MonoBehaviour
    {
        [SerializeField] private float launchCooldown = 0.3f;
        [SerializeField] private SlotManager slotManager;

        private bool _firePermit = true;
        private float _cooldownTimer;
        private bool _pinLoaded;

        public bool CanFire => _firePermit && _pinLoaded && slotManager != null && slotManager.ActiveSlotIndex >= 0;

        private void Update()
        {
            if (_firePermit)
            {
                return;
            }

            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer <= 0f)
            {
                _firePermit = true;
                _cooldownTimer = 0f;
            }
        }

        public void SetPinLoaded(bool loaded)
        {
            _pinLoaded = loaded;
        }

        public bool RequestFire()
        {
            if (!CanFire)
            {
                return false;
            }

            _firePermit = false;
            _cooldownTimer = launchCooldown;
            return true;
        }
    }
}
