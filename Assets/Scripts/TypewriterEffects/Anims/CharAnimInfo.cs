using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.TypewriterEffects.Anims
{
    public class CharAnimInfo
    {
        private const float ShakeAmplitude = 6f;
        private const float ShakeFrequency = 25f;

        private const float WaveAmplitude = 0.06f;
        private const float WaveFrequency = 2f*Mathf.PI / 1f; // 2 * PI / time-to-loop 
        private const float WavePhaseShift = -1.1f; // offset between characters
        
        private const float BlinkFrequency = 2f*Mathf.PI / 1.25f; // 2 * PI / time-to-loop 

        private static readonly Color32 EmptyColor = new Color32();

        private readonly ICollection<CharEffect> _charEffects;

        private readonly VerticesData _initialVerticesData;
        private readonly Vector3 _initialCharCenter;
        private readonly float _charScale;

        private readonly float _revealDuration;
        
        private float _revealTime = -1;

        public int CharIndex { get; }
        public int VerticesIndex { get; }

        public CharAnimInfo(int charIndex, ICollection<CharEffect> charEffects, int verticesIndex, float revealDuration, VerticesData initialVerticesData)
        {
            if (revealDuration <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(revealDuration));
            }

            CharIndex = charIndex;
            VerticesIndex = verticesIndex;

            _charEffects = charEffects;
            _revealDuration = revealDuration;
            _initialVerticesData = initialVerticesData;
            _initialCharCenter = (_initialVerticesData.Positions[1] + _initialVerticesData.Positions[3]) / 2;
            _charScale = Vector3.Distance(_initialVerticesData.Positions[1], _initialVerticesData.Positions[3]);
        }

        public VerticesData GetUpdatedVerticesData(int caretIndex, float time)
        {
            var data = new VerticesData();
            if (caretIndex <= CharIndex)
            {
                for (int i = 0; i < VerticesData.Count; i++)
                {
                    data.Positions[i] = _initialCharCenter;
                    data.Colors[i] = EmptyColor;
                }

                return data;
            }

            if (_revealTime < 0)
            {
                // Caret just moved over this char
                _revealTime = Time.unscaledTime;
            }

            // Compute character scale
            float charSize = 0;
            if (_revealTime >= 0)
            {
                float elapsedTimeSinceReveal = Time.unscaledTime - _revealTime;
                charSize = Mathf.Min(elapsedTimeSinceReveal / _revealDuration, 1);
            }

            // Compute character displacement due to animation
            Vector3 effectDisplacement = GetEffectsDisplacement(time);

            // Rescale character and adjust its position from its animation
            bool shouldBlink = _charEffects.Contains(CharEffect.Blink);
            for (int i = 0; i < VerticesData.Count; i++)
            {
                data.Positions[i] = (_initialVerticesData.Positions[i] - _initialCharCenter) * charSize + _initialCharCenter + effectDisplacement;
                var color = _initialVerticesData.Colors[i];
                if (shouldBlink)
                {
                    data.Colors[i] = GetColorShift(color, time);
                }
                else
                {
                    data.Colors[i] = color;
                }
            }

            return data;
        }

        private Color32 GetColorShift(Color32 color, float time)
        {
            float alpha = color.a * ((Mathf.Sin(BlinkFrequency * time) + 1f) / 2f); // make the alpha alternate between 0 and the initial alpha
            var shiftedColor = new Color32(color.r, color.g, color.b, (byte) alpha);
            return shiftedColor;
        }

        private Vector3 GetEffectsDisplacement(float time)
        {
            float x = 0;
            float y = 0;

            foreach (CharEffect charEffect in _charEffects)
            {
                switch (charEffect)
                {
                    case CharEffect.Shake:
                        x += ShakeAmplitude * (Mathf.PerlinNoise(ShakeFrequency * time, 0) - 0.5f);
                        y += ShakeAmplitude * (Mathf.PerlinNoise(0, ShakeFrequency * time) - 0.5f);
                        break;
                        
                    case CharEffect.Wave:
                        float phaseShift = CharIndex * WavePhaseShift;
                        float amplitude = WaveAmplitude * _charScale;
                        y += amplitude * Mathf.Sin(WaveFrequency * (time + phaseShift));
                        break;

                    case CharEffect.Blink:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new Vector3(x, y, 0);
        }
    }
}