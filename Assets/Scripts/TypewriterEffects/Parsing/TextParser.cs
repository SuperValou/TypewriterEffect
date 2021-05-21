using System;
using System.Collections.Generic;
using Assets.Scripts.TypewriterEffects.Anims;
using Assets.Scripts.TypewriterEffects.Tokens;

namespace Assets.Scripts.TypewriterEffects.Parsing
{
    public class TextParser
    {
        private Queue<Token> _remainingTokens;
        private Token _lastToken;

        private AnimableText _parsedAnimableText = null;

        private static readonly Dictionary<string, float> Pauses = new Dictionary<string, float>{
            { "short", 0.5f },
            { "long", 1f },
        };

        private static readonly Dictionary<string, float> Speeds = new Dictionary<string, float>{
            { "slow", 0.1f },
            { "normal", 1f },
            { "fast", 4f }
        };
        
        public void Parse(ICollection<Token> tokens)
        {
            _remainingTokens = new Queue<Token>(tokens);

            _lastToken = new Token(TokenType.ClosingTag, string.Empty); // to start the parser
            BuildAst();
        }

        private void BuildAst()
        {
            _parsedAnimableText = new AnimableText();
            while (_remainingTokens.Count > 0)
            {
                Token currentToken = _remainingTokens.Dequeue();

                var expectedTokenTypes = GetExpectedFollowingTokens(_lastToken.Type);
                if (!expectedTokenTypes.Contains(currentToken.Type))
                {
                    throw new InvalidOperationException($"Unexpected token '{currentToken}' after '{_lastToken}'. {string.Join(" or ", expectedTokenTypes)} was expected instead.");
                }

                switch (currentToken.Type)
                {
                    case TokenType.Value:
                        INode node;
                        if (_lastToken.Type == TokenType.PauseTag)
                        {
                            if (!Pauses.ContainsKey(currentToken.Value))
                            {
                                throw new InvalidOperationException($"Unknown pause value '{currentToken.Value}'. Allowed pauses are: {string.Join(", ", Pauses.Keys)}.");
                            }

                            float pauseDuration = Pauses[currentToken.Value];
                            node = new PauseNode(pauseDuration);
                            _parsedAnimableText.AppendNode(node);
                        }
                        else if (_lastToken.Type == TokenType.SpeedChangeTag)
                        {
                            if (!Speeds.ContainsKey(currentToken.Value))
                            {
                                throw new InvalidOperationException($"Unknown speed value '{currentToken.Value}'. Allowed speeds are: {string.Join(", ", Speeds.Keys)}.");
                            }

                            float speed = Speeds[currentToken.Value];
                            node = new SpeedChangeNode(speed);
                            _parsedAnimableText.AppendNode(node);
                        }
                        else if (_lastToken.Type == TokenType.AnimStartTag)
                        {
                            if (!Enum.TryParse(currentToken.Value, ignoreCase: true, out CharEffect charEffect)) 
                            {
                                throw new InvalidOperationException($"Unknown effect '{currentToken.Value}'. Allowed effect are: {string.Join(", ", Enum.GetNames(typeof(CharEffect)))}.");
                            }

                            node = new BeginCharEffectNode(charEffect);
                            _parsedAnimableText.AppendNode(node);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Incoherent token type: last token was {_lastToken} and current one is {currentToken}.");
                        }
                        break;

                    case TokenType.OpeningTag:
                    case TokenType.ClosingTag:
                    case TokenType.PauseTag:
                    case TokenType.SpeedChangeTag:
                    case TokenType.AnimStartTag:
                        break;
                        
                    case TokenType.RawText:
                        INode textNode = new TextNode(currentToken.Value);
                        _parsedAnimableText.AppendNode(textNode);
                        break;

                    case TokenType.RichTextInstruction:
                        INode richTextNode = new RichTextInstructionNode(currentToken.Value);
                        _parsedAnimableText.AppendNode(richTextNode);
                        break;

                    case TokenType.SpriteInstruction:
                        INode spriteNode = new SpriteNode(currentToken.Value);
                        _parsedAnimableText.AppendNode(spriteNode);
                        break;

                    case TokenType.AnimEndTag:
                        INode endCharEffectNode = new EndCharEffectNode();
                        _parsedAnimableText.AppendNode(endCharEffectNode);
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown token type for {currentToken}");
                }

                _lastToken = currentToken;
            }
        }

        public AnimableText GetParsedText()
        {
            if (_parsedAnimableText == null)
            {
                throw new InvalidOperationException($"Nothing was parsed beforehand. Did you forget to call the {nameof(Parse)} method?");
            }

            return _parsedAnimableText;
        }

        private static ICollection<TokenType> GetExpectedFollowingTokens(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.RawText:
                case TokenType.ClosingTag:
                case TokenType.RichTextInstruction:
                case TokenType.SpriteInstruction:
                case TokenType.AnimEndTag:
                    return new List<TokenType> { TokenType.OpeningTag, TokenType.RawText, TokenType.RichTextInstruction, TokenType.SpriteInstruction, TokenType.AnimEndTag };

                case TokenType.OpeningTag:
                    return new List<TokenType> { TokenType.PauseTag, TokenType.SpeedChangeTag, TokenType.AnimStartTag };

                case TokenType.PauseTag:
                case TokenType.SpeedChangeTag:
                case TokenType.AnimStartTag:
                    return new List<TokenType> { TokenType.Value };

                case TokenType.Value:
                    return new List<TokenType> { TokenType.ClosingTag };

                default:
                    throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, $"Unknown token type {tokenType}");
            }
        }
    }
}