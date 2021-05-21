using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.TypewriterEffects.Parsing;

namespace Assets.Scripts.TypewriterEffects.Anims
{
    public class AnimableText
    {
        private readonly ICollection<INode> _nodes = new List<INode>();
        
        private int _maxCaretPosition;
        
        private readonly Dictionary<int, CaretEffect> _caretEffects = new Dictionary<int, CaretEffect>();
        private readonly Stack<Tuple<int, CharEffect, int>> _charEffects = new Stack<Tuple<int, CharEffect, int>>(); // item1: start of range, item2: effect, item3: end of range

        public void AppendNode(INode node)
        {
            _nodes.Add(node);

            if (node is TextNode textNode)
            {
                _maxCaretPosition += textNode.Text.Length;
            }
            else if (node is RichTextInstructionNode)
            {
                // ignore rich text tags
                return;
            }
            else if (node is SpriteNode)
            {
                // a sprite counts as one character
                _maxCaretPosition++;
            }
            else if (node is BeginCharEffectNode beginEffectNode)
            {
                // add an effect from current max position, to the end of the text
                var effectRange = new Tuple<int, CharEffect, int>(_maxCaretPosition, beginEffectNode.CharEffect, int.MaxValue);
                _charEffects.Push(effectRange);
            }
            else if (node is EndCharEffectNode)
            {
                if (_charEffects.Count == 0)
                {
                    throw new InvalidOperationException($"Unable to mark the end of a character effect at position {_maxCaretPosition}, because no effect began before this point. " +
                                                        $"Did you forget or mispelled an opening tag ({string.Join("/", Enum.GetNames(typeof(CharEffect)))})?");
                    
                }
                // find the last effect range without a defined end
                var invertedStack = new Stack<Tuple<int, CharEffect, int>>();
                var effectRange = _charEffects.Pop();

                while (effectRange.Item3 != int.MaxValue) 
                {
                    invertedStack.Push(effectRange);
                    if (_charEffects.Count == 0)
                    {
                        throw new InvalidOperationException($"Unable to mark the end of a character effect at position {_maxCaretPosition}, as there is not enough effects to close. " +
                                                            $"Last known effect was {effectRange.Item2} and ranged from character index {effectRange.Item1} to character index {effectRange.Item3}. " +
                                                            $"Did you forget or mispelled an opening tag ({string.Join("/", Enum.GetNames(typeof(CharEffect)))})?");
                    }

                    effectRange = _charEffects.Pop();
                }

                // mark the end of the range as the current max position
                var updatedEffectRange = new Tuple<int, CharEffect, int>(effectRange.Item1, effectRange.Item2, _maxCaretPosition);
                _charEffects.Push(updatedEffectRange);

                // put the other ranges back
                while (invertedStack.Count > 0)
                {
                    _charEffects.Push(invertedStack.Pop());
                }
            }
            else
            {
                float delay = 0;
                float speed = 1;

                if (_caretEffects.ContainsKey(_maxCaretPosition))
                {
                    delay = _caretEffects[_maxCaretPosition].Delay;
                    speed = _caretEffects[_maxCaretPosition].Speed;
                }

                switch (node)
                {
                    case SpeedChangeNode speedNode:
                        speed *= speedNode.Speed;
                        break;

                    case PauseNode pauseNode:
                        delay += pauseNode.PauseDuration;
                        break;
                    
                    default:
                        return;
                }

                _caretEffects[_maxCaretPosition] = new CaretEffect(delay, speed);
            }
        }

        public string GetRichText()
        {
            var builder = new StringBuilder();
            foreach (var node in _nodes)
            {
                if (node is TextNode textNode)
                {
                    builder.Append(textNode.Text);
                }
                else if (node is RichTextInstructionNode richTextInstructionNode)
                {
                    builder.Append(richTextInstructionNode.Value);
                }
                else if (node is SpriteNode spriteNode)
                {
                    builder.Append(spriteNode.Value);
                }
                else
                {
                    continue;
                }
            }

            return builder.ToString();
        }

        public string GetDisplayedText()
        {
            var builder = new StringBuilder();
            foreach (var textNode in _nodes.OfType<TextNode>())
            {
                builder.Append(textNode.Text);
            }

            return builder.ToString();
        }

        public IDictionary<int, CaretEffect> GetCaretEffects()
        {
            return _caretEffects;
        }

        public ICollection<CharEffect> GetCharacterEffects(int charIndex)
        {
            List<CharEffect> result = new List<CharEffect>();
            foreach (var effect in _charEffects)
            {
                if (effect.Item1 <= charIndex && charIndex < effect.Item3)
                {
                    // char is in range of the effect
                    result.Add(effect.Item2);
                }
            }

            return result;
        }
    }
}