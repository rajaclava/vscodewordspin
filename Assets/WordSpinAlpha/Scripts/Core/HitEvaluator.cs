using UnityEngine;
using WordSpinAlpha.Content;

namespace WordSpinAlpha.Core
{
    public class HitEvaluator : MonoBehaviour
    {
        [SerializeField] private SlotManager slotManager;

        public HitData EvaluateImpact(Slot slot, char letter, Vector3 pinTipWorldPoint, DifficultyProfileDefinition profile, DifficultyTierDefinition tier)
        {
            float perfectScale = profile != null ? Mathf.Clamp(profile.perfectAngle / 6.5f, 0.70f, 1.30f) : 1f;
            float goodScale = profile != null ? Mathf.Clamp(profile.toleranceAngle / 15f, 0.85f, 1.35f) : 1f;
            float nearMissScale = goodScale;

            if (tier != null)
            {
                perfectScale *= Mathf.Clamp(tier.perfectWindowScale, 0.65f, 1.8f);
                goodScale *= Mathf.Clamp(tier.goodWindowScale, 0.8f, 1.8f);
                nearMissScale *= Mathf.Clamp(tier.nearMissScale, 0.9f, 2.0f);
            }

            return slotManager.EvaluatePlaqueHit(slot, letter, pinTipWorldPoint, perfectScale, goodScale, nearMissScale);
        }
    }
}
