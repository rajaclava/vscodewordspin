using System.Collections.Generic;
using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class InputBuffer : MonoBehaviour
    {
        [SerializeField] private int maxCapacity = 3;

        private readonly Queue<char> _buffer = new Queue<char>();
        private char _expectedLetter;

        public void SetExpectedLetter(char letter)
        {
            _expectedLetter = char.ToUpperInvariant(letter);
        }

        public void ClearExpectedLetter()
        {
            _expectedLetter = '\0';
            _buffer.Clear();
        }

        public bool IsWrongLetter(char candidate)
        {
            if (_expectedLetter == '\0')
            {
                return false;
            }

            return char.ToUpperInvariant(candidate) != _expectedLetter;
        }

        public bool TryAdd(char candidate)
        {
            if (_buffer.Count >= maxCapacity || IsWrongLetter(candidate))
            {
                return false;
            }

            _buffer.Enqueue(char.ToUpperInvariant(candidate));
            return true;
        }

        public bool TryPop(out char candidate)
        {
            if (_buffer.Count == 0)
            {
                candidate = '\0';
                return false;
            }

            candidate = _buffer.Dequeue();
            return true;
        }
    }
}
