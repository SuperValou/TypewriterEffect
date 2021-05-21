using UnityEngine;

namespace Assets.Scripts.TypewriterEffects.Anims
{
    public class VerticesData
    {
        public const int Count = 4;
        public Vector3[] Positions { get; } = new Vector3[Count];
        public Color32[] Colors { get; } = new Color32[Count];
    }
}