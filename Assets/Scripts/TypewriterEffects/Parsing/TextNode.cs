using System;

namespace Assets.Scripts.TypewriterEffects.Parsing
{
    internal class TextNode : INode
    {
        public string Text { get; }

        public TextNode(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}