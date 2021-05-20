using TMPro;
using UnityEngine;

namespace Assets.Scripts.Typewriters
{
    public class TypewriterAnimator : MonoBehaviour
    {
        // -- Editor

        [Header("Parts")]
        public TMP_Text textBox;

        // -- Class

        public void Animate(string textToAnimate)
        {
            textBox.text = textToAnimate;
        }
    }
}
