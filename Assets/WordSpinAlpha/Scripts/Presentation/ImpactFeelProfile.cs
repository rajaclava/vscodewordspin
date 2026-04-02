using System;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    [CreateAssetMenu(fileName = "ImpactFeelProfile", menuName = "WordSpin Alpha/Impact Feel Profile")]
    public class ImpactFeelProfile : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public HitResultType impactType;
            public float hitStopMs;
            [Range(0f, 1f)] public float hapticIntensity;
            [Range(0f, 1f)] public float hapticSharpness;
            [Range(0.8f, 1.4f)] public float audioAttack;
            public float cameraKick;
            public float settleMs;
            [Range(0.8f, 1.4f)] public float flashScale;
            [Range(0f, 2f)] public float particleBurst;
        }

        [SerializeField] private Entry[] entries =
        {
            new Entry
            {
                impactType = HitResultType.Perfect,
                hitStopMs = 32f,
                hapticIntensity = 0.85f,
                hapticSharpness = 0.9f,
                audioAttack = 1.08f,
                cameraKick = 0.22f,
                settleMs = 120f,
                flashScale = 1.12f,
                particleBurst = 1.15f
            },
            new Entry
            {
                impactType = HitResultType.Tolerated,
                hitStopMs = 20f,
                hapticIntensity = 0.6f,
                hapticSharpness = 0.68f,
                audioAttack = 1.02f,
                cameraKick = 0.16f,
                settleMs = 105f,
                flashScale = 1.06f,
                particleBurst = 0.8f
            },
            new Entry
            {
                impactType = HitResultType.NearMiss,
                hitStopMs = 8f,
                hapticIntensity = 0.35f,
                hapticSharpness = 0.5f,
                audioAttack = 0.96f,
                cameraKick = 0.12f,
                settleMs = 95f,
                flashScale = 1.02f,
                particleBurst = 0.25f
            },
            new Entry
            {
                impactType = HitResultType.WrongSlot,
                hitStopMs = 18f,
                hapticIntensity = 0.9f,
                hapticSharpness = 0.95f,
                audioAttack = 0.92f,
                cameraKick = 0.34f,
                settleMs = 155f,
                flashScale = 1.1f,
                particleBurst = 1.35f
            },
            new Entry
            {
                impactType = HitResultType.WrongLetter,
                hitStopMs = 10f,
                hapticIntensity = 0.62f,
                hapticSharpness = 0.82f,
                audioAttack = 0.94f,
                cameraKick = 0.28f,
                settleMs = 130f,
                flashScale = 1.04f,
                particleBurst = 0f
            },
            new Entry
            {
                impactType = HitResultType.Miss,
                hitStopMs = 10f,
                hapticIntensity = 0.55f,
                hapticSharpness = 0.7f,
                audioAttack = 0.94f,
                cameraKick = 0.22f,
                settleMs = 115f,
                flashScale = 1.03f,
                particleBurst = 0f
            }
        };

        public bool TryGetEntry(HitResultType type, out Entry entry)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].impactType == type)
                {
                    entry = entries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}
