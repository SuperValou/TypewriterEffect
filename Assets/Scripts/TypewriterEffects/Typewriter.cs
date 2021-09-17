using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.TypewriterEffects.Anims;
using Assets.Scripts.TypewriterEffects.Notifiables;
using Assets.Scripts.TypewriterEffects.Parsing;
using Assets.Scripts.TypewriterEffects.Tokens;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.TypewriterEffects
{
    public class Typewriter : MonoBehaviour
    {
        // -- Inspector

        [Header("Values")]
        [Tooltip("Time to reveal a character (seconds).")]
        public float charRevealTime = 0.05f;

        [Tooltip("Delay spent by the caret before moving to the next character, at normal speed (seconds).")]
        public float defaultCaretDelay = 0.02f;
        
        [Header("Target")]
        [SerializeField]
        private TMP_Text _targetTextMesh;

        [Header("References")]
        public MonoBehaviour[] typingNotifiables;

        // -- Class

        private readonly TextTokenizer _tokenizer = new TextTokenizer();
        private readonly TextParser _parser = new TextParser();
        private readonly ICollection<ITypingNotifiable> _typingNotifiables = new List<ITypingNotifiable>();

        private bool _skipToEnd = false;
        private Coroutine _animationCoroutine;

        private int _caretIndex = 0;

        private float _lastCaretMoveTime = 0;
        private float _caretDelay = 0;
        
        void Start()
        {
            if (_targetTextMesh == null)
            {
                throw new InvalidOperationException($"{nameof(_targetTextMesh)} of {nameof(Typewriter)} ({this.gameObject}) is null. " +
                                                    $"Did you forget to set a target?");
            }

            foreach (var typingNotifiable in typingNotifiables)
            {
                _typingNotifiables.Add((ITypingNotifiable) typingNotifiable);
            }
        }

        /// <summary>
        /// Change the current TextMesh target.
        /// </summary>
        public void SetTargetTextMesh(TMP_Text textMesh)
        {
            Stop();
            _targetTextMesh = textMesh ?? throw new ArgumentNullException(nameof(textMesh));
        }

        /// <summary>
        /// Animate the text from the targeted TextMeshPro component.
        /// </summary>
        public void Animate()
        {
            Animate(_targetTextMesh.text);
        }

        /// <summary>
        /// Animate the given text into the targeted TextMeshPro component.
        /// </summary>
        public void Animate(string inputText)
        {
            if (inputText == null)
            {
                throw new ArgumentNullException(nameof(inputText));
            }

            Stop();
            
            AnimableText animableText = GetAnimableText(inputText);
            
            // Apply message and compute characters
            _targetTextMesh.textInfo.ClearAllMeshInfo();
            _targetTextMesh.text = animableText.GetRichText();
            _targetTextMesh.ForceMeshUpdate();

            TMP_TextInfo textInfo = _targetTextMesh.textInfo;

            // Initialize anims for each char
            List<CharAnimInfo> charAnims = new List<CharAnimInfo>();

            int charCount = textInfo.characterCount;
            for (int charIndex = 0; charIndex < charCount; charIndex++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
                if (!charInfo.isVisible)
                {
                    continue;
                }

                var materialIndex = charInfo.materialReferenceIndex;
                var vertexIndex = charInfo.vertexIndex;

                // Get vertices data of current char
                Color32[] allColors = textInfo.meshInfo[materialIndex].colors32;
                var allVertices = textInfo.meshInfo[materialIndex].vertices;
                var data = new VerticesData();
                for (int i = 0; i < VerticesData.Count; i++)
                {
                    data.Positions[i] = allVertices[vertexIndex + i];
                    data.Colors[i] = allColors[vertexIndex + i];
                }
                
                // Get effect to apply to current char
                var charEffects = animableText.GetCharacterEffects(charIndex);
                var charAnim = new CharAnimInfo(charIndex, charEffects, charInfo.vertexIndex, charRevealTime, data);
                charAnims.Add(charAnim);
            }

            // Get caret effects
            var caretEffects = animableText.GetCaretEffects();

            // Run animation
            _animationCoroutine = StartCoroutine(RunAnimationRoutine(textInfo, charAnims, caretEffects));
        }

        /// <summary>
        /// Move the caret directly to the end of the animated text.
        /// </summary>
        public void SkipToEnd()
        {
            _skipToEnd = true;
        }

        /// <summary>
        /// Stop the animation.
        /// </summary>
        public void Stop()
        {
            if (_animationCoroutine == null)
            {
                return;
            }

            this.StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
        
        private AnimableText GetAnimableText(string inputText)
        {
            if (inputText == string.Empty)
            {
                return new AnimableText();
            }

            var tokens = _tokenizer.Tokenize(inputText);
            _parser.Parse(tokens);
            var animableText = _parser.GetParsedText();
            return animableText;
        }


        private IEnumerator RunAnimationRoutine(TMP_TextInfo textInfo, ICollection<CharAnimInfo> charAnims, IDictionary<int, CaretEffect> caretEffects)
        {
            _caretIndex = 0;
            _skipToEnd = false;
            _caretDelay = defaultCaretDelay;

            OnTypingBegin();

            int charCount = textInfo.characterCount;
            if (charCount == 0)
            {
                OnTypingEnd();
                yield break;
            }

            while (true)
            {
                // Move caret
                if (_caretIndex < charCount)
                {
                    if (_skipToEnd)
                    {
                        _caretIndex = charCount;
                        OnTypingEnd();
                    }
                    else if (ShouldMoveCaret())
                    {
                        OnCaretMove();

                        _caretIndex++;
                        _lastCaretMoveTime = Time.unscaledTime;
                        if (_caretIndex == charCount)
                        {
                            OnTypingEnd();
                        }
                        else
                        {
                            ApplyCaretEffectsForCurrentIndex(caretEffects);
                        }
                    }
                }

                // Apply animation on characters
                foreach (var charAnim in charAnims)
                {
                    // Get refs to update
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[charAnim.CharIndex];
                    int vertexIndex = charInfo.vertexIndex;

                    var materialIndex = charInfo.materialReferenceIndex;
                    Color32[] allColors = textInfo.meshInfo[materialIndex].colors32;
                    Vector3[] allVertices = textInfo.meshInfo[materialIndex].vertices;

                    // Update vertices positions and colors
                    var verticesData = charAnim.GetUpdatedVerticesData(_caretIndex, Time.unscaledTime);

                    for (int i = 0; i < VerticesData.Count; i++)
                    {
                        allVertices[vertexIndex + i] = verticesData.Positions[i];
                        allColors[vertexIndex + i] = verticesData.Colors[i];
                    }
                }

                _targetTextMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32 | TMP_VertexDataUpdateFlags.Vertices);
                yield return null;
            }
        }


        private void ApplyCaretEffectsForCurrentIndex(IDictionary<int, CaretEffect> caretEffects)
        {
            if (!caretEffects.TryGetValue(_caretIndex, out var caretEffect))
            {
                return;
            }

            _lastCaretMoveTime = Time.unscaledTime + caretEffect.Delay;
            _caretDelay = defaultCaretDelay / caretEffect.Speed;

            //Debug.Log($"Applying caret speed {caretEffect.Speed}, resulting in a delay of {_caretDelay}s for each char starting at index {_caretIndex} on char '{_animableText.GetDisplayedText()[_caretIndex]}'.");
        }

        private bool ShouldMoveCaret()
        {
            return Time.unscaledTime > _lastCaretMoveTime + _caretDelay;
        }

        // -- Notifications

        private void OnTypingBegin()
        {
            foreach (var notifiable in _typingNotifiables)
            {
                try
                {
                    notifiable.OnTypingBegin();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void OnCaretMove()
        {
            foreach (var notifiable in _typingNotifiables)
            {
                try
                {
                    notifiable.OnCaretMove();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void OnTypingEnd()
        {
            foreach (var notifiable in _typingNotifiables)
            {
                try
                {
                    notifiable.OnTypingEnd();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
