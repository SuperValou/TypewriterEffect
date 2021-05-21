using Assets.Scripts.TypewriterEffects;
using UnityEngine;

namespace Assets.Scripts.SceneScripts
{
    public class DemoScript : MonoBehaviour
    {
        // -- Editor

        [TextArea(minLines:5, maxLines:20)]
        public string textToAnimate = "Hello world";

        public TypewriterAnimator typewriterAnimator;

        // -- Class

        void Start()
        {
            typewriterAnimator.Animate(textToAnimate);
        }
    }
}
