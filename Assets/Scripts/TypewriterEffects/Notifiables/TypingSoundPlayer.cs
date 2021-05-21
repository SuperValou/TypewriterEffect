using UnityEngine;

namespace Assets.Scripts.TypewriterEffects.Notifiables
{
    public class TypingSoundPlayer : MonoBehaviour, ITypingNotifiable
    {
        // -- Editor

        [Header("Values")]
        [Tooltip("Minimal delay before playing a typing sound (seconds).")]
        public float minTypingDelay = 0.03f;

        [Tooltip("Maximum value of a randomized additional delay (seconds).")]
        public float maxTypingRandomizedDelay = 0.05f;

        [Header("Parts")]
        public AudioClip soundToPlay;
        public AudioSource audioSource;

        // -- Class

        private float _lastTypingTime = 0;
        private float _typingSoundDelay = 0;

        void Start()
        {
            _typingSoundDelay = minTypingDelay;
        }

        public void OnTypingBegin()
        {
            // do nothing
        }

        public void OnCaretMove()
        {
            if (Time.unscaledTime < _lastTypingTime + _typingSoundDelay)
            {
                return;
            }

            audioSource.PlayOneShot(soundToPlay);
            _lastTypingTime = Time.unscaledTime;
            _typingSoundDelay = minTypingDelay + Random.Range(0, maxTypingRandomizedDelay);
        }

        public void OnTypingEnd()
        {
            // do nothing
        }
    }
}
